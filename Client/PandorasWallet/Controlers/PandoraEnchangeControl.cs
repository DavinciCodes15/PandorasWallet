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
using Pandora.Client.Exchange.Exchangers.Contracts;
using Pandora.Client.Exchange.Exchangers;
using Pandora.Client.Exchange.Models;
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
using Pandora.Client.Exchangers.Contracts;
using Pandora.Client.Exchange.Contracts;

namespace Pandora.Client.PandorasWallet.Controlers
{
    public class PandoraEnchangeControl : IDisposable
    {
        private const decimal FExchangeTradeFeePercent = 0.0025M;
        //private PandoraExchanger FExchanger;

        private PandoraExchangeFactoryProducer FExchangeFactoryProducer;
        private ExchangeOrderManager FOrderManager;

        private ConcurrentDictionary<string, IExchangeMarket> FExchangeUserMarkets;
        private LoginExchanger FExchangeLogin;
        private CancellationTokenSource FExchangeTaskCancellationSource;
        private IExchangeMarket FExchangeSelectedMarket;
        private PandoraExchangeSQLiteSaveManager FDBExchanger;
        private string FLastExchangeSelectedCurrency = "";
        private PandoraClientControl FPandoraClientControl;

        private Dictionary<string, UserTradeOrder> FCacheLastOrders = new Dictionary<string, UserTradeOrder>();
        private Task FDataLoadingTask;
        private IPandoraExchanger FCurrentExchanger;
        private AvailableExchangesList FSelectedExchange;

        private ConcurrentDictionary<string, ExchangeStageOperator> FStageOperators;

        private decimal FLastExchangeQuantity;
        private decimal FLastExchangeTotal;
        private decimal FLastExchangePrice;
        private bool FUpdatingGUIPrices;

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
                    try
                    {
                        foreach (var lCurrencyItem in GetUserCurrencies())
                        {
#if DEBUG
                            System.Diagnostics.Debug.WriteLine($"Now loading order data for {lCurrencyItem.Name}");
#endif
                            LoadNewCurrencyData(lCurrencyItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Write(LogLevel.Error, $"Exception in loading data task. Details: {ex}");
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
            var lExchangeFactory = GetCurrentExchangeFactory();
            var lCurrencyOrders = FOrderManager.LoadOrders(aCurrencyItem, lExchangeFactory, GetCurrencyIDFromIdentifier);
            foreach (var lOrder in lCurrencyOrders)
            {
                if (lOrder.Market != null)
                    AddCurrencyOrderToGUI(aCurrencyItem.Id, lOrder, false);
                else
                    Log.Write(LogLevel.Warning, $"No market found for order {lOrder.InternalID} - {lOrder.Name} of market {lOrder.Market.MarketID} at exchange {(AvailableExchangesList) lOrder.Market.ExchangeID}. To avoid exceptions it will not load");
            }
        }

        private IEnumerable<ICurrencyIdentity> GetUserCurrencies()
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

        private long? GetCurrencyIDFromIdentifier(string aTicker)
        {
            return GetCurrencyFromIdentifier(string.Empty, aTicker)?.Id;
        }

        private ICurrencyIdentity GetCurrencyFromIdentifier(string aCurrencyName, string aTicker)
        {
            ICurrencyIdentity lResult = GetUserCurrencies().Where(lCoin => string.Equals(lCoin.Ticker, aTicker, StringComparison.OrdinalIgnoreCase) || string.Equals(lCoin.Name, aCurrencyName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            return lResult;
        }

        public void SaveRestoredKeyAndSecret()
        {
            var lKeyValueObject = FKeyValueHelper.LoadKeyValues(0);
            FKeyValueHelper.SaveChanges(lKeyValueObject);
        }

        private void ExchangeInitialize()
        {
            FExchangeLogin = new LoginExchanger();

            FExchangeFactoryProducer = PandoraExchangeFactoryProducer.GetInstance();
            FExchangeUserMarkets = new ConcurrentDictionary<string, IExchangeMarket>();

            MainForm.OnSelectedCurrencyChanged += MainForm_ExchangeOnSelectedCurrencyChanged; ;
            MainForm.OnExchangeSelectedCurrencyChanged += MainForm_OnExhangeMarketSelectionChanged;
            MainForm.OnLabelEstimatePriceClick += MainForm_OnLabelEstimatePriceClick;
            MainForm.OnExchangeSelectionChanged += MainForm_OnExchangeSelectionChanged;
            MainForm.OnChangeExchangeKeysBtnClick += MainForm_OnChangeExchangeKeysBtnClick;

            MainForm.OnExchangeQuantityChanged += MainForm_OnExchangeQuantityChanged;
            MainForm.OnExchangeBtnClick += MainForm_OnExchangeBtnClick;
            MainForm.OnPriceChanged += MainForm_OnPriceChanged;
            //MainForm.OnOrderHistorySelectionChanged += MainForm_OnOrderHistorySelectionChanged;
            MainForm.OnCancelBtnClick += MainForm_OnCancelBtnClick;

            MainForm.OnExchangeReceivedChanged += MainForm_OnExchangeReceivedChanged;
            MainForm.OnCheckAllOrderHistory += MainForm_OnCheckAllOrderHistory;
            MainForm.OnExchangeChartIntervalChanged += MainForm_OnExchangeChartIntervalChanged;

            MainForm.LoadMarketIntervals(Enum.GetNames(typeof(ChartInterval)));
        }

        private void MainForm_OnExchangeChartIntervalChanged()
        {
            if (MainForm.SelectedExchangeMarket != null)
                LoadMarketChart(FExchangeSelectedMarket);
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
        private IEnumerable<IExchangeMarket> GetMarketsUsableByUser(ICurrencyIdentity aCurrency)
        {
            var lUserMarkets = FCurrentExchanger.GetMarketCoins(aCurrency, GetCurrencyIDFromIdentifier).Where(lMarket => lMarket.BuyingCurrencyInfo.Id != 0);
            FDisableUpdating = true;
            FExchangeUserMarkets.Clear();
            foreach (var lUserMarket in lUserMarkets)
                FExchangeUserMarkets.TryAdd(lUserMarket.MarketID, lUserMarket);
            FDisableUpdating = false;
            return lUserMarkets;
        }

        /// <summary>
        /// Triggers with event OnCurrencyItemSelectionChanged. It will generate his own thread
        /// </summary>
        /// <param name="aCurrency">Selected currency id</param>
        private void PandoraCurrencyChangedProcess(long aCurrency)
        {
            MainForm.BeginInvoke(new Action(() =>
            {
                MainForm.ClearExchangeMarketSelector();
                MainForm.StatusControlExchange.ClearStatusList();
                MainForm.StatusControlOrderHistory.ClearStatusList();
                MainForm.ClearExchangeChart();
            }));

            //added condition before to last conditional if;  unexpected null issue removed since first execution of PW without exchange's credentials.
            if ((bool) MainForm.Invoke((Func<bool>) (() => MainForm.SelectedExchange == null)) || FCurrentExchanger == null || !FCurrentExchanger.IsCredentialsSet)
                return;

            MainForm.BeginInvoke(new Action(() => { MainForm.ExchangeLoadingHidden = false; }));
            ICurrencyIdentity lCurrency = FPandorasWalletConnection.GetCurrency(aCurrency);
            if (lCurrency == null)
                lCurrency = FPandorasWalletConnection.GetCurrencyToken(aCurrency) ?? throw new Exception($"No existing currency with id {aCurrency} found on the wallet");

            LoadExchangeMarkets(lCurrency);
            if (!FExchangeUserMarkets.Any())
                FExchangeSelectedMarket = null;
            DisableExchangeInterface();
            LoadOrderAcordingToAllCheckbox(aCurrency);
            FLastExchangeSelectedCurrency = string.Empty;
            MainForm.BeginInvoke(new Action(() =>
            {
                MainForm.TickerQuantity = MainForm.SelectedCurrency.Ticker;
                MainForm.ExchangeLoadingHidden = true;
            }));
        }

        private void LoadExchangeMarkets(ICurrencyIdentity aCurrency)
        {
            var lUserMarkets = GetMarketsUsableByUser(aCurrency).ToArray();
            MainForm.BeginInvoke((Action) (() => MainForm.ExchangeMarketSelectorEnabled = false));
            foreach (var lUserMarket in lUserMarkets)
            {
                var lCurrencyItem = lUserMarket.BuyingCurrencyInfo;
                decimal lPrice = lUserMarket.Prices.Last;
                MainForm.BeginInvoke(new Action(() => MainForm.AddCoinExchangeTo(lCurrencyItem.Id, lCurrencyItem.Name, lCurrencyItem.Ticker, lPrice)));
            }
            MainForm.BeginInvoke((Action) (() => MainForm.ExchangeMarketSelectorEnabled = true));
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
            MainForm.ExchangeButtonEnabled = false;
            try
            {
                decimal lAmountTrade = MainForm.ExchangeQuantity;
                if (MainForm.ExchangeTargetPrice <= 0 || MainForm.ExchangeStopPrice <= 0)
                    throw new ClientExceptions.InvalidOperationException("Invalid target or stop price amount.");
                if (MainForm.ExchangeQuantity == 0 && MainForm.ExchangeTotalReceived > 0)
                    MainForm_OnExchangeReceivedChanged();
                if (MainForm.ExchangeTotalReceived == 0 && MainForm.ExchangeQuantity > 0)
                    MainForm_OnExchangeQuantityChanged();
                decimal lAmounTradeSize = FExchangeSelectedMarket.MarketDirection == MarketDirection.Sell ? MainForm.ExchangeQuantity : MainForm.ExchangeTotalReceived;
                if (lAmounTradeSize <= 0)
                    if (FExchangeSelectedMarket.MinimumTrade >= (lAmounTradeSize))
                        throw new ClientExceptions.InvalidOperationException(string.Format("Minimum order trade size is {0} {1}", FExchangeSelectedMarket.MinimumTrade, FExchangeSelectedMarket.MarketDirection == MarketDirection.Sell ? FExchangeSelectedMarket.SellingCurrencyInfo.Ticker : FExchangeSelectedMarket.BuyingCurrencyInfo.Ticker));
                var lTxFee = GetCurrencyTxFeeFromWallet(ActiveCurrencyID);
                if (lAmountTrade + lTxFee > FPandoraClientControl.GetBalance(ActiveCurrencyID))
                    throw new ClientExceptions.InvalidOperationException($"Not enough balance for this trade size. Please consider that the order consumes a fee of around {lTxFee.ToString("0.######")}");
                TryToCreateNewExchangeTransaction(MainForm.ExchangeTargetPrice, MainForm.ExchangeStopPrice, MainForm.ExchangeQuantity, lTxFee);
            }
            finally
            {
                MainForm.ExchangeButtonEnabled = true;
            }
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

        private ICurrencyItem GetFormCurrency(long aCurrencyID)
        {
            var lCurrency = FPandoraClientControl.GetCurrency(aCurrencyID);
            if (lCurrency == null) throw new Exception("Currency not found or invalid");
            return lCurrency;
        }

        private void UpdateGUIExchangeAproxReceived()
        {
            if (FUpdatingGUIPrices) return;
            FUpdatingGUIPrices = true;
            try
            {
                var lFormCurrency = GetFormCurrency(FExchangeSelectedMarket.SellingCurrencyInfo.Id);
                decimal lTotal = Math.Round(MainForm.ExchangeQuantity / (FExchangeSelectedMarket.MarketDirection == MarketDirection.Sell ? 1 / MainForm.ExchangeTargetPrice : MainForm.ExchangeTargetPrice), lFormCurrency.Precision);
                decimal lComision = lTotal * FExchangeTradeFeePercent;
                MainForm.ExchangeTotalReceived = Math.Round(lTotal - lComision, lFormCurrency.Precision);
                FLastExchangeQuantity = MainForm.ExchangeQuantity;
                FLastExchangePrice = MainForm.ExchangeTargetPrice;
            }
            catch (OverflowException)
            {
                MainForm.ExchangeQuantity = FLastExchangeQuantity;
                MainForm.ExchangeTargetPrice = FLastExchangePrice;
            }
            finally
            {
                FUpdatingGUIPrices = false;
            }
        }

        private void UpdateGUIExchangeSpend()
        {
            if (FUpdatingGUIPrices) return;
            FUpdatingGUIPrices = true;
            try
            {
                var lFormCurrency = GetFormCurrency(FExchangeSelectedMarket.SellingCurrencyInfo.Id);
                decimal lTotal = Math.Round(MainForm.ExchangeTotalReceived * (FExchangeSelectedMarket.MarketDirection == MarketDirection.Sell ? 1 / MainForm.ExchangeTargetPrice : MainForm.ExchangeTargetPrice), lFormCurrency.Precision);
                decimal lComision = lTotal * FExchangeTradeFeePercent;
                MainForm.ExchangeQuantity = Math.Round(lTotal - lComision, lFormCurrency.Precision);
                FLastExchangeTotal = MainForm.ExchangeTotalReceived;
                FLastExchangePrice = MainForm.ExchangeTargetPrice;
            }
            catch (OverflowException)
            {
                MainForm.ExchangeTotalReceived = FLastExchangeTotal;
                MainForm.ExchangeTargetPrice = FLastExchangePrice;
            }
            finally
            {
                FUpdatingGUIPrices = false;
            }
        }

        private void MainForm_OnExchangeQuantityChanged()
        {
            //I need to disable the other event to prevent a stack overflow exception
            MainForm.OnExchangeReceivedChanged -= MainForm_OnExchangeReceivedChanged;
            if (MainForm.ExchangeTargetPrice > 0)
                UpdateGUIExchangeAproxReceived();
            MainForm.OnExchangeReceivedChanged += MainForm_OnExchangeReceivedChanged;
        }

        private void MainForm_OnExchangeReceivedChanged()
        {
            MainForm.OnExchangeQuantityChanged -= MainForm_OnExchangeQuantityChanged;
            if (MainForm.ExchangeTargetPrice > 0)
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

        private async Task<Tuple<string, string>> TryDecryptAndSetExchangeCredentials(KeyManager aKeyManager, string aEncryptedPublicKey, string aEncryptedPrivateKey, AvailableExchangesList aSelectedExchange)
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
                await Task.Run(() => SetExchangeCredentials(aSelectedExchange, lExchangePublicKey, lExchangeSecret));
            }
            else throw new ClientExceptions.CancelledUserOperation("No password provided");
            return new Tuple<string, string>(lExchangePublicKey, lExchangeSecret);
        }

        private async void DoExchangeSelectionChanged()
        {
            var lExchangeFactory = GetCurrentExchangeFactory();
            var lExchanger = lExchangeFactory.GetPandoraExchange(FSelectedExchange);

            try
            {
                MainForm.BeginInvoke((Action) (() =>
                {
                    MainForm.ExchangeSelectorEnabled = false;
                    MainForm.ExchangeChangeKeysEnabled = false;
                    MainForm.ExchangeLoadingHidden = false;
                }));
                //In case that when the user loaded their info, we should try again every time that it clicks in the combo box
                if (!Started)
                {
                    StartProcess();
                    MainForm.BeginInvoke((Action) (() => MainForm.ChangeExchangeSelection(null)));
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
                                await TryDecryptAndSetExchangeCredentials(lKeyManager, lKeyValueObject.PublicKeys[lSelectedExchange], lKeyValueObject.PrivateKeys[lSelectedExchange], FSelectedExchange);
                                return;
                            }
                            else if (FSelectedExchange == AvailableExchangesList.Bittrex)
                            {
                                var lOldKeyValueHelper = new ExchangeKeyValueHelper<ExchangeKeyValueObject>(FDBExchanger);
                                var lOldKeyValues = lOldKeyValueHelper.LoadKeyValues(0);
                                if (!string.IsNullOrEmpty(lOldKeyValues.PublicKey) && !string.IsNullOrEmpty(lOldKeyValues.PrivateKey))
                                {
                                    var lUncryptedKeys = await TryDecryptAndSetExchangeCredentials(lKeyManager, lOldKeyValues.PublicKey, lOldKeyValues.PrivateKey, FSelectedExchange);
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
                            MainForm.BeginInvoke((Action) (() => MainForm.StandardWarningMsgBox("Credentials saved no longer valid. Please add new key pair on next window.")));
                        }
                        MainForm.Invoke((Action) (() => MainForm.SetArrowCursor()));
                        AskUserForExchangeCredentials(lKeyValueObject, lKeyManager, FSelectedExchange);
                        MainForm.Invoke((Action) (() => MainForm.SetWaitCursor()));
                    }
                }
            }
            finally
            {
                if (!lExchanger.IsCredentialsSet)
                    MainForm.BeginInvoke((Action) (() => MainForm.ChangeExchangeSelection(null)));
                else
                {
                    FCurrentExchanger = lExchanger;
                    try
                    {
                        await FDataLoadingTask;
                        MainForm.BeginInvoke((Action) (() => MainForm.LoadCurrencyExchangeOrders(ActiveCurrencyID)));
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
                    await Task.Run(() =>
                    {
                        try
                        {
                            ExchangePandoraCurrencyChanged(ActiveCurrencyID);
                        }
                        catch (Exception ex)
                        {
                            Log.Write(LogLevel.Error, $"Exception thrown at task ExchangePandoraCurrencyChanged. Details: {ex}");
                        }
                        MainForm.BeginInvoke((Action) (() => MainForm.ExchangeChangeKeysEnabled = true));
                    });
                }
                MainForm.BeginInvoke((Action) (() => { MainForm.ExchangeSelectorEnabled = true; MainForm.ExchangeLoadingHidden = true; MainForm.SetArrowCursor(); }));
            }
        }

        private void MainForm_OnExchangeSelectionChanged(object sender, EventArgs e)
        {
            if (MainForm.IsHandleCreated)
                MainForm.BeginInvoke(new MethodInvoker(() =>
                {
                    DisableExchangeInterface();
                    MainForm.ClearExchangeMarketSelector();
                    MainForm.ClearOrderHistory();
                    MainForm.ClearExchangeChart();
                    MainForm.StatusControlExchange.StatusName = string.Empty;
                }));

            if (MainForm.SelectedExchange == null)
                FSelectedExchange = 0;
            else
            {
                FSelectedExchange = (AvailableExchangesList) MainForm.SelectedExchange.ID;
                DoExchangeSelectionChanged();
            }
        }

        private void MainForm_OnExhangeMarketSelectionChanged(object sender, EventArgs e)
        {
            if (MainForm.SelectedExchangeMarket != null)
            {
                //Even if the reference does not change anywhere else, this maybe will be used by others threads so I want this to be an atomic operation
                var lSelectedMarket = FExchangeUserMarkets.Values.FirstOrDefault(lMarket => MainForm.SelectedExchangeMarket.Name == lMarket.BuyingCurrencyInfo.Id.ToString());
                if (lSelectedMarket == null) throw new Exception("Invalid market selected. Please try restarting the application.");
                Interlocked.Exchange(ref FExchangeSelectedMarket, lSelectedMarket);
                MainForm.EstimatePrice = FExchangeSelectedMarket.MarketDirection == MarketDirection.Sell ? FExchangeSelectedMarket.Prices.Bid : FExchangeSelectedMarket.Prices.Ask;
                MainForm.LabelPriceInCoin = FExchangeSelectedMarket.MarketBaseCurrencyInfo.Ticker;
                MainForm.TickerTotalReceived = MainForm.SelectedExchangeMarket.SubItems[1].Text;
                MainForm.TickerPrices = FExchangeSelectedMarket.MarketBaseCurrencyInfo.Ticker;
                if (!string.IsNullOrEmpty(FLastExchangeSelectedCurrency) && FLastExchangeSelectedCurrency == MainForm.SelectedExchangeMarket.SubItems[1].Text)
                    return;
                MainForm.ExchangeTargetPrice = FExchangeSelectedMarket.MarketDirection == MarketDirection.Sell ? FExchangeSelectedMarket.Prices.Bid : FExchangeSelectedMarket.Prices.Ask;
                MainForm.ExchangeStopPrice = FExchangeSelectedMarket.MarketDirection == MarketDirection.Sell ? FExchangeSelectedMarket.Prices.Bid : FExchangeSelectedMarket.Prices.Ask;
                MainForm.ExchangeTransactionName = string.Format("{0} -> {1} - {2}", FExchangeSelectedMarket.SellingCurrencyInfo.Ticker, MainForm.SelectedExchangeMarket.SubItems[1].Text, DateTime.Now.ToString());
                FLastExchangeSelectedCurrency = MainForm.SelectedExchangeMarket.SubItems[1].Text;
                EnableExchangeInterface();
                MainForm_OnExchangeQuantityChanged();
                ChangeLastOrderAcordingToMarket(FExchangeSelectedMarket);
                LoadMarketChart(lSelectedMarket);
            }
        }

        private void LoadMarketChart(IExchangeMarket aMarket)
        {
            MainForm.BeginInvoke(new Action(MainForm.ClearExchangeChart));
            var lInterval = MainForm.ExchangeSelectedChartInterval != null ? (ChartInterval) Enum.Parse(typeof(ChartInterval), MainForm.ExchangeSelectedChartInterval) : ChartInterval.Daily;
            if (aMarket == null) return;
            DateTime lEndTime = DateTime.UtcNow;
            DateTime lStartTime;
            switch (lInterval)
            {
                case ChartInterval.Daily:
                    lStartTime = lEndTime.AddDays(-45);
                    break;

                case ChartInterval.Hourly:
                    lStartTime = lEndTime.AddHours(-45);
                    break;

                case ChartInterval.FiveMinutes:
                    lStartTime = lEndTime.AddMinutes(-225);
                    break;

                default:
                    lStartTime = lEndTime.AddDays(-1);
                    break;
            }
            var lPoints = FCurrentExchanger.GetCandleStickChart(aMarket, lStartTime, lEndTime, lInterval);
            if (lPoints.Any())
            {
                MainForm.BeginInvoke(new Action(() =>
                {
                    MainForm.InitializeExchangeChart(aMarket.MarketBaseCurrencyInfo.Ticker, $"{(aMarket.MarketDirection == MarketDirection.Sell ? aMarket.SellingCurrencyInfo.Name : aMarket.BuyingCurrencyInfo.Name)} Price Chart:");
                    foreach (var lCandlePoint in lPoints)
                        MainForm.AddExchangeChartPoint(lCandlePoint.TimeStamp.ToLocalTime(), lCandlePoint.Open, lCandlePoint.Close, lCandlePoint.High, lCandlePoint.Low);
                }));
            }
        }

        private void ChangeLastOrderAcordingToMarket(IExchangeMarket aMarket)
        {
            if (!FCacheLastOrders.TryGetValue(aMarket.MarketID, out UserTradeOrder lMarketOrder))
            {
                var lMarketOrders = FOrderManager.GetOrdersByCurrencyID(ActiveCurrencyID).Where(lOrder => string.Equals(lOrder.Market.MarketID, aMarket.MarketID, StringComparison.OrdinalIgnoreCase));
                lMarketOrder = lMarketOrders.Any() ? lMarketOrders.Aggregate((lOrder1, lOrder2) => lOrder1.OpenTime > lOrder2.OpenTime ? lOrder1 : lOrder2) : null;
                if (lMarketOrder != null) FCacheLastOrders.Add(aMarket.MarketID, lMarketOrder);
            }
            SetGUILastTransaction(lMarketOrder);
        }

        private void AddCurrencyOrderToGUI(long aCurrencyID, UserTradeOrder aMarketOrder, bool aUpdateGUI = true)
        {
            if (aMarketOrder.Market == null)
            {
                Log.Write(LogLevel.Error, $"Exchange GUI: Market not present for order with name {aMarketOrder.Name}.");
                return;
            }
            if (aMarketOrder.Market.BuyingCurrencyInfo.Id == 0 || aMarketOrder.Market.BuyingCurrencyInfo.Id == 0)
            {
                Log.Write(LogLevel.Error, $"Exchange GUI: Market Currency Wallet ID not found for {aMarketOrder.Name}. Selling: {aMarketOrder.Market.SellingCurrencyInfo.Ticker}, Buying: {aMarketOrder.Market.BuyingCurrencyInfo.Ticker}");
                return;
            }
            decimal lRate = aMarketOrder.Market.MarketDirection == MarketDirection.Sell ? 1 / aMarketOrder.Rate : aMarketOrder.Rate;
            decimal lRawAmount = aMarketOrder.SentQuantity / lRate;
            var lOrderView = new AppMainForm.ExchangeOrderViewModel(aMarketOrder, new AppMainForm.ExchangeOrderViewModel.ExchangeOrderViewModelContextData
            {
                MainForm = MainForm,
                Market = aMarketOrder.Market,
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

        private void FExchanger_OnMarketPricesChanging(IEnumerable<IExchangeMarket> aMarketBalanceChanged)
        {
            if (FPandorasWalletConnection != null && FCurrentExchanger != null && FCurrentExchanger.IsCredentialsSet)
            {
                var lMarketsUpdated = new List<IExchangeMarket>();
                foreach (var lMarketChanged in aMarketBalanceChanged)
                {
                    if (FDisableUpdating) return;
                    if (!FExchangeUserMarkets.TryGetValue(lMarketChanged.MarketID, out IExchangeMarket lUserMarket)) continue;
                    lMarketsUpdated.Add(lUserMarket);
                }
                MainForm?.BeginInvoke(new MethodInvoker(delegate ()
                {
                    if (!(MainForm.Disposing || MainForm.IsDisposed))
                    {
                        foreach (var lUpdatedMarket in lMarketsUpdated)
                        {
                            MainForm.AddCoinExchangeTo(lUpdatedMarket.BuyingCurrencyInfo.Id, lUpdatedMarket.BuyingCurrencyInfo.Name, lUpdatedMarket.BuyingCurrencyInfo.Ticker, lUpdatedMarket.MarketDirection == MarketDirection.Sell ? lUpdatedMarket.Prices.Bid : lUpdatedMarket.Prices.Ask);
                            if (FExchangeSelectedMarket?.MarketID == lUpdatedMarket.MarketID)
                                MainForm.EstimatePrice = lUpdatedMarket.MarketDirection == MarketDirection.Sell ? FExchangeSelectedMarket.Prices.Bid : FExchangeSelectedMarket.Prices.Ask;
                        }
                    }
                }));
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
                MainForm.ClearExchangeMarketSelector();
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
            }));
        }

        private void SetExchangeCredentials(AvailableExchangesList aExchangeID, string aKey, string aSecret)
        {
            var lFactory = GetCurrentExchangeFactory();
            var lExchange = lFactory.GetPandoraExchange(aExchangeID);
            lExchange.SetCredentials(aKey, aSecret);
        }

        private string TryToTransferMoneyToExchange(decimal aAmount, string aAddress, long aCurrencyID, decimal? aTxFee)
        {
            decimal lBalance = PandoraClientControl.GetInstance().GetBalance(aCurrencyID);
            var lTxFee = aTxFee ?? GetCurrencyTxFeeFromWallet(aCurrencyID);
            if (lBalance == 0 || (aAmount + lTxFee) > lBalance) throw new Exception("Not enough balance to transfer to the exchange.");
            return ExecuteSendTx(aAmount, aCurrencyID, lTxFee, aAddress);
        }

        private decimal GetCurrencyTxFeeFromWallet(long aCurrencyID)
        {
            decimal lTxFee = 0;
            int lCounter = 0;
            string lExceptionMessage = string.Empty;
            do
            {
                lCounter++;
                try
                {
                    lTxFee = PandoraClientControl.GetInstance().CalculateTxFee(aCurrencyID);
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
            return lTxFee;
        }

        /// <summary>
        /// Construct a raw transaction to be sended to server
        /// </summary>
        /// <param name="aAmount">An amount to send</param>
        /// <param name="aCurrencyId">Working currency id</param>
        /// <param name="aTxFee">Transaction calculated txfee</param>
        /// <param name="aExchangeAddress">Destination address (exchange address)</param>
        /// <returns></returns>
        private string ExecuteSendTx(decimal aAmount, long aCurrencyId, decimal aTxFee, string aExchangeAddress)
        {
            string lTx = FPandoraClientControl.GetNewSignedTransaction(GetFormCurrency(aCurrencyId), aExchangeAddress, aAmount, aTxFee);
            long lTxHandle = FPandorasWalletConnection.DirectSendTransaction(aCurrencyId, lTx);

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
        private void TryToCreateNewExchangeTransaction(decimal aPriceTarget, decimal aStopPrice, decimal aAmount, decimal? aTxFee = null)
        {
            if (FPandoraClientControl.FromUserDecryptWallet(false))
            {
                string lTempGUID = Guid.NewGuid().ToString("N");

                UserTradeOrder lNewOrder = new UserTradeOrder
                {
                    CoinTxID = lTempGUID,
                    SentQuantity = aAmount,
                    Market = FExchangeSelectedMarket ?? throw new Exception("No market selected"),
                    Rate = aPriceTarget,
                    StopPrice = aStopPrice,
                    Status = OrderStatus.Initial,
                    OpenTime = DateTime.UtcNow,
                    Name = MainForm.ExchangeTransactionName,
                    CoinSendingTxFee = aTxFee
                };
                UserTradeOrder lOrderWithID = FOrderManager.SaveNewOrder(lNewOrder);
                FOrderManager.WriteTransactionLogEntry(lOrderWithID);
                AddCurrencyOrderToGUI(ActiveCurrencyID, lOrderWithID);
                SetGUILastTransaction(lOrderWithID);
                string lCacheLastOrdersKey = string.Concat(ActiveCurrencyID, lOrderWithID.Market.MarketID);
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