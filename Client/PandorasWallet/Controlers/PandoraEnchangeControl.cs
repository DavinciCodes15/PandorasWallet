//   Copyright 2017-2019 Davinci Codes
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Also use the software for non-commercial purposes.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
using Pandora.Client.ClientLib;
using Pandora.Client.ClientLib.Contracts;
using Pandora.Client.Exchange;
using Pandora.Client.Exchange.Exchanges;
using Pandora.Client.Exchange.Factories;
using Pandora.Client.Exchange.Objects;
using Pandora.Client.Exchange.SaveManagers;
using Pandora.Client.PandorasWallet.Dialogs;
using Pandora.Client.PandorasWallet.Wallet;
using Pandora.Client.Universal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet.Controlers
{
    public class PandoraEnchangeControl : IDisposable
    {
        private const decimal FExchangeTradeFeePercent = 0.0025M;
        //private PandoraExchanger FExchanger;

        private PandoraExchangeFactoryProducer FExchangeFactoryProducer;
        private ExchangeOrderManager FOrderManager;

        private ConcurrentBag<ExchangeMarket> FExchangeCoinMarkets;
        private LoginExchanger FExchangeLogin;
        private CancellationTokenSource FExchangeTaskCancellationSource;
        private ExchangeMarket FExchangeSelectedCoin;
        private PandoraExchangeSQLiteSaveManager FDBExchanger;
        private string FLastExchangeSelectedCurrency = "";
        private PandoraClientControl FPandoraClientControl;

        private Dictionary<string, UserTradeOrder> FCacheLastOrders = new Dictionary<string, UserTradeOrder>();
        private Task FDataLoadingTask;
        private IPandoraExchanger FCurrentExchanger;
        private AvailableExchangesList FSelectedExchange;

        private ConcurrentDictionary<string, ExchangeStageOperator> FStageOperators;

        private ServerAccess.ServerConnection FPandorasWalletConnection
        {
            get;
            set;
        }

        private bool FDisableUpdating;

        private readonly object FExchangeInterfaceLock = new object();
        private ExchangeKeyValueHelper<MultiExchangeKeyValueObject> FKeyValueHelper;

        internal delegate bool GetKeyManagerDelegate(out KeyManager aKeyManagar);

        internal GetKeyManagerDelegate GetKeyManagerMethod { get; set; }
        internal Func<bool> SetKeyManagerPassword { get; set; }

        public PandoraEnchangeControl(ServerAccess.ServerConnection aPandorasWalletConnection)
        {
            FPandoraClientControl = PandoraClientControl.GetExistingInstance();
            FPandorasWalletConnection = aPandorasWalletConnection;
            MainForm = PandoraClientControl.GetExistingInstance().AppMainForm;
            DisableExchangeInterface();
            ExchangeInitialize();
        }

        public long ActiveCurrencyID { get; private set; }
        public AppMainForm MainForm { get; private set; }

        public string SqLiteDbFileName => PandoraExchangeSQLiteSaveManager.BuildPath(PandoraClientControl.GetExistingInstance().Settings.DataPath, FPandorasWalletConnection.InstanceId);

        public void ValidLocalDBFile(string aExchangeFilePath)
        {
            var lDBManager = PandoraExchangeSQLiteSaveManager.GetReadOnlyInstance(aExchangeFilePath);
            lDBManager.GetUserData(out string lFileUsername, out string lFileEmail);
            if (FPandorasWalletConnection.UserName != lFileUsername || FPandorasWalletConnection.Email != lFileEmail)
                throw new Exception($"The file does not belong to user {FPandorasWalletConnection.UserName} ({FPandorasWalletConnection.Email}).");
        }

        public void BeginBackupRestore(out string lDBCopyFileName)
        {
            lDBCopyFileName = null;
            if (Started)
            {
                StopStageOperators();
                lDBCopyFileName = FDBExchanger.CreateDBFileCopy();
            }
        }

        public void EndBackupRestore(string lDBCopyFileName)
        {
            if (Started)
                StartStageOperators();
            else
                StartProcess();
            if (!string.IsNullOrEmpty(lDBCopyFileName) && File.Exists(lDBCopyFileName))
                File.Delete(lDBCopyFileName);
        }

        private void StartStageOperators()
        {
            foreach (var lOperator in FStageOperators.Values)
                lOperator.Start();
        }

        private void StopStageOperators()
        {
            foreach (var lOperator in FStageOperators.Values)
                lOperator.Stop();
        }

        private void SetStageOperator(IPandoraExchanger aExchanger)
        {
            if (FStageOperators == null)
                FStageOperators = new ConcurrentDictionary<string, ExchangeStageOperator>();
            if (!FStageOperators.ContainsKey(aExchanger.UID))
            {
                var lOperator = new ExchangeStageOperator(aExchanger, FOrderManager, FPandorasWalletConnection);
                lOperator.OnTransferCoinsNeeded += TryToTransferMoneyToExchange;
                FStageOperators.TryAdd(aExchanger.UID, lOperator);
                aExchanger.OnMarketPricesChanging += FExchanger_OnMarketPricesChanging;
            }
        }

        public bool Started { get; private set; }

        public void StartProcess()
        {
            if (Started) return;
            try
            {
                SetupDBSaveManager();
                FExchangeTaskCancellationSource = new CancellationTokenSource();
                FKeyValueHelper = new ExchangeKeyValueHelper<MultiExchangeKeyValueObject>(FDBExchanger);
                var lFactory = GetCurrentExchangeFactory();
                foreach (var lExchangeID in FExchangeFactoryProducer.Inventory.Keys)
                    SetStageOperator(lFactory.GetPandoraExchange((AvailableExchangesList) lExchangeID));
                StartStageOperators();
                //Add exchanges to list
                MainForm.BeginInvoke((Action) (() =>
                {
                    var lExchangeItems = FExchangeFactoryProducer.Inventory
                        .Select(lExchange => new AppMainForm.ExchangeItem
                        {
                            ID = lExchange.Key,
                            Name = lExchange.Value
                        }
                    );
                    MainForm.ClearExchangeList();
                    MainForm.AddExchange(lExchangeItems);
                }));

                FDataLoadingTask = Task.Run(() =>
                {
                    foreach (var lCurrencyItem in GetUserDisplayedCurrencies())
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine($"Now loading order data for {lCurrencyItem.Name}");
#endif
                        LoadNewCurrencyData(lCurrencyItem);
                    }
                }, FExchangeTaskCancellationSource.Token);
                Started = true;
            }
            catch (AggregateException ex)
            {
                var lInnerException = ex.InnerException;
                StopProcess();
                throw lInnerException;
            }
            catch (Exception ex)
            {
                StopProcess();
                throw;
            }
        }

        public void LoadNewCurrencyData(ICurrencyIdentity aCurrencyItem)
        {
            var lCurrencyOrders = FOrderManager.LoadOrders(aCurrencyItem.Ticker, aCurrencyItem.Id);
            var lExchangeFactory = GetCurrentExchangeFactory();
            var lMarketCache = new Dictionary<int, IEnumerable<ExchangeMarket>>();
            foreach (var lOrder in lCurrencyOrders)
            {
                var lExchanger = lExchangeFactory.GetPandoraExchange((AvailableExchangesList) lOrder.PandoraExchangeID);
                if (!lMarketCache.TryGetValue(lOrder.PandoraExchangeID, out IEnumerable<ExchangeMarket> lMarkets))
                {
                    lMarkets = lExchanger.GetMarketCoins(aCurrencyItem.Name, aCurrencyItem.Ticker, GetCurrencyIDFromIdentifier);
                    lMarketCache.Add(lOrder.PandoraExchangeID, lMarkets);
                }
                ExchangeMarket lMarket = lMarkets.Where(lMarketElement => string.Equals(lMarketElement.MarketName, lOrder.ExchangeMarketName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (lMarket != null)
                    AddCurrencyOrderToGUI(aCurrencyItem.Id, lMarket, lOrder, false, lExchanger);
                else
                    Log.Write(LogLevel.Warning, $"No market found for order {lOrder.InternalID} - {lOrder.Name} of market {lOrder.ExchangeMarketName} at exchange {((AvailableExchangesList) lOrder.PandoraExchangeID).ToString()}. To avoid exceptions it will not load");
            }
        }

        private IEnumerable<ICurrencyIdentity> GetUserDisplayedCurrencies()
        {
            var lResult = new List<ICurrencyIdentity>();
            lResult.AddRange(FPandorasWalletConnection.GetDisplayedCurrencies().Cast<ICurrencyIdentity>());
            lResult.AddRange(FPandorasWalletConnection.GetDisplayedCurrencyTokens().Where(lToken => lToken.ParentCurrencyID == 10194).Cast<ICurrencyIdentity>()); //Currently we are going to only support with tokens based on ethereum
            //TODO: Add an identifier to choose which coins are good to be tradable
            return lResult;
        }

        private IPandoraExchangeFactory GetCurrentExchangeFactory()
        {
            return FExchangeFactoryProducer.GetExchangeFactory(FPandorasWalletConnection.UserName, FPandorasWalletConnection.Email, 0);
        }

        private long? GetCurrencyIDFromIdentifier(string aCurrencyName, string aTicker)
        {
            return GetCurrencyFromIdentifier(aCurrencyName, aTicker)?.Id;
        }

        private ICurrencyIdentity GetCurrencyFromIdentifier(string aCurrencyName, string aTicker)
        {
            ICurrencyIdentity lResult = GetUserDisplayedCurrencies().Where(lCoin => string.Equals(lCoin.Ticker, aTicker, StringComparison.OrdinalIgnoreCase) || string.Equals(lCoin.Name, aCurrencyName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            return lResult;
        }

        public void SaveRestoredKeyAndSecret(string aEncryptedKey, string aEncryptedSecret)
        {
            var lKeyValueObject = FKeyValueHelper.LoadKeyValues(0);
            FKeyValueHelper.SaveChanges(lKeyValueObject);
        }

        private void ExchangeInitialize()
        {
            FExchangeLogin = new LoginExchanger();

            FExchangeFactoryProducer = PandoraExchangeFactoryProducer.GetInstance();
            FExchangeCoinMarkets = new ConcurrentBag<ExchangeMarket>();

            MainForm.OnSelectedCurrencyChanged += MainForm_ExchangeOnSelectedCurrencyChanged; ;
            MainForm.OnExchangeSelectedCurrencyChanged += MainForm_OnExhangeMarketSelectionChanged;
            MainForm.OnLabelEstimatePriceClick += MainForm_OnLabelEstimatePriceClick;
            MainForm.OnExchangeSelectionChanged += MainForm_OnExchangeSelectionChanged;
            MainForm.OnChangeExchangeKeysBtnClick += MainForm_OnChangeExchangeKeysBtnClick;

            MainForm.OnExchangeQuantityChanged += MainForm_OnExchangeQuantityChanged;
            MainForm.OnExchangeBtnClick += MainForm_OnExchangeBtnClick;
            MainForm.OnPriceChanged += MainForm_OnPriceChanged; ;
            //MainForm.OnOrderHistorySelectionChanged += MainForm_OnOrderHistorySelectionChanged;
            MainForm.OnCancelBtnClick += MainForm_OnCancelBtnClick;

            MainForm.OnExchangeReceivedChanged += MainForm_OnExchangeReceivedChanged;
            MainForm.OnCheckAllOrderHistory += MainForm_OnCheckAllOrderHistory;
        }

        private void MainForm_OnChangeExchangeKeysBtnClick(object sender, EventArgs e)
        {
            if (GetKeyManagerMethod.Invoke(out KeyManager lKeyManager))
            {
                var lKeyValueObject = FKeyValueHelper.LoadKeyValues(0); //WHERE PROFILE 0 IS THE DEFAULT AND INITIAL ONE
                AskUserForExchangeCredentials(lKeyValueObject, lKeyManager, FSelectedExchange);
            }
        }

        private void MainForm_OnCancelBtnClick(int aInternalOrderId)
        {
            UserTradeOrder lOrder = FOrderManager.GetOrderByInternalID(aInternalOrderId);
            if (lOrder == null) throw new Exception("Order id not found");
            var lWarningString = lOrder.Status == OrderStatus.Completed ? "WARNING!: Cancelling completed orders will cause to prevent withdraw of traded coins." : string.Empty;
            if (!MainForm.AskUserDialog($"Cancel order {lOrder.Name}", $"Do you want to cancel selected order? {Environment.NewLine} {lWarningString}")) return;
            bool lWithdraw = false;
            if (lOrder.Status == OrderStatus.Placed || lOrder.Status == OrderStatus.Waiting)
                lWithdraw = AskUserToWithdrawFunds(lOrder.Name);
            FOrderManager.WriteTransactionLogEntry(lOrder, OrderMessage.OrderMessageLevel.Info, string.Format("Order cancellation requested by user. Cancelling..."));
            ExchangeStageOperator.MarkOrderToCancel(lOrder.InternalID, lWithdraw);
        }

        private bool AskUserToWithdrawFunds(string aOrderName)
        {
            return MainForm.AskUserDialog("Coin withdraw", $"Do you want to withdraw the order '{aOrderName}' funds from exchange? {Environment.NewLine} Note: Please consider that a tx fee will apply for this operation.");
        }

        private void MainForm_ExchangeOnSelectedCurrencyChanged(object sender, EventArgs e)
        {
            ActiveCurrencyID = MainForm.SelectedCurrencyId;
            Task.Run(() => ExchangePandoraCurrencyChanged(ActiveCurrencyID), FExchangeTaskCancellationSource?.Token ?? default);
        }

        private void ExchangePandoraCurrencyChanged(long aCurrency)
        {
            FDisableUpdating = true;
            try
            {
                PandoraCurrencyChangedProcess(aCurrency);
            }
            catch (Exception ex)
            {
                Log.Write(LogLevel.Error, $"Exception thrown at ExchangePandoraCurrencyChanged. Details: {ex}");
                MainForm.BeginInvoke((Action) (() => MainForm.StandardExceptionMsgBox(ex, "Exception thrown at changing exchange currency market info")));
            }
            finally
            {
                FDisableUpdating = false;
            }
        }

        /// <summary>
        /// Looks for markets and prices that the user is able to use for the selected currency ticker
        /// </summary>
        /// <param name="aCurrencyTicker">Selected currency ticker</param>
        /// <returns>Tuple array where first element is a currency item with market and second is current price market</returns>
        private IEnumerable<Tuple<ICurrencyIdentity, decimal>> GetCoinsWithExchangePrice(string aName, string aTicker)
        {
            var lUserPossibleMarkets = new List<ExchangeMarket>();
            ExchangeMarket[] lFullMarkets = FCurrentExchanger.GetMarketCoins(aName, aTicker, GetCurrencyIDFromIdentifier);
            List<Tuple<ICurrencyIdentity, decimal>> lCoinsWithMarket = new List<Tuple<ICurrencyIdentity, decimal>>();
            var lUserCoins = GetUserDisplayedCurrencies();
            foreach (ExchangeMarket lMarket in lFullMarkets)
            {
                ICurrencyIdentity lCoin = lUserCoins.FirstOrDefault(lItem => (lMarket.DestinationCurrencyInfo.WalletID.HasValue && lItem.Id == lMarket.DestinationCurrencyInfo.WalletID)
                                                                                                    || string.Equals(lItem.Ticker, lMarket.DestinationCurrencyInfo.Ticker, StringComparison.OrdinalIgnoreCase)
                                                                                                    || string.Equals(lItem.Name, lMarket.DestinationCurrencyInfo.Name, StringComparison.OrdinalIgnoreCase));
                if (lCoin != null)
                {
                    lCoinsWithMarket.Add(new Tuple<ICurrencyIdentity, decimal>(lCoin, lMarket.IsSell ? lMarket.Prices.Bid : lMarket.Prices.Ask));
                    lUserPossibleMarkets.Add(lMarket);
                }
            }
            Interlocked.Exchange(ref FExchangeCoinMarkets, new ConcurrentBag<ExchangeMarket>(lUserPossibleMarkets));
            return lCoinsWithMarket;
        }

        /// <summary> 
        /// Triggers with event OnCurrencyItemSelectionChanged. It will generate his own thread 
        /// </summary> 
        /// <param name="aCurrency">Selected currency id</param> 
        private void PandoraCurrencyChangedProcess(long aCurrency)
        {
            MainForm.BeginInvoke(new Action(() =>
            {
                MainForm.ClearListExchangeTo();
                MainForm.StatusControlExchange.ClearStatusList();
                MainForm.StatusControlOrderHistory.ClearStatusList();
            }));

            //added condition before to last conditional if;  unexpected null issue removed since first execution of PW without exchange's credentials. 
            if ((bool) MainForm.Invoke((Func<bool>) (() => MainForm.SelectedExchange == null)) || FCurrentExchanger==null|| !FCurrentExchanger.IsCredentialsSet )
            return;

            ICurrencyIdentity lCurrency = FPandorasWalletConnection.GetCurrency(aCurrency);
            if (lCurrency == null)
                lCurrency = FPandorasWalletConnection.GetCurrencyToken(aCurrency) ?? throw new Exception($"No existing currency with id {aCurrency} found on the wallet");
            var lCoinsWithMarket = GetCoinsWithExchangePrice(lCurrency.Name, lCurrency.Ticker);

            foreach (var lCurrencyItemWithPrice in lCoinsWithMarket)
            {
                var lCurrencyItem = lCurrencyItemWithPrice.Item1;
                decimal lPrice = lCurrencyItemWithPrice.Item2;
                MainForm.BeginInvoke(new Action(() => MainForm.AddCoinExchangeTo(lCurrencyItem.Id, lCurrencyItem.Name, lCurrencyItem.Ticker, lPrice)));
            }
            if (!FExchangeCoinMarkets.Any())
                FExchangeSelectedCoin = null;
            DisableExchangeInterface();
            LoadOrderAcordingToAllCheckbox(aCurrency);
            FLastExchangeSelectedCurrency = string.Empty;
            MainForm.BeginInvoke(new Action(() => MainForm.TickerQuantity = MainForm.SelectedCurrency.Ticker));
        }

        private void LoadOrderAcordingToAllCheckbox(long aCurrencyID)
        {
            MainForm.BeginInvoke(new MethodInvoker(() =>
            {
                if (MainForm.AllOrderHistoryChecked)
                    MainForm.LoadAllCurrencyExchangeOrders();
                else
                    MainForm.LoadCurrencyExchangeOrders(aCurrencyID);
            }));
        }

        private void MainForm_OnExchangeBtnClick(object sender, EventArgs e)
        {
            decimal lAmountTrade = MainForm.ExchangeQuantity;
            if (MainForm.ExchangeTargetPrice <= 0 || MainForm.ExchangeStopPrice <= 0)
                throw new ClientExceptions.InvalidOperationException("Invalid target or stop price amount.");
            bool lMinimumTradeDirection = FExchangeSelectedCoin.MinTradeIsBaseCurrency ^ FExchangeSelectedCoin.IsSell;
            if (MainForm.ExchangeQuantity == 0 && MainForm.ExchangeTotalReceived > 0)
                MainForm_OnExchangeReceivedChanged();
            if (MainForm.ExchangeTotalReceived == 0 && MainForm.ExchangeQuantity > 0)
                MainForm_OnExchangeQuantityChanged();
            decimal lAmounTradeSize = lMinimumTradeDirection ? MainForm.ExchangeQuantity : MainForm.ExchangeTotalReceived;
            if (lAmounTradeSize <= 0)
                if (FExchangeSelectedCoin.MinimumTrade >= (lAmounTradeSize))
                    throw new ClientExceptions.InvalidOperationException(string.Format("Minimum order trade size is {0} {1}", FExchangeSelectedCoin.MinimumTrade, lMinimumTradeDirection ? FExchangeSelectedCoin.BaseCurrencyInfo.Name : FExchangeSelectedCoin.DestinationCurrencyInfo.Name));
            if (lAmountTrade > FPandoraClientControl.GetBalance(ActiveCurrencyID))
                throw new ClientExceptions.InvalidOperationException(string.Format("Not enough balance for this trade size"));
            TryToCreateNewExchangeTransaction(MainForm.ExchangeTargetPrice, MainForm.ExchangeStopPrice, MainForm.ExchangeQuantity, FExchangeSelectedCoin);
        }

        private void SetGUILastTransaction(UserTradeOrder aOrder)
        {
            if (aOrder == null)
                MainForm.BeginInvoke(new MethodInvoker(() =>
                {
                    MainForm.StatusControlExchange.StatusName = "No exchange market orders done yet.";
                    MainForm.StatusControlExchange.ClearStatusList();
                }));
            else
                MainForm.BeginInvoke(new MethodInvoker(() =>
                {
                    MainForm.StatusControlExchange.StatusName = aOrder.Name;
                    MainForm.SetExchangeLastOrder(aOrder.InternalID);
                }));
        }

        private void MainForm_OnPriceChanged()
        {
            MainForm.OnExchangeReceivedChanged -= MainForm_OnExchangeReceivedChanged;
            if (MainForm.ExchangeTargetPrice > 0 && MainForm.ExchangeQuantity > 0)
                UpdateGUIExchangeAproxReceived();
            MainForm.OnExchangeReceivedChanged += MainForm_OnExchangeReceivedChanged;
        }

        private void UpdateGUIExchangeAproxReceived()
        {
            var lCurrencyItem = FPandoraClientControl.GetCurrency(FExchangeSelectedCoin.BaseCurrencyInfo.WalletID.Value);
            decimal lTotal = Math.Round(MainForm.ExchangeQuantity / (FExchangeSelectedCoin.IsSell ? 1 / MainForm.ExchangeTargetPrice : MainForm.ExchangeTargetPrice), lCurrencyItem.Precision);
            decimal lComision = lTotal * FExchangeTradeFeePercent;
            MainForm.ExchangeTotalReceived = Math.Round(lTotal - lComision, lCurrencyItem.Precision);
        }

        private void UpdateGUIExchangeSpend()
        {
            var lCurrencyItem = FPandoraClientControl.GetCurrency(FExchangeSelectedCoin.BaseCurrencyInfo.WalletID.Value);
            decimal lTotal = Math.Round(MainForm.ExchangeTotalReceived * (FExchangeSelectedCoin.IsSell ? 1 / MainForm.ExchangeTargetPrice : MainForm.ExchangeTargetPrice), lCurrencyItem.Precision);
            decimal lComision = lTotal * FExchangeTradeFeePercent;
            MainForm.ExchangeQuantity = Math.Round(lTotal - lComision, lCurrencyItem.Precision);
        }

        private void MainForm_OnExchangeQuantityChanged()
        {
            //I need to disable the other event to prevent a stack overflow exception
            MainForm.OnExchangeReceivedChanged -= MainForm_OnExchangeReceivedChanged;
            if (MainForm.ExchangeTargetPrice > 0 && MainForm.ExchangeQuantity > 0)
                UpdateGUIExchangeAproxReceived();
            MainForm.OnExchangeReceivedChanged += MainForm_OnExchangeReceivedChanged;
        }

        private void MainForm_OnExchangeReceivedChanged()
        {
            MainForm.OnExchangeQuantityChanged -= MainForm_OnExchangeQuantityChanged;
            if (MainForm.ExchangeTotalReceived > 0 && MainForm.ExchangeTargetPrice > 0)
                UpdateGUIExchangeSpend();
            MainForm.OnExchangeQuantityChanged += MainForm_OnExchangeQuantityChanged;
        }

        private void MainForm_OnCheckAllOrderHistory(object sender, EventArgs e)
        {
            LoadOrderAcordingToAllCheckbox(ActiveCurrencyID);
        }

        private void MainForm_OnLabelEstimatePriceClick(object sender, EventArgs e)
        {
            MainForm.ExchangeTargetPrice = MainForm.EstimatePrice;
        }

        private void AskUserForExchangeCredentials(MultiExchangeKeyValueObject aKeyValueObject, KeyManager aKeyManager, AvailableExchangesList aSelectedExchange)
        {
            if (FExchangeLogin.Execute())
                ChangeUserExchangeCredentials(aKeyValueObject, aKeyManager, aSelectedExchange, FExchangeLogin.ExchageKey, FExchangeLogin.ExchangeSecret);
        }

        private void ChangeUserExchangeCredentials(MultiExchangeKeyValueObject aKeyValueObject, KeyManager aKeyManager, AvailableExchangesList aSelectedExchange, string aExchangeKey, string aExchangeSecret)
        {
            SetExchangeCredentials(aSelectedExchange, aExchangeKey, aExchangeSecret);
            int lSelectedExchangeInt = (int) aSelectedExchange;

            if (!aKeyManager.IsPasswordSet)
                SetKeyManagerPassword.Invoke();
            if (!aKeyValueObject.PublicKeys.ContainsKey(lSelectedExchangeInt))
                aKeyValueObject.PublicKeys.Add(lSelectedExchangeInt, aKeyManager.EncryptText(aExchangeKey));
            else
                aKeyValueObject.PublicKeys[lSelectedExchangeInt] = aKeyManager.EncryptText(aExchangeKey);

            if (!aKeyValueObject.PrivateKeys.ContainsKey(lSelectedExchangeInt))
                aKeyValueObject.PrivateKeys.Add(lSelectedExchangeInt, aKeyManager.EncryptText(aExchangeSecret));
            else
                aKeyValueObject.PrivateKeys[lSelectedExchangeInt] = aKeyManager.EncryptText(aExchangeSecret);

            FKeyValueHelper.SaveChanges(aKeyValueObject);
        }

        private Tuple<string, string> TryDecryptAndSetExchangeCredentials(KeyManager aKeyManager, string aEncryptedPublicKey, string aEncryptedPrivateKey, AvailableExchangesList aSelectedExchange)
        {
            string lExchangePublicKey = null;
            string lExchangeSecret = null;
            bool lPasswordSet = aKeyManager.IsPasswordSet;
            if (!lPasswordSet)
                lPasswordSet = SetKeyManagerPassword.Invoke();
            if (lPasswordSet)
            {
                lExchangePublicKey = aKeyManager.DecryptText(aEncryptedPublicKey);
                lExchangeSecret = aKeyManager.DecryptText(aEncryptedPrivateKey);
                SetExchangeCredentials(aSelectedExchange, lExchangePublicKey, lExchangeSecret);
            }
            else throw new ClientExceptions.CancelledUserOperation("No password provided");
            return new Tuple<string, string>(lExchangePublicKey, lExchangeSecret);
        }

        private void MainForm_OnExchangeSelectionChanged(object sender, EventArgs e)
        {
            if (MainForm.SelectedExchange == null)
            {
                if (MainForm.IsHandleCreated)
                    MainForm.BeginInvoke(new MethodInvoker(() =>
                    {
                        DisableExchangeInterface();
                        MainForm.ClearListExchangeTo();
                        MainForm.ClearOrderHistory();
                        MainForm.StatusControlExchange.StatusName = string.Empty;
                    }));
                FSelectedExchange = 0;
                return;
            }
            FSelectedExchange = (AvailableExchangesList) MainForm.SelectedExchange.ID;
            var lExchangeFactory = GetCurrentExchangeFactory();
            var lExchanger = lExchangeFactory.GetPandoraExchange(FSelectedExchange);

            try
            {
                //In case that when the user loaded their info, we should try again every time that it clicks in the combo box
                if (!Started)
                {
                    StartProcess();
                    MainForm.ChangeExchangeSelection(null);
                    return;
                }

                if (!lExchanger.IsCredentialsSet)
                {
                    if (GetKeyManagerMethod.Invoke(out KeyManager lKeyManager))
                    {
                        var lSelectedExchange = (int) FSelectedExchange;
                        var lKeyValueObject = FKeyValueHelper.LoadKeyValues(0); //WHERE PROFILE 0 IS THE DEFAULT AND INITIAL ONE
                        try
                        {
                            if (lKeyValueObject.PublicKeys.ContainsKey(lSelectedExchange) && lKeyValueObject.PrivateKeys.ContainsKey(lSelectedExchange))
                            {
                                TryDecryptAndSetExchangeCredentials(lKeyManager, lKeyValueObject.PublicKeys[lSelectedExchange], lKeyValueObject.PrivateKeys[lSelectedExchange], FSelectedExchange);
                                return;
                            }
                            else if (FSelectedExchange == AvailableExchangesList.Bittrex)
                            {
                                var lOldKeyValueHelper = new ExchangeKeyValueHelper<ExchangeKeyValueObject>(FDBExchanger);
                                var lOldKeyValues = lOldKeyValueHelper.LoadKeyValues(0);
                                if (!string.IsNullOrEmpty(lOldKeyValues.PublicKey) && !string.IsNullOrEmpty(lOldKeyValues.PrivateKey))
                                {
                                    var lUncryptedKeys = TryDecryptAndSetExchangeCredentials(lKeyManager, lOldKeyValues.PublicKey, lOldKeyValues.PrivateKey, FSelectedExchange);
                                    ChangeUserExchangeCredentials(lKeyValueObject, lKeyManager, FSelectedExchange, lUncryptedKeys.Item1, lUncryptedKeys.Item2);
                                    lOldKeyValueHelper.ClearKeys(0);
                                    return;
                                }
                            }
                        }
                        catch (ClientExceptions.CancelledUserOperation)
                        {
                            return;
                        }
                        catch (PandoraExchangeExceptions.InvalidExchangeCredentials ex)
                        {
                            Log.Write(LogLevel.Warning, $"Exchange credentials no longer valid. New credentials are asked. Exception: {ex}");
                            MainForm.StandardWarningMsgBox("Credentials saved no longer valid. Please add new key pair on next window.");
                        }

                        AskUserForExchangeCredentials(lKeyValueObject, lKeyManager, FSelectedExchange);
                    }
                }
            }
            finally
            {
                if (!lExchanger.IsCredentialsSet)
                    MainForm.ChangeExchangeSelection(null);
                else
                {
                    FCurrentExchanger = lExchanger;
                    MainForm.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            MainForm.Invoke((Action) (() => MainForm.SetWaitCursor()));
                            if (!FDataLoadingTask.Wait(20000, FExchangeTaskCancellationSource.Token))
                                MainForm.StandardErrorMsgBox("Exchange data loading error", "Failed to load exchange data from database. Please try restarting application");
                            MainForm.Invoke((Action) (() => MainForm.SetArrowCursor()));
                            MainForm.LoadCurrencyExchangeOrders(ActiveCurrencyID);
                        }
                        catch (AggregateException ex)
                        {
                            var lException = ex.InnerExceptions.First();
                            Log.Write(LogLevel.Error, $"Exception thrown when loading exchange orders for {ActiveCurrencyID}. Details: {lException}");
                            MainForm.StandardExceptionMsgBox(lException, "Loading exchange order exception thrown");
                        }
                        catch (Exception ex)
                        {
                            Log.Write(LogLevel.Error, $"Exception thrown when loading exchange orders for {ActiveCurrencyID}. Details: {ex}");
                            MainForm.StandardExceptionMsgBox(ex, "Loading exchange order exception thrown");
                        }
                    }));
                    Task.Run(() =>
                    {
                        try
                        {
                            MainForm.Invoke((Action) (() => MainForm.SetWaitCursor()));
                            ExchangePandoraCurrencyChanged(ActiveCurrencyID);
                            MainForm.Invoke((Action) (() => MainForm.SetArrowCursor()));
                        }
                        catch (Exception ex)
                        {
                            Log.Write(LogLevel.Error, $"Exception thrown at task ExchangePandoraCurrencyChanged. Details: {ex}");
                        }
                    }, FExchangeTaskCancellationSource.Token);
                }
            }
        }

        private void MainForm_OnExhangeMarketSelectionChanged(object sender, EventArgs e)
        {
            if (MainForm.SelectedExchangeMarket != null)
            {
                //Even if the reference does not change anywhere else, this maybe will be used by others threads so I want this to be an atomic operation
                var lSelectedCoin = FExchangeCoinMarkets.ToList().Find(x => MainForm.SelectedExchangeMarket.Name == x.DestinationCurrencyInfo.WalletID.ToString());
                Interlocked.Exchange(ref FExchangeSelectedCoin, lSelectedCoin);
                MainForm.EstimatePrice = FExchangeSelectedCoin.IsSell ? FExchangeSelectedCoin.Prices.Bid : FExchangeSelectedCoin.Prices.Ask;
                MainForm.LabelPriceInCoin = "BTC";
                MainForm.TickerTotalReceived = MainForm.SelectedExchangeMarket.SubItems[1].Text;
                if (!string.IsNullOrEmpty(FLastExchangeSelectedCurrency) && FLastExchangeSelectedCurrency == MainForm.SelectedExchangeMarket.SubItems[1].Text)
                    return;
                MainForm.ExchangeTargetPrice = FExchangeSelectedCoin.IsSell ? FExchangeSelectedCoin.Prices.Bid : FExchangeSelectedCoin.Prices.Ask;
                MainForm.ExchangeStopPrice = FExchangeSelectedCoin.IsSell ? FExchangeSelectedCoin.Prices.Bid : FExchangeSelectedCoin.Prices.Ask;
                MainForm.ExchangeTransactionName = string.Format("{0} -> {1} - {2}", FExchangeSelectedCoin.BaseCurrencyInfo.Ticker, MainForm.SelectedExchangeMarket.SubItems[1].Text, DateTime.Now.ToString());
                FLastExchangeSelectedCurrency = MainForm.SelectedExchangeMarket.SubItems[1].Text;
                EnableExchangeInterface();
                MainForm_OnExchangeQuantityChanged();
                ChangeLastOrderAcordingToMarket(FExchangeSelectedCoin);
            }
        }

        private void ChangeLastOrderAcordingToMarket(ExchangeMarket aMarket)
        {
            string lCacheKey = string.Concat(ActiveCurrencyID, aMarket.MarketName);
            if (!FCacheLastOrders.TryGetValue(lCacheKey, out UserTradeOrder lMarketOrder))
            {
                var lMarketOrders = FOrderManager.GetOrdersByCurrencyID(ActiveCurrencyID).Where(lOrder => string.Equals(lOrder.ExchangeMarketName, aMarket.MarketName, StringComparison.OrdinalIgnoreCase));
                lMarketOrder = lMarketOrders.Any() ? lMarketOrders.Aggregate((lOrder1, lOrder2) => lOrder1.OpenTime > lOrder2.OpenTime ? lOrder1 : lOrder2) : null;
                if (lMarketOrder != null) FCacheLastOrders.Add(lCacheKey, lMarketOrder);
            }
            SetGUILastTransaction(lMarketOrder);
        }

        private void AddCurrencyOrderToGUI(long aCurrencyID, ExchangeMarket aMarket, UserTradeOrder aMarketOrder, bool aUpdateGUI = true, IPandoraExchanger aExchanger = null)
        {
            if (aMarket == null)
            {
                Log.Write(LogLevel.Error, $"Exchange GUI: Market not present for order with name {aMarketOrder.Name}.");
                return;
            }
            if (!aMarket.DestinationCurrencyInfo.WalletID.HasValue || !aMarket.DestinationCurrencyInfo.WalletID.HasValue)
            {
                Log.Write(LogLevel.Error, $"Exchange GUI: Market Currency Wallet ID not found for {aMarketOrder.Name}. Base: {aMarket.BaseCurrencyInfo.Name}, Destination: {aMarket.DestinationCurrencyInfo}");
                return;
            }
            decimal lRate = aMarket.IsSell ? 1 / aMarketOrder.Rate : aMarketOrder.Rate;
            decimal lRawAmount = aMarketOrder.SentQuantity / lRate;
            var lExchanger = aExchanger ?? FCurrentExchanger;
            if (!aMarket.DestinationCurrencyInfo.WalletID.HasValue || !aMarket.DestinationCurrencyInfo.WalletID.HasValue)
                return;
            var lOrderView = new AppMainForm.ExchangeOrderViewModel(aMarketOrder, new AppMainForm.ExchangeOrderViewModel.ExchangeOrderViewModelContextData
            {
                MainForm = MainForm,
                Market = aMarket,
                Precision = FPandorasWalletConnection.GetDefaultCurrency().Precision,
                TradeComission = 0.0025M
            });
            FDBExchanger.ReadOrderLogs(aMarketOrder.InternalID, out List<OrderMessage> lMessages);
            lOrderView.AddLog(lMessages.Select(lMessage => new AppMainForm.ExchangeOrderLogViewModel
            {
                ID = lMessage.ID,
                Message = lMessage.Message,
                Time = lMessage.Time.ToLocalTime().ToString()
            }));
            var lAsyncResult = MainForm.BeginInvoke(new Action(() =>
            {
                try
                {
                    MainForm.AddOrUpdateOrder(lOrderView, aCurrencyID, aUpdateGUI);
                }
                catch (Exception ex)
                {
                    Log.Write(LogLevel.Critical, $"Exception thrown adding exchange order to interface. Details: {ex}");
                }
            }
            ));
        }

        private void FExchanger_OnMarketPricesChanging(IEnumerable<string> aMarketBalanceChanged, int aExchangeID)
        {
            if (FPandorasWalletConnection != null && FCurrentExchanger != null && FCurrentExchanger.IsCredentialsSet)
            {
                foreach (ExchangeMarket lMarket in FExchangeCoinMarkets.Where(lMarket => aMarketBalanceChanged.Any(lMarketBalanceChanged => string.Equals(lMarketBalanceChanged, lMarket.MarketName, StringComparison.OrdinalIgnoreCase))))
                {
                    if (FDisableUpdating) return;
                    var lCurrency = GetUserDisplayedCurrencies().FirstOrDefault(lItem => (lMarket.DestinationCurrencyInfo.WalletID.HasValue && lItem.Id == lMarket.DestinationCurrencyInfo.WalletID)
                                                                                    || string.Equals(lItem.Ticker, lMarket.DestinationCurrencyInfo.Ticker, StringComparison.OrdinalIgnoreCase)
                                                                                    || string.Equals(lItem.Name, lMarket.DestinationCurrencyInfo.Name, StringComparison.OrdinalIgnoreCase));
                    if (lCurrency != null)
                        MainForm?.BeginInvoke(new MethodInvoker(delegate ()
                            {
                                if (!(MainForm.Disposing || MainForm.IsDisposed) && aExchangeID == (int) FSelectedExchange)
                                {
                                    MainForm.AddCoinExchangeTo(lCurrency.Id, lCurrency.Name, lCurrency.Ticker, lMarket.IsSell ? lMarket.Prices.Bid : lMarket.Prices.Ask);
                                    if (FExchangeSelectedCoin?.MarketName == lMarket.MarketName)
                                        MainForm.EstimatePrice = FExchangeSelectedCoin.IsSell ? FExchangeSelectedCoin.Prices.Bid : FExchangeSelectedCoin.Prices.Ask; ;
                                }
                            }));
                }
            }
        }

        private void SetupDBSaveManager()
        {
            PandoraExchangeSQLiteSaveManager lOldDBExchanger = null;
            if (FDBExchanger != null)
                lOldDBExchanger = FDBExchanger;
            FDBExchanger = new PandoraExchangeSQLiteSaveManager(SqLiteDbFileName, FPandorasWalletConnection.UserName, FPandorasWalletConnection.Email);
            FOrderManager = new ExchangeOrderManager(FDBExchanger);
            FOrderManager.OnOrderStatusChanged += OrderManager_OnOrderStatusChanged;
            FOrderManager.OnNewOrderLogsAdded += OrderManager_OnNewOrderLogsAdded;
            lOldDBExchanger?.Dispose();
        }

        private void OrderManager_OnNewOrderLogsAdded(int aOrderID, IEnumerable<OrderMessage> aOrderMessages)
        {
            var lViewModel = MainForm.GetOrderViewModel(aOrderID);
            var lViewOrderMessages = aOrderMessages.Select(lMsg => new AppMainForm.ExchangeOrderLogViewModel { ID = lMsg.ID, Message = lMsg.Message, Time = lMsg.Time.ToLocalTime().ToString() });
            if (lViewOrderMessages.Any() && lViewModel != null)
                lViewModel.AddLog(lViewOrderMessages);
        }

        private void OrderManager_OnOrderStatusChanged(int aInternalID, OrderStatus aNewStatus, long aCurrencyID)
        {
            var lViewModel = MainForm.GetOrderViewModel(aInternalID);
            if (lViewModel != null)
            {
                lViewModel.Status = aNewStatus.ToString();
                var lAsyncResult = MainForm.BeginInvoke(new MethodInvoker(() => MainForm.AddOrUpdateOrder(lViewModel, aCurrencyID)));
                MainForm.EndInvoke(lAsyncResult);
            }
        }

        public void StopProcess()
        {
            if (!Started) return;
            FExchangeTaskCancellationSource.Cancel();
            FExchangeTaskCancellationSource = null;
            StopStageOperators();
            FStageOperators.Clear();
            FStageOperators = null;
            FOrderManager?.Dispose();
            ResetInterface();
            ClearResources();
        }

        private void ClearResources()
        {
            FDBExchanger = null;
            Started = false;
        }

        private void ResetInterface()
        {
            FLastExchangeSelectedCurrency = string.Empty;
            if (MainForm != null)
            {
                DisableExchangeInterface();
                MainForm.ClearListExchangeTo();
                MainForm.EstimatePrice = 0;
                MainForm.ChangeExchangeSelection(null);
            }
        }

        private void ClearExchangeInterface()
        {
            MainForm.ExchangeTargetPrice = 0;
            MainForm.ExchangeQuantity = 0;
            MainForm.ExchangeTotalReceived = 0;
            MainForm.TickerTotalReceived = string.Empty;
        }

        private void DisableExchangeInterface()
        {
            MainForm.BeginInvoke((Action) (() =>
            {
                ClearExchangeInterface();
                MainForm.ExchangeTargetPriceEnabled = false;
                MainForm.ExchangeQuantityEnabled = false;
                MainForm.ExchangeTotalReceivedEnabled = false;
                MainForm.ExchangeTransactionName = string.Empty;
                MainForm.ExchangeTransactionNameEnabled = false;
                MainForm.CheckAllOrderHistoryEnabled = false;
                MainForm.ExchangeButtonEnabled = false;
                MainForm.ExchangeStoptPriceEnabled = false;
                MainForm.ExchangeChangeKeysEnabled = false;
                MainForm.StatusControlExchange.StatusName = string.Empty;
            }));
        }

        private void EnableExchangeInterface()
        {
            MainForm.BeginInvoke((Action) (() =>
            {
                MainForm.ExchangeStoptPriceEnabled = true;
                MainForm.ExchangeTargetPriceEnabled = true;
                MainForm.ExchangeQuantityEnabled = true;
                MainForm.ExchangeTotalReceivedEnabled = true;
                MainForm.ExchangeTransactionNameEnabled = true;
                MainForm.CheckAllOrderHistoryEnabled = true;
                MainForm.ExchangeButtonEnabled = true;
                MainForm.ExchangeChangeKeysEnabled = true;
            }));
        }

        private ConcurrentBag<IPandoraExchanger> FExchanges;

        private void SetExchangeCredentials(AvailableExchangesList aExchangeID, string aKey, string aSecret)
        {
            var lFactory = GetCurrentExchangeFactory();
            var lExchange = lFactory.GetPandoraExchange(aExchangeID);
            lExchange.SetCredentials(aKey, aSecret);
        }

        private string TryToTransferMoneyToExchange(decimal aAmount, string aAddress, long aCurrencyID)
        {
            decimal lTxFee = 0;
            int lCounter = 0;
            string lExceptionMessage = string.Empty;
            do
            {
                lCounter++;
                try
                {
                    lTxFee = PandoraClientControl.GetInstance().CalculateTxFee(aAddress, aAmount, aCurrencyID);
                    break;
                }
                catch (SubscriptionOverException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    lExceptionMessage = ex.Message;
                    continue;
                }
            } while (lCounter < 6);
            if (lTxFee == 0) throw new Exception($"Failed to get transaction txfee. Details: {lExceptionMessage}");
            decimal lBalance = PandoraClientControl.GetInstance().GetBalance(aCurrencyID);
            decimal lDecimalTxFee = (lTxFee / (decimal) Math.Pow(10, FPandorasWalletConnection.GetDefaultCurrency().Precision));
            if (lBalance == 0 || (aAmount + lDecimalTxFee) > lBalance) throw new Exception("Not enough balance to transfer to the exchange");
            return ExecuteSendTx(aAmount, aCurrencyID, lTxFee, aAddress);
        }

        /// <summary>
        /// Construct a raw transaction to be sended to server
        /// </summary>
        /// <param name="aAmount">An amount to send</param>
        /// <param name="aActiveCurrencyID">Working currency id</param>
        /// <param name="aTxFee">Transaction calculated txfee</param>
        /// <param name="aExchangeAddress">Destination address (exchange address)</param>
        /// <returns></returns>
        private string ExecuteSendTx(decimal aAmount, long aActiveCurrencyID, decimal aTxFee, string aExchangeAddress)
        {
            //FPandoraClientControl.InitializeRootSeed();
            var lClientControl = PandoraClientControl.GetInstance();
            string lTx = lClientControl.GetNewSignedTransaction(lClientControl.GetCurrency(aActiveCurrencyID), aExchangeAddress, aAmount, aTxFee);

            long lTxHandle = FPandorasWalletConnection.DirectSendTransaction(aActiveCurrencyID, lTx);
      
            string lReturnedTxID = null;

            try
            {
                string lTxID;
                while (!FPandorasWalletConnection.CheckTransactionHandle(lTxHandle, out lTxID)) //Waits endlessly until server response
                    System.Threading.Thread.Sleep(1000);
                lReturnedTxID = lTxID;
            }
            catch (Exception ex)
            {
                string s = string.Format("Unhandled error on sending transaction. Details: {0}", ex);
                Log.Write("Unhandled error on sending transaction. Details: {0}", ex);
            }

            return lReturnedTxID;
        }

        /// <summary>
        /// Unlocks the wallet and place the initial requested order into the DB
        /// </summary>
        /// <param name="aPriceTarget">Price target</param>
        /// <param name="aStopPrice">Stop price</param>
        /// <param name="aAmount">Amount to transfer</param>
        /// <param name="aMarket">Current selected market</param>
        private void TryToCreateNewExchangeTransaction(decimal aPriceTarget, decimal aStopPrice, decimal aAmount, ExchangeMarket aMarket)
        {
            if (FPandoraClientControl.FromUserDecryptWallet(false))
            {
                string lTempGUID = Guid.NewGuid().ToString("N");

                UserTradeOrder lNewOrder = new UserTradeOrder
                {
                    CoinTxID = lTempGUID,
                    SentQuantity = aAmount,
                    ExchangeMarketName = aMarket.MarketName,
                    Rate = aPriceTarget,
                    StopPrice = aStopPrice,
                    Status = OrderStatus.Initial,
                    OpenTime = DateTime.UtcNow,
                    Name = MainForm.ExchangeTransactionName,
                    PandoraExchangeID = FCurrentExchanger.ID
                };
                lNewOrder.BaseCurrency.Ticker = aMarket.BaseCurrencyInfo.Ticker;
                UserTradeOrder lOrderWithID = FOrderManager.SaveNewOrder(lNewOrder, ActiveCurrencyID);
                FOrderManager.WriteTransactionLogEntry(lOrderWithID);
                AddCurrencyOrderToGUI(ActiveCurrencyID, aMarket, lOrderWithID);
                SetGUILastTransaction(lOrderWithID);
                string lCacheLastOrdersKey = string.Concat(ActiveCurrencyID, lOrderWithID.ExchangeMarketName);
                FCacheLastOrders[lCacheLastOrdersKey] = lOrderWithID;
            }
        }

        public void Dispose()
        {
            MainForm.OnSelectedCurrencyChanged -= MainForm_ExchangeOnSelectedCurrencyChanged; ;
            MainForm.OnExchangeSelectedCurrencyChanged -= MainForm_OnExhangeMarketSelectionChanged;
            MainForm.OnLabelEstimatePriceClick -= MainForm_OnLabelEstimatePriceClick;
            MainForm.OnExchangeSelectionChanged -= MainForm_OnExchangeSelectionChanged;

            MainForm.OnExchangeQuantityChanged -= MainForm_OnExchangeQuantityChanged;
            MainForm.OnExchangeBtnClick -= MainForm_OnExchangeBtnClick;
            //MainForm.OnOrderHistorySelectionChanged -= MainForm_OnOrderHistorySelectionChanged;
            MainForm.OnCancelBtnClick -= MainForm_OnCancelBtnClick;

            MainForm.OnExchangeReceivedChanged -= MainForm_OnExchangeReceivedChanged;
            MainForm.OnCheckAllOrderHistory -= MainForm_OnCheckAllOrderHistory;
            StopProcess();
        }
    }
}