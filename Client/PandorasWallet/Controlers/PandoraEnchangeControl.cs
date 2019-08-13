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
using Pandora.Client.Exchange;
using Pandora.Client.Exchange.SaveManagers;
using Pandora.Client.PandorasWallet.Dialogs;
using Pandora.Client.PandorasWallet.Wallet;
using Pandora.Client.Universal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet.Controlers
{
    public class PandoraEnchangeControl : IDisposable
    {
        private PandoraExchanger FExchanger;
        private ConcurrentBag<PandoraExchanger.ExchangeMarket> FExchangeCoinMarket;
        private LoginExchanger FExchangeLogin;
        private ConcurrentDictionary<long, List<MarketOrder>> FExchangeCurrencyOrders;
        private CancellationTokenSource FExchangeTaskCancellationSource;
        private Task[] FExchangeTasks;
        private PandoraExchanger.ExchangeMarket FExchangeSelectedCoin;
        private PandoraExchangeSQLiteSaveManager FDBExchanger;
        private string FLastExchangeSelectedCurrency = "";
        private PandoraClientControl FPandoraClientControl;
        private ServerAccess.ServerConnection FPandorasWalletConnection { get;
            set;
        }
        private bool FDisableUpdating;

        private readonly object FExchangeInterfaceLock = new object();

        private decimal FCacheTotal = -1;
        private decimal FCacheQuantity = -1;

        private int FPreviosOrderHistory = -1;
        private ExchangeKeyValueHelper<ExchangeKeyValueObject> FKeyValueHelper;

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

        public List<CurrencyItem> UserCoins { get => FPandorasWalletConnection.GetDisplayedCurrencies(); }
        public long ActiveCurrencyID { get; private set; }
        public AppMainForm MainForm { get; private set; }

        public string SqLiteDbFileName => PandoraExchangeSQLiteSaveManager.BuildPath(PandoraClientControl.GetExistingInstance().Settings.DataPath, FPandorasWalletConnection.InstanceId);

        public void ValidLocalDBFile(string aExchangeFilePath)
        {
            var lDBManager = PandoraExchangeSQLiteSaveManager.GetReadOnlyInstance(aExchangeFilePath);
            lDBManager.GetUserData(out string lFileUsername, out string lFileEmail);
            if (FPandorasWalletConnection.UserName != lFileUsername || FPandorasWalletConnection.Email != lFileEmail)
                throw new Exception($"The file does not belong to user {FPandorasWalletConnection.UserName} ({FPandorasWalletConnection.Email}).");
            ExchangeInitialize();
        }

        public void BeginBackupRestore()
        {
            if (Started)
                StopTasks();
        }

        public void EndBackupRestore()
        {
            if (Started)
                StartTaks();
            else
                StartProcess();
        }

        private void StartTaks()
        {
            FExchangeTaskCancellationSource = new CancellationTokenSource();
            FExchangeTasks[0] = Task.Run(() => LocalOrderScanTask(), FExchangeTaskCancellationSource.Token);
            FExchangeTasks[1] = Task.Run(() => RemoteOrderScanTask(), FExchangeTaskCancellationSource.Token);
        }

        public bool Started { get; private set; }

        public void StartProcess()
        {
            if (Started) return;
            var lPriceUpdatingStarter = Task.Run(() =>
            {
                try
                {
                    FExchanger.StartMarketPriceUpdating();
                }
                catch (Exception ex)
                {
                    Log.Write("Error starting market price updating. Details: {0}", ex);
                }
            });
            try
            {
                FExchangeTasks = new Task[3];
                InitializeOrderDBHandler();
                FKeyValueHelper = new ExchangeKeyValueHelper<ExchangeKeyValueObject>(FDBExchanger);
                ActiveCurrencyID = FPandorasWalletConnection.GetDefaultCurrency().Id;
                StartTaks();
                List<string> lCurrencies = new List<string>
            {
                "Bittrex"
            };

                MainForm.AddExchanges(lCurrencies);
                var lUserCoins = UserCoins;
                FDataLoadingTask = Task.Run(() =>
                {
                    foreach (CurrencyItem lCurrencyItem in lUserCoins)
                        LoadNewCurrencyData(lCurrencyItem);
                }, FExchangeTaskCancellationSource.Token);
                lPriceUpdatingStarter.Wait(2000);
                if (lPriceUpdatingStarter.Exception != null)
                    throw lPriceUpdatingStarter.Exception.Flatten();
                Started = true;
            }
            catch (Exception ex)
            {
                Log.Write(LogLevel.Error, $"Failed to start exchange process. Exception {ex}");
                StopProcess();
                throw;
            }
        }

        public void LoadNewCurrencyData(CurrencyItem aCurrencyItem)
        {
            if (!FExchangeCurrencyOrders.ContainsKey(aCurrencyItem.Id))
                FExchangeCurrencyOrders[aCurrencyItem.Id] = new List<MarketOrder>();
            if (!FExchangeCurrencyOrders[aCurrencyItem.Id].Any())
                FExchangeCurrencyOrders[aCurrencyItem.Id].AddRange(FExchanger.LoadTransactions(aCurrencyItem.Ticker));
            PandoraExchanger.ExchangeMarket[] lMarkets = FExchanger.GetMarketCoins(aCurrencyItem.Ticker);
            PandoraExchanger.ExchangeMarket lMarket = lMarkets.ToList().Find(x => x.BaseTicker == aCurrencyItem.Ticker);
            foreach (var lOrder in FExchangeCurrencyOrders)
                AddCurrencyOrdersToGUI(lOrder.Key, lMarket, lOrder.Value, false);
        }

        public void SaveRestoredKeyAndSecret(string aEncryptedKey, string aEncryptedSecret)
        {
            var lKeyValueObject = FKeyValueHelper.LoadKeyValues(0);
            lKeyValueObject.PublicKey = aEncryptedKey;
            lKeyValueObject.PrivateKey = aEncryptedSecret;
            FKeyValueHelper.SaveChanges(lKeyValueObject);
        }

        private void ExchangeInitialize()
        {
            FExchangeLogin = new LoginExchanger();

            FExchanger = PandoraExchanger.GetInstance();
            FExchanger.OnMarketPricesChanging += FExchanger_OnMarketPricesChanging;

            FExchangeCoinMarket = new ConcurrentBag<PandoraExchanger.ExchangeMarket>();
            FExchangeCurrencyOrders = new ConcurrentDictionary<long, List<MarketOrder>>();

            MainForm.OnSelectedCurrencyChanged += MainForm_ExchangeOnSelectedCurrencyChanged; ;
            MainForm.OnExchangeSelectedCurrencyChanged += MainForm_OnExhangeMarketSelectionChanged;
            MainForm.OnLabelEstimatePriceClick += MainForm_OnLabelEstimatePriceClick;
            MainForm.OnExchangeSelectionChanged += MainForm_OnExchangeSelectionChanged;

            MainForm.OnTxtQuantityLeave += MainForm_OnExchangeQuantityTxtChanged;
            MainForm.OnExchangeBtnClick += MainForm_OnExchangeBtnClick;
            //MainForm.OnOrderHistorySelectionChanged += MainForm_OnOrderHistorySelectionChanged;
            MainForm.OnCancelBtnClick += MainForm_OnCancelBtnClick;

            MainForm.OnTxtTotalLeave += MainForm_OnTotalReceivedChanged;
            MainForm.OnCheckAllOrderHistory += MainForm_OnCheckAllOrderHistory;
        }

        private void MainForm_OnCancelBtnClick(int aInternalOrderId)
        {
            MarketOrder lOrder = null;
            long lCurrencyID = 0;
            foreach (var lOrders in FExchangeCurrencyOrders)
            {
                lOrder = lOrders.Value.Find(lOrderItem => lOrderItem.InternalID == aInternalOrderId);
                if (lOrder != null)
                {
                    lCurrencyID = lOrders.Key;
                    break;
                }
            }
            if (lOrder == null) throw new Exception("Order id not found");
            var lWarningString = lOrder.Status == OrderStatus.Completed ? "WARNING!: Cancelling completed orders will cause to prevent withdraw of traded coins." : string.Empty;
            if (!MainForm.AskUserDialog($"Cancel order {lOrder.Name}", $"Do you want to cancel selected order? {Environment.NewLine} {lWarningString}")) return;
            bool lWithdraw = false;
            if (lOrder.Status == OrderStatus.Placed || lOrder.Status == OrderStatus.Waiting)
                lWithdraw = AskUserToWithdrawFunds(lOrder.Name);
            WriteTransactionLogEntry(lOrder, OrderMessage.OrderMessageLevel.Info, string.Format("Order cancellation requested by user. Cancelling..."));
            while (!FCancelledOrderList.TryAdd(lOrder.InternalID, lWithdraw)) ;
        }

        private ConcurrentDictionary<int, bool> FCancelledOrderList = new ConcurrentDictionary<int, bool>();
        private ConcurrentDictionary<int, MarketOrder> FWaitingToRefund = new ConcurrentDictionary<int, MarketOrder>();

        private void ScanForCancelledOrders(IEnumerable<MarketOrder> aOrders, long aCurrencyID)
        {
            foreach (var lOrder in aOrders)
            {
                if (FCancelledOrderList.TryGetValue(lOrder.InternalID, out bool lMustWithdraw) && lOrder.Status != OrderStatus.Withdrawed)
                {
                    var lOriginalStatus = lOrder.Status;
                    bool lCanWithdraw = lOriginalStatus == OrderStatus.Placed || lOriginalStatus == OrderStatus.Waiting;
                    UpdateOrderStatus(lOrder, OrderStatus.Interrupted, aCurrencyID);
                    WriteTransactionLogEntry(lOrder, OrderMessage.OrderMessageLevel.FatalError, string.Format("Order succesfully cancelled."));
                    if (lOriginalStatus == OrderStatus.Placed)
                        TryCancelOrder(lOrder);
                    if (lMustWithdraw && lCanWithdraw)
                        RefundOrder(lOrder, aCurrencyID, lOriginalStatus);
                    while (!FCancelledOrderList.TryRemove(lOrder.InternalID, out bool lDummy)) ;
                }
            }
        }

        private void ProcessOrdersMarkedToRefund(TransactionRecordList aCurrencyTxs, CurrencyItem aCurrencyItem)
        {
            if (!aCurrencyTxs.Any()) return;
            var lExchangeminConf = FExchanger.GetConfirmations(aCurrencyItem.Ticker);
            var lCurrencyBlockHeight = FPandorasWalletConnection.GetBlockHeight(aCurrencyItem.Id);
            if (lExchangeminConf < 0 || lCurrencyBlockHeight <= 0) return;
            foreach (var lOrder in FWaitingToRefund.Values)
            {
                var lTx = aCurrencyTxs.ToList().Find(lTxItem => lTxItem.TxId == lOrder.CoinTxID);
                bool lConfirmed = lTx.Block != 0 && (lCurrencyBlockHeight - lTx.Block) > lExchangeminConf;
                if (lTx != null && lConfirmed)
                {
                    RefundOrder(lOrder, aCurrencyItem.Id);
                    while (!FWaitingToRefund.TryRemove(lOrder.InternalID, out MarketOrder lDummy)) ;
                }
            }
        }

        private void TryCancelOrder(MarketOrder aOrder)
        {
            try
            {
                FExchanger.CancelOrder(aOrder);
                WriteTransactionLogEntry(aOrder, OrderMessage.OrderMessageLevel.Error, $"Order id: {aOrder.ID} successfully cancelled at exchange.");
            }
            catch (Exception ex)
            {
                Universal.Log.Write(Universal.LogLevel.Error, string.Format("Order: {0}. Exception: {1}", aOrder.InternalID, ex.ToString()));
                WriteTransactionLogEntry(aOrder, OrderMessage.OrderMessageLevel.Error, $"Error on cancel remote order: {ex.Message}.");
            }
        }

        private bool AskUserToWithdrawFunds(string aOrderName)
        {
            return MainForm.AskUserDialog("Coin withdraw", $"Do you want to withdraw the order '{aOrderName}' funds from exchange? {Environment.NewLine} Note: Please consider that a tx fee will apply for this operation.");
        }

        private void RefundOrder(MarketOrder aOrderToWithdraw, long aCurrencyID, OrderStatus? aOriginalStatus = null)
        {
            PandoraExchanger.ExchangeMarket lMarket = FExchangeCoinMarket.Where(x => x.MarketName == aOrderToWithdraw.Market && x.BaseTicker == aOrderToWithdraw.BaseTicker).FirstOrDefault();
            if (lMarket == null) return;

            if (aOriginalStatus.HasValue && aOriginalStatus.Value == OrderStatus.Waiting)
            {
                WriteTransactionLogEntry(aOrderToWithdraw, OrderMessage.OrderMessageLevel.Info, $"Waiting for transaction {aOrderToWithdraw.CoinTxID} to be confirmed at exchange to try withdraw coins.");
                while (!FWaitingToRefund.TryAdd(aOrderToWithdraw.InternalID, aOrderToWithdraw)) ;
            }
            else
                TryRefundOrder(lMarket, aOrderToWithdraw, aCurrencyID);
        }

        private void TryRefundOrder(PandoraExchanger.ExchangeMarket aMarket, MarketOrder aOrderToWithdraw, long aCurrencyID)
        {
            try
            {
                WriteTransactionLogEntry(aOrderToWithdraw, OrderMessage.OrderMessageLevel.Info, "Trying to refund coins from exchange.");
                FExchanger.RefundOrder(aMarket, aOrderToWithdraw, FPandorasWalletConnection.GetCoinAddress(aCurrencyID));
                WriteTransactionLogEntry(aOrderToWithdraw, OrderMessage.OrderMessageLevel.Info, "Coins successfully withdrawn. Please wait some minutes to be shown in wallet");
            }
            catch (Exception ex)
            {
                Universal.Log.Write(Universal.LogLevel.Error, string.Format("Order: {0}. Exception: {1}", aOrderToWithdraw.InternalID, ex.ToString()));
                WriteTransactionLogEntry(aOrderToWithdraw, OrderMessage.OrderMessageLevel.Error, $"Error on refund withdraw process in order: {ex.Message}. Please try to manually withdraw your coins from exchange");
            }
        }

        private void MainForm_ExchangeOnSelectedCurrencyChanged(object sender, EventArgs e)
        {
            ActiveCurrencyID = MainForm.SelectedCurrencyId;
            MainForm.BeginInvoke((Action<long>)ExchangePandoraCurrencyChanged, ActiveCurrencyID);
        }

        private void ExchangePandoraCurrencyChanged(long aCurrency)
        {
            FDisableUpdating = true;
            try
            {
                PandoraCurrencyChangedProcess(aCurrency);
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
        private Tuple<CurrencyItem, decimal>[] GetCoinsWithExchangePrice(string aCurrencyTicker)
        {
            List<CurrencyItem> lUserCoins = UserCoins;
            PandoraExchanger.ExchangeMarket[] lFullMarkets = FExchanger.GetMarketCoins(aCurrencyTicker);

            List<PandoraExchanger.ExchangeMarket> lUserPossibleMarkets = lFullMarkets.Where(lMarket => lUserCoins.Exists(lCoin => lCoin.Ticker == lMarket.CoinTicker)).ToList();
            List<Tuple<CurrencyItem, decimal>> lCoinsWithMarket = new List<Tuple<CurrencyItem, decimal>>();

            foreach (PandoraExchanger.ExchangeMarket lMarket in lUserPossibleMarkets)
            {
                CurrencyItem lCoin = lUserCoins.Find(lItem => lItem.Ticker == lMarket.CoinTicker);
                lCoinsWithMarket.Add(new Tuple<CurrencyItem, decimal>(lCoin, lMarket.BasePrice));
            }
            Interlocked.Exchange(ref FExchangeCoinMarket, new ConcurrentBag<PandoraExchanger.ExchangeMarket>(lUserPossibleMarkets));
            return lCoinsWithMarket.ToArray();
        }

        /// <summary>
        /// Triggers with event OnCurrencyItemSelectionChanged. Must run only on MainForm owner thread
        /// </summary>
        /// <param name="aCurrency">Selected currency id</param>
        private void PandoraCurrencyChangedProcess(long aCurrency)
        {
            MainForm.ClearListExchangeTo();
            MainForm.StatusControlExchange.ClearStatusList();
            MainForm.StatusControlOrderHistory.ClearStatusList();

            if (string.IsNullOrEmpty(MainForm.SelectedExchange) || !FExchanger.IsCredentialsSet)
                return;

            Tuple<CurrencyItem, decimal>[] lCoinsWithMarket = GetCoinsWithExchangePrice(FPandorasWalletConnection.GetCurrency(aCurrency).Ticker);
            foreach (Tuple<CurrencyItem, decimal> lCurrencyItemWithPrice in lCoinsWithMarket)
            {
                CurrencyItem lCurrencyItem = lCurrencyItemWithPrice.Item1;
                decimal lPrice = lCurrencyItemWithPrice.Item2;
                MainForm.AddCoinExchangeTo(lCurrencyItem.Id, lCurrencyItem.Name, lCurrencyItem.Ticker, lPrice);
            }

            MainForm.ExchangeSelectCurrency(FExchangeCoinMarket.FirstOrDefault()?.CoinName);
            if (!FExchangeCoinMarket.Any())
            {
                FExchangeSelectedCoin = null;
                DisableExchangeInterface();
            }

            LoadOrderAcordingToAllCheckbox(aCurrency);
            FPreviosOrderHistory = -1;
            FLastExchangeSelectedCurrency = string.Empty;
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
            if (lAmountTrade < (decimal)0.0005)
                throw new ClientExceptions.InvalidOperationException("Sell Quantity should be greater than 0.0005");
            decimal lAmounTradeSize = FExchangeSelectedCoin.IsSell ? MainForm.ExchangeQuantity : MainForm.ExchangeTotalReceived;
            if (FExchangeSelectedCoin.MinimumTrade >= (lAmounTradeSize))
                throw new ClientExceptions.InvalidOperationException(string.Format("Sell minimum trade size is {0} {1}", FExchangeSelectedCoin.MinimumTrade, FExchangeSelectedCoin.BaseTicker));
            if (lAmountTrade > FPandoraClientControl.GetBalance(ActiveCurrencyID))
                throw new ClientExceptions.InvalidOperationException(string.Format("Not enough balance for this trade size"));
            TryToCreateNewExchangeTransaction(MainForm.ExchangeTargetPrice, MainForm.ExchangeStopPrice, MainForm.ExchangeQuantity, FExchangeSelectedCoin);
        }

        private void SetGUILastTransaction(MarketOrder aOrder)
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

        private void MainForm_OnExchangeQuantityTxtChanged(object sender, EventArgs e)
        {
            if (MainForm.ExchangeTargetPrice > 0 && MainForm.ExchangeQuantity > 0 && (MainForm.ExchangeQuantity != FCacheQuantity || MainForm.ExchangeTotalReceived != FCacheTotal))
            {
                List<CurrencyItem> lCoinToSell = UserCoins;
                CurrencyItem lPresicion = lCoinToSell.Find(x => x.Ticker == FExchangeSelectedCoin.BaseTicker);
                decimal lTotal = Math.Round(MainForm.ExchangeQuantity / (FExchangeSelectedCoin.IsSell ? 1 / MainForm.ExchangeTargetPrice : MainForm.ExchangeTargetPrice), FPandorasWalletConnection.GetDefaultCurrency().Precision);
                decimal lComision = lTotal * (decimal)0.0025;
                MainForm.ExchangeTotalReceived = Math.Round(lTotal - lComision, lPresicion.Precision);
                FCacheTotal = MainForm.ExchangeTotalReceived;
                FCacheQuantity = MainForm.ExchangeQuantity;
            }
        }

        private void MainForm_OnCheckAllOrderHistory(object sender, EventArgs e)
        {
            LoadOrderAcordingToAllCheckbox(ActiveCurrencyID);
        }

        private void MainForm_OnTotalReceivedChanged(object sender, EventArgs e)
        {
            if (MainForm.ExchangeTotalReceived > 0 && FCacheTotal != MainForm.ExchangeTotalReceived && (MainForm.ExchangeQuantity != FCacheQuantity || MainForm.ExchangeTotalReceived != FCacheTotal))
            {
                List<CurrencyItem> lCoinToSell = UserCoins;
                CurrencyItem lPresicion = lCoinToSell.Find(x => x.Ticker == FExchangeSelectedCoin.BaseTicker);
                decimal lTotal = Math.Round(MainForm.ExchangeTotalReceived * (FExchangeSelectedCoin.IsSell ? 1 / MainForm.ExchangeTargetPrice : MainForm.ExchangeTargetPrice), FPandorasWalletConnection.GetDefaultCurrency().Precision);
                decimal lComision = lTotal * (decimal)0.0025;
                MainForm.ExchangeQuantity = Math.Round(lTotal - lComision, lPresicion.Precision);
                FCacheTotal = MainForm.ExchangeTotalReceived;
                FCacheQuantity = MainForm.ExchangeQuantity;
            }
        }

        private void MainForm_OnLabelEstimatePriceClick(object sender, EventArgs e)
        {
            MainForm.ExchangeTargetPrice = Convert.ToDecimal(MainForm.LabelEstimatePrice);
        }

        private void MainForm_OnExchangeSelectionChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MainForm.SelectedExchange))
            {
                if (MainForm.IsHandleCreated)
                    MainForm.BeginInvoke(new MethodInvoker(() =>
                    {
                        DisableExchangeInterface();
                        MainForm.ClearListExchangeTo();
                        MainForm.ClearOrderHistory();
                    }));
                return;
            }
            try
            {
                if (!FExchanger.IsCredentialsSet)
                {
                    if (GetKeyManagerMethod.Invoke(out KeyManager lKeyManager))
                    {
                        var lKeyValueObject = FKeyValueHelper.LoadKeyValues(0); //WHERE PROFILE 0 IS THE DEFAULT AND INITIAL ONE
                        if (!string.IsNullOrEmpty(lKeyValueObject.PublicKey) && !string.IsNullOrEmpty(lKeyValueObject.PrivateKey))
                            try
                            {
                                if (!lKeyManager.IsPasswordSet)
                                    SetKeyManagerPassword.Invoke();
                                var lExchangePublicKey = lKeyManager.DecryptText(lKeyValueObject.PublicKey);
                                var lExchangeSecret = lKeyManager.DecryptText(lKeyValueObject.PrivateKey);
                                SetExchangeCredentials(lExchangePublicKey, lExchangeSecret);
                                return;
                            }
                            catch (PandoraExchangeExceptions.InvalidExchangeCredentials ex)
                            {
                                Log.Write(LogLevel.Warning, $"Exchange credentials no longer valid. New credentials are asked. Exception: {ex}");
                                MainForm.StandardWarningMsgBox("Credentials saved no longer valid. Please add new key pair on next window.");
                            }

                        if (FExchangeLogin.Execute())
                        {
                            SetExchangeCredentials(FExchangeLogin.ExchageKey, FExchangeLogin.ExchangeSecret);
                            if (!lKeyManager.IsPasswordSet)
                                SetKeyManagerPassword.Invoke();
                            lKeyValueObject.PublicKey = lKeyManager.EncryptText(FExchangeLogin.ExchageKey);
                            lKeyValueObject.PrivateKey = lKeyManager.EncryptText(FExchangeLogin.ExchangeSecret);
                            FKeyValueHelper.SaveChanges(lKeyValueObject);
                        }
                    }
                }
            }
            finally
            {
                if (!FExchanger.IsCredentialsSet)
                    MainForm.SelectedExchange = "0";
                else
                {
                    EnableExchangeInterface();
                    ExchangePandoraCurrencyChanged(ActiveCurrencyID);
                    if (!FDataLoadingTask.Wait(60000))
                        throw new Exception("Failed to load data from database. Please try again");
                    MainForm.BeginInvoke((Action<long>)MainForm.LoadCurrencyExchangeOrders, ActiveCurrencyID);
                }
            }
        }

        private void MainForm_OnExhangeMarketSelectionChanged(object sender, EventArgs e)
        {
            if (MainForm.SelectedExchangeMarket != null)
            {
                //Even if the reference does not change anywhere else, this maybe will be used by others threads so I want this to be an atomic operation
                Interlocked.Exchange(ref FExchangeSelectedCoin, FExchangeCoinMarket.ToList().Find(x => MainForm.SelectedExchangeMarket.SubItems[1].Text == x.CoinTicker));
                MainForm.LabelEstimatePrice = FExchangeSelectedCoin.BasePrice.ToString();
                MainForm.LabelPriceInCoin = "BTC";
                MainForm.LabelTotalCoinReceived = string.Format("{0} ({1})", MainForm.SelectedExchangeMarket.SubItems[0].Text, MainForm.SelectedExchangeMarket.SubItems[1].Text);
                if (!string.IsNullOrEmpty(FLastExchangeSelectedCurrency) && FLastExchangeSelectedCurrency == MainForm.SelectedExchangeMarket.SubItems[1].Text)
                    return;
                FCacheQuantity = FCacheTotal = -1;
                MainForm.ExchangeTargetPrice = FExchangeSelectedCoin.BasePrice;
                MainForm.ExchangeStopPrice = FExchangeSelectedCoin.BasePrice;
                MainForm.ExchangeTransactionName = string.Format("{0} -> {1} - {2}", FExchangeSelectedCoin.BaseTicker, MainForm.SelectedExchangeMarket.SubItems[1].Text, DateTime.Now.ToString());
                FPreviosOrderHistory = -1;
                FLastExchangeSelectedCurrency = MainForm.SelectedExchangeMarket.SubItems[1].Text;
                EnableExchangeInterface();
                MainForm_OnExchangeQuantityTxtChanged(null, null); ;
                ChangeLastOrderAcordingToMarket(FExchangeSelectedCoin);
            }
        }

        private Dictionary<string, MarketOrder> FCacheLastOrders = new Dictionary<string, MarketOrder>();
        private Task FDataLoadingTask;

        private void ChangeLastOrderAcordingToMarket(PandoraExchanger.ExchangeMarket aMarket)
        {
            string lCacheKey = string.Concat(ActiveCurrencyID, aMarket.MarketName);
            if (!FCacheLastOrders.TryGetValue(lCacheKey, out MarketOrder lMarketOrder))
            {
                var lMarketOrders = FExchangeCurrencyOrders[ActiveCurrencyID].Where(lOrder => lOrder.Market == aMarket.MarketName);
                lMarketOrder = lMarketOrders.Any() ? lMarketOrders.Aggregate((lOrder1, lOrder2) => lOrder1.OpenTime > lOrder2.OpenTime ? lOrder1 : lOrder2) : null;
                if (lMarketOrder != null) FCacheLastOrders.Add(lCacheKey, lMarketOrder);
            }
            SetGUILastTransaction(lMarketOrder);
        }

        private void AddCurrencyOrdersToGUI(long aCurrencyID, PandoraExchanger.ExchangeMarket aMarket, IEnumerable<MarketOrder> aMarketOrders, bool aUpdateGUI = true)
        {
            if (aMarket == null) return;
            foreach (var lMarketOrder in aMarketOrders)
            {
                decimal lRate = aMarket.IsSell ? 1 / lMarketOrder.Rate : lMarketOrder.Rate;
                decimal lRawAmount = lMarketOrder.SentQuantity / lRate;
                decimal lQuantity = Math.Round(lRawAmount - FExchanger.GetTransactionsFee(aMarket.CoinTicker) - (lRawAmount * (decimal)0.0025), FPandorasWalletConnection.GetDefaultCurrency().Precision);
                var lOrderView = new AppMainForm.ExchangeOrderViewModel(lMarketOrder, new AppMainForm.ExchangeOrderViewModel.ExchangeOrderViewModelContextData
                {
                    ExchangeFee = FExchanger.GetTransactionsFee(aMarket.CoinTicker),
                    MainForm = MainForm,
                    Market = aMarket,
                    Precision = FPandorasWalletConnection.GetDefaultCurrency().Precision,
                    TradeComission = 0.0025M
                });
                FDBExchanger.ReadOrderLogs(lMarketOrder.InternalID, out List<OrderMessage> lMessages);
                lOrderView.AddLog(lMessages.Select(lMessage => new AppMainForm.ExchangeOrderLogViewModel
                {
                    ID = lMessage.ID,
                    Message = lMessage.Message,
                    Time = lMessage.Time.ToLocalTime().ToString()
                }));
                var lAsyncResult = MainForm.BeginInvoke(new MethodInvoker(() => MainForm.AddOrUpdateOrder(lOrderView, aCurrencyID, aUpdateGUI)));
                MainForm.EndInvoke(lAsyncResult);
            }
        }

        private int RandomNumber(int aMin, int aMax)
        {
            Random random = new Random();
            return random.Next(aMin, aMax);
        }

        private bool MyDelayCanceled(int aDelaySeconds, CancellationToken aCancellationToken)
        {
            aDelaySeconds *= 10;
            while (aDelaySeconds-- > 0 && !aCancellationToken.IsCancellationRequested)
                Thread.Sleep(100);
            return aCancellationToken.IsCancellationRequested;
        }

        private void FExchanger_OnMarketPricesChanging(IEnumerable<string> aMarketBalanceChanged)
        {
            if (FPandorasWalletConnection != null && FExchanger.IsCredentialsSet)
            {
                foreach (PandoraExchanger.ExchangeMarket lMarket in FExchangeCoinMarket.Where(lMarket => aMarketBalanceChanged.Contains(lMarket.MarketName)))
                {
                    if (FDisableUpdating) return;
                    CurrencyItem lCurrency = UserCoins.Find(X => X.Ticker == lMarket.CoinTicker);
                    if (FExchangeSelectedCoin != null)
                        MainForm?.BeginInvoke(new MethodInvoker(delegate ()
                        {
                            if (!(MainForm.Disposing || MainForm.IsDisposed))
                            {
                                MainForm.AddCoinExchangeTo(lCurrency.Id, lCurrency.Name, lCurrency.Ticker, lMarket.BasePrice);
                                MainForm.LabelEstimatePrice = FExchangeSelectedCoin.BasePrice.ToString();
                            }
                        }));
                }
            }
        }

        private void InitializeOrderDBHandler()
        {
            PandoraExchangeSQLiteSaveManager lOldDBExchanger = null;
            if (FDBExchanger != null)
            {
                lOldDBExchanger = FDBExchanger;
            }
            FDBExchanger = new PandoraExchangeSQLiteSaveManager(SqLiteDbFileName, FPandorasWalletConnection.UserName, FPandorasWalletConnection.Email);
            FExchanger.TransactionHandler = FDBExchanger;
            lOldDBExchanger?.Dispose();
        }

        public void StopProcess()
        {
            if (!Started) return;
            StopTasks();
            ResetInterface();
            ClearResources();
        }

        private void StopTasks()
        {
            FDisableUpdating = true;
            try
            {
                FExchangeTaskCancellationSource?.Cancel();
                if (FExchangeTasks != null)
                {
                    var lTasks = FExchangeTasks.Where(x => x != null);

                    Task.WaitAll(lTasks.ToArray());
                }
            }
            catch (OperationCanceledException e)
            {
                Log.Write(LogLevel.Info, e.Message);
            }
            catch (AggregateException e)
            {
                Log.Write(LogLevel.Info, e.Message);
            }
        }

        private void ClearResources()
        {
            FExchanger?.Clear();
            FExchangeCoinMarket = new ConcurrentBag<PandoraExchanger.ExchangeMarket>();
            FExchangeCurrencyOrders?.Clear();
            FDBExchanger = null;
            Started = false;
        }

        private void ResetInterface()
        {
            FLastExchangeSelectedCurrency = string.Empty;
            FPreviosOrderHistory = -1;
            if (MainForm != null)
            {
                DisableExchangeInterface();
                MainForm.ClearListExchangeTo();
                MainForm.LabelEstimatePrice = "";
                MainForm.SelectedExchange = "-1";
            }
        }

        private void RemoteOrderScanTask()
        {
            try
            {
                if (FPandorasWalletConnection != null && FExchanger.IsCredentialsSet)
                {
                    foreach (var lOrderList in FExchangeCurrencyOrders)
                    {
                        var lPlacedOrders = lOrderList.Value.Where(x => x.Status == OrderStatus.Placed);
                        VerifyPlacedOrders(lPlacedOrders, lOrderList.Key);
                        var lCompletedOrders = lOrderList.Value.Where(x => x.Status == OrderStatus.Completed);
                        VerifyCompletedOrders(lCompletedOrders, lOrderList.Key);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write("Error on Checking order statuses. Details: " + ex.Message + " on " + ex.Source);
            }
            finally
            {
                if (!MyDelayCanceled(RandomNumber(5, 7), FExchangeTaskCancellationSource.Token))
                    FExchangeTasks[2] = Task.Run(() => RemoteOrderScanTask(), FExchangeTaskCancellationSource.Token);
            }
        }

        private void VerifyPlacedOrders(IEnumerable<MarketOrder> aOrders, long aCurrencyID)
        {
            ScanForCancelledOrders(aOrders, aCurrencyID);
            foreach (var lOrder in aOrders)
            {
                MarketOrder lRemoteOrder = FExchanger.GetOrder(new Guid(lOrder.ID));
                if (!lRemoteOrder.Completed && !lRemoteOrder.Cancelled)
                    continue;
                lOrder.Completed = lRemoteOrder.Completed;
                lOrder.Cancelled = lRemoteOrder.Cancelled;
                OrderStatus lStatus = OrderStatus.Placed;
                if (lOrder.Completed)
                    lStatus = OrderStatus.Completed;
                if (lOrder.Cancelled)
                    lStatus = OrderStatus.Interrupted;
                UpdateOrderStatus(lOrder, lStatus, aCurrencyID);
                WriteTransactionLogEntry(lOrder);
            }
        }

        private void VerifyCompletedOrders(IEnumerable<MarketOrder> aOrders, long aCurrencyID)
        {
            ScanForCancelledOrders(aOrders, aCurrencyID);
            foreach (MarketOrder lOrder in aOrders)
                TryToWithdrawOrder(lOrder);
        }

        private void TryToWithdrawOrder(MarketOrder aOrder)
        {
            if (aOrder.Status != OrderStatus.Completed)
            {
                return;
            }

            PandoraExchanger.ExchangeMarket lMarket = FExchangeCoinMarket.Where(x => x.MarketName == aOrder.Market && x.BaseTicker == aOrder.BaseTicker).FirstOrDefault();

            if (lMarket == null)
            {
                return;
            }

            long lCurrencyID = UserCoins.Where(x => x.Ticker == lMarket.CoinTicker).Select(x => x.Id).First();

            if (aOrder.ErrorCounter % 60 == 0)
            {
                try
                {
                    if (FExchanger.WithdrawOrder(lMarket, aOrder, FPandorasWalletConnection.GetCoinAddress(lCurrencyID), FExchanger.GetTransactionsFee(aOrder.BaseTicker)))
                        UpdateOrderStatus(aOrder, OrderStatus.Withdrawed, lCurrencyID);
                    WriteTransactionLogEntry(aOrder);
                    aOrder.ErrorCounter = 0;
                }
                catch (Exception ex)
                {
                    int lNumberofretrys = aOrder.ErrorCounter / 60;

                    if (lNumberofretrys >= 9)
                    {
                        aOrder.Cancelled = true;
                        UpdateOrderStatus(aOrder, OrderStatus.Interrupted, lCurrencyID);
                        WriteTransactionLogEntry(aOrder);
                    }

                    aOrder.ErrorCounter += 1;
                    Universal.Log.Write(Universal.LogLevel.Error, string.Format("Order: {0}. Exception: {1}", aOrder.InternalID, ex.ToString()));
                    WriteTransactionLogEntry(aOrder, OrderMessage.OrderMessageLevel.Error, "Error on Withdraw Order: " + ex.Message);
                    WriteTransactionLogEntry(aOrder, OrderMessage.OrderMessageLevel.Info, string.Format("Retrying in 5 minute. Attempt: {0}/10", (lNumberofretrys + 1)));
                }
            }
            else
            {
                aOrder.ErrorCounter++;
            }
        }

        private void DisableExchangeInterface()
        {
            MainForm.ExchangeTargetPrice = 0;
            MainForm.ExchangeTargetPriceEnabled = false;
            MainForm.ExchangeQuantity = 0;
            MainForm.ExchangeQuantityEnabled = false;
            MainForm.ExchangeTotalReceived = 0;
            MainForm.ExchangeTotalReceivedEnabled = false;
            MainForm.ExchangeTransactionName = string.Empty;
            MainForm.ExchangeTransactionNameEnabled = false;
            MainForm.CheckAllOrderHistoryEnabled = false;
            MainForm.ExchangeButtonEnabled = false;
            MainForm.ExchangeStoptPriceEnabled = false;
        }

        private void EnableExchangeInterface()
        {
            MainForm.ExchangeStoptPriceEnabled = true;
            MainForm.ExchangeTargetPriceEnabled = true;
            MainForm.ExchangeQuantityEnabled = true;
            MainForm.ExchangeTotalReceivedEnabled = true;
            MainForm.ExchangeTransactionNameEnabled = true;
            MainForm.CheckAllOrderHistoryEnabled = true;
            MainForm.ExchangeButtonEnabled = true;
        }

        private void SetExchangeCredentials(string aKey, string aSecret)
        {
            FExchanger.SetCredentials(aKey, aSecret);
        }

        /// <summary>
        /// Tries to form a transaction that meets the order requeriments and then sends it to the exchange user address. Also does amount verification
        /// </summary>
        /// <param name="aOrder">Order to fulfill</param>
        /// <param name="aMarket">Order's market</param>
        /// <param name="aCurrencyID">Selected currency id</param>
        private void TryToTransferMoneyToExchange(MarketOrder aOrder, PandoraExchanger.ExchangeMarket aMarket, long aCurrencyID)
        {
            try
            {
                WriteTransactionLogEntry(aOrder, OrderMessage.OrderMessageLevel.Info, string.Format("Stop price reached, trying to send coins to exchange.", FExchanger.GetConfirmations(aMarket.BaseTicker)));
                string lExchangeAddress = FExchanger.GetDepositAddress(aMarket);
                decimal lAmount = aMarket.IsSell ? aOrder.SentQuantity : aOrder.SentQuantity + (aOrder.SentQuantity * (decimal)0.025);
                decimal lTxFee = 0;
                int lCounter = 0;
                do
                {
                    lCounter++;
                    try
                    {
                        lTxFee = PandoraClientControl.GetInstance().CalculateTxFee(lExchangeAddress, lAmount, aCurrencyID);
                        break;
                    }
                    catch
                    {
                        continue;
                    }
                } while (lCounter < 6);
                if (lTxFee == 0) throw new Exception("Failed to get transaction txfee");
                decimal lBalance = PandoraClientControl.GetInstance().GetBalance(aCurrencyID);
                decimal lDecimalTxFee = (lTxFee / (decimal)Math.Pow(10, FPandorasWalletConnection.GetDefaultCurrency().Precision));
                if (lBalance == 0 || (lAmount + lDecimalTxFee) > lBalance) throw new Exception("Not enough balance to transfer to the exchange");
                Thread.Sleep(5000);
                string lTxID = ExecuteSendTx(lAmount, aCurrencyID, lTxFee, lExchangeAddress);
                if (string.IsNullOrEmpty(lTxID)) throw new Exception("Unable to broadcast transaction");
                aOrder.CoinTxID = lTxID;
                UpdateOrderStatus(aOrder, OrderStatus.Waiting, aCurrencyID);
                WriteTransactionLogEntry(aOrder);
                WriteTransactionLogEntry(aOrder, OrderMessage.OrderMessageLevel.Info, string.Format("Number of confirmations needed: {0} confirmations", FExchanger.GetConfirmations(aMarket.BaseTicker)));
            }
            catch (Exception ex)
            {
                UpdateOrderStatus(aOrder, OrderStatus.Interrupted, aCurrencyID);
                WriteTransactionLogEntry(aOrder, OrderMessage.OrderMessageLevel.FatalError, string.Format("Failed to send transaction. Details: {0}.", ex.Message));
            }
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
            string lTx = PandoraClientControl.GetInstance().CreateSignedTransaction(aExchangeAddress, aAmount, aTxFee, aActiveCurrencyID);
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
        private void TryToCreateNewExchangeTransaction(decimal aPriceTarget, decimal aStopPrice, decimal aAmount, PandoraExchanger.ExchangeMarket aMarket)
        {
            if (FPandoraClientControl.FromUserDecryptWallet(false))
            {
                string lTempGUID = Guid.NewGuid().ToString("N");

                MarketOrder lNewOrder = new MarketOrder
                {
                    CoinTxID = lTempGUID,
                    SentQuantity = aAmount,
                    Market = aMarket.MarketName,
                    Rate = aPriceTarget,
                    StopPrice = aStopPrice,
                    Status = OrderStatus.Initial,
                    BaseTicker = aMarket.BaseTicker,
                    OpenTime = DateTime.UtcNow,
                    Name = MainForm.ExchangeTransactionName
                };

                FDBExchanger.WriteTransaction(lNewOrder);
                FDBExchanger.ReadTransactions(out MarketOrder[] lOrders, aMarket.BaseTicker);
                MarketOrder lOrderWithID = lOrders.Where(x => x.CoinTxID == lTempGUID).First();
                if (!FExchangeCurrencyOrders.ContainsKey(ActiveCurrencyID))
                    FExchangeCurrencyOrders[ActiveCurrencyID] = new List<MarketOrder>();
                FExchangeCurrencyOrders[ActiveCurrencyID].Add(lOrderWithID);
                var lCurrency = FPandoraClientControl.GetCurrency(ActiveCurrencyID);

                PandoraExchanger.ExchangeMarket[] lMarkets = FExchanger.GetMarketCoins(lCurrency.Ticker);
                PandoraExchanger.ExchangeMarket lMarket = lMarkets.ToList().Find(x => x.BaseTicker == lCurrency.Ticker);
                AddCurrencyOrdersToGUI(ActiveCurrencyID, lMarket, new[] { lOrderWithID });

                WriteTransactionLogEntry(lOrderWithID);
                SetGUILastTransaction(lOrderWithID);
                string lCacheLastOrdersKey = string.Concat(ActiveCurrencyID, lOrderWithID.Market);
                FCacheLastOrders[lCacheLastOrdersKey] = lOrderWithID;
            }
        }

        /// <summary>
        /// Performs a scan of all orders and executes actions as needed
        /// </summary>
        private void LocalOrderScanTask()
        {
            try
            {
                if (FPandorasWalletConnection != null && FExchanger.IsCredentialsSet)
                {
                    foreach (CurrencyItem lCoin in UserCoins)
                    {
                        if (FExchangeCurrencyOrders.TryGetValue(lCoin.Id, out List<MarketOrder> lOrders))
                        {
                            if (!lOrders.Any()) continue;
                            List<PandoraExchanger.ExchangeMarket> lExchangeCoinMarkets;
                            try
                            {
                                lExchangeCoinMarkets = FExchanger.GetMarketCoins(lCoin.Ticker).ToList();
                            }
                            catch (Exception ex)
                            {
                                Universal.Log.Write(Universal.LogLevel.Error, string.Format("{0}: Exception in getmarketcoins. {1}", lCoin.Name, ex));
                                continue;
                            }
                            List<MarketOrder> lOrdersInitial = lOrders.Where(lOrder => lOrder.Status == OrderStatus.Initial).ToList();
                            VerifyInitialOrders(lOrdersInitial, lExchangeCoinMarkets, lCoin.Ticker, lCoin.Id);
                            List<MarketOrder> lOrderWaiting = lOrders.Where(lOrder => lOrder.Status == OrderStatus.Waiting).ToList();
                            if (lOrderWaiting.Any() || FWaitingToRefund.Any())
                            {
                                var lCurrencyTxs = FPandorasWalletConnection.GetTransactionRecords(lCoin.Id);
                                ProcessOrdersMarkedToRefund(lCurrencyTxs, lCoin);
                                VerifyWaitingOrders(lOrderWaiting, lCoin, lCurrencyTxs, lExchangeCoinMarkets);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Universal.Log.Write(Universal.LogLevel.Error, string.Format("Failed to Update Exchange Order. Details: {0}", ex));
            }
            finally
            {
                if (!MyDelayCanceled(RandomNumber(1, 3), FExchangeTaskCancellationSource.Token))
                    FExchangeTasks[1] = Task.Run(() => LocalOrderScanTask(), FExchangeTaskCancellationSource.Token);
            }
        }

        /// <summary>
        /// Checks order that are in initial state, and changes it to waiting state if is needed (Here the stop price is verified)
        /// </summary>
        /// <param name="aInitialOrders">Orders with initial state</param>
        /// <param name="aListExchangeCoinMarket">Markets to look fo</param>
        /// <param name="aTicker">Order currency ticker</param>
        /// <param name="aCurrencyID">Order currency id</param>
        private void VerifyInitialOrders(List<MarketOrder> aInitialOrders, List<PandoraExchanger.ExchangeMarket> aListExchangeCoinMarket, string aTicker, long aCurrencyID)
        {
            ScanForCancelledOrders(aInitialOrders, aCurrencyID);
            if (!aInitialOrders.Any())
                return;
            foreach (MarketOrder lOrder in aInitialOrders)
            {
                decimal lTargetPrice = lOrder.Rate;
                PandoraExchanger.ExchangeMarket lMarket = aListExchangeCoinMarket.Find(x => x.BaseTicker == aTicker && lOrder.Market == x.MarketName);
                bool lPlaceOrderFlag = false;
                if (lMarket.IsSell)
                    lPlaceOrderFlag = lOrder.StopPrice <= lMarket.BasePrice;
                else
                    lPlaceOrderFlag = lOrder.StopPrice >= lMarket.BasePrice;

                if (lPlaceOrderFlag)
                    TryToTransferMoneyToExchange(lOrder, lMarket, aCurrencyID);
            }
        }

        /// <summary>
        /// Checks order that are in waiting state, and changes it to placed state if is needed (Here transaction confirmations are verified))
        /// </summary>
        /// <param name="aWaitingOrders">Orders with waiting state</param>
        /// <param name="aCurrencyItem">Order currency item</param>
        /// <param name="aCurrencyTransactions">Pandora's currency transactions</param>
        /// <param name="aListExchangeCoinMarket">Markets to look for</param>
        private void VerifyWaitingOrders(List<MarketOrder> aWaitingOrders, CurrencyItem aCurrencyItem, IEnumerable<TransactionRecord> aCurrencyTransactions, List<PandoraExchanger.ExchangeMarket> aListExchangeCoinMarket)
        {
            long lCurrencyBlockHeight;
            int lExchangeminConf;
            ScanForCancelledOrders(aWaitingOrders, aCurrencyItem.Id);
            if (!aWaitingOrders.Any() || !aCurrencyTransactions.Any()
                || (lExchangeminConf = FExchanger.GetConfirmations(aCurrencyItem.Ticker)) < 0
                || (lCurrencyBlockHeight = FPandorasWalletConnection.GetBlockHeight(aCurrencyItem.Id)) <= 0) return;
            var lConfirmedTxs = aCurrencyTransactions.Where(lTx => lTx.Block != 0 && (lCurrencyBlockHeight - lTx.Block) > lExchangeminConf);
            foreach (var lTx in lConfirmedTxs)
            {
                MarketOrder lItem = aWaitingOrders.Find(lOrder => lOrder.CoinTxID == lTx.TxId);
                if (lItem == null)
                    continue;
                PandoraExchanger.ExchangeMarket lMarket = aListExchangeCoinMarket.Find(x => x.BaseTicker == aCurrencyItem.Ticker && lItem.Market == x.MarketName);
                if (lItem.Status == OrderStatus.Waiting && lMarket != null)
                    TryToPlaceOrder(lItem, lMarket, aCurrencyItem.Id);
            }
        }

        private void TryToPlaceOrder(MarketOrder aOrder, PandoraExchanger.ExchangeMarket aMarket, long aCurrencyID)
        {
            if (aOrder.ErrorCounter % 60 == 0)
            {
                try
                {
                    WriteTransactionLogEntry(aOrder, OrderMessage.OrderMessageLevel.Info, "Attempting to place order in exchange.");
                    if (FExchanger.PlaceOrder(aOrder, aMarket))
                        UpdateOrderStatus(aOrder, OrderStatus.Placed, aCurrencyID);
                    WriteTransactionLogEntry(aOrder);
                    aOrder.ErrorCounter = 0;
                }
                catch (Exception ex)
                {
                    int lNumberofretrys = aOrder.ErrorCounter / 60;

                    if (lNumberofretrys >= 9)
                    {
                        aOrder.Cancelled = true;
                        UpdateOrderStatus(aOrder, OrderStatus.Interrupted, aCurrencyID);
                        WriteTransactionLogEntry(aOrder);
                    }

                    aOrder.ErrorCounter += 1;
                    Universal.Log.Write(Universal.LogLevel.Error, string.Format("Order: {0}. Exception: {1}", aOrder.InternalID, ex.ToString()));

                    WriteTransactionLogEntry(aOrder, OrderMessage.OrderMessageLevel.Error, "Error while placing Order: " + ex.Message);
                    WriteTransactionLogEntry(aOrder, OrderMessage.OrderMessageLevel.Info, string.Format("Retrying in 1 minute. Attempt: {0}/10", (lNumberofretrys + 1)));
                }
            }
            else
            {
                aOrder.ErrorCounter++;
            }
        }

        private void UpdateOrderStatus(MarketOrder aOrder, OrderStatus aStatus, long aCurrencyID)
        {
            FExchanger.UpdateOrderStatus(aOrder, aStatus);
            var lViewModel = MainForm.GetOrderViewModel(aOrder.InternalID);
            if (lViewModel != null)
            {
                lViewModel.Status = aStatus.ToString();
                var lAsyncResult = MainForm.BeginInvoke(new MethodInvoker(() => MainForm.AddOrUpdateOrder(lViewModel, aCurrencyID)));
                MainForm.EndInvoke(lAsyncResult);
            }
        }

        private void WriteTransactionLogEntry(MarketOrder aOrder, OrderMessage.OrderMessageLevel aLevel = OrderMessage.OrderMessageLevel.Info, string aMessage = null)
        {
            if (string.IsNullOrEmpty(aMessage))
            {
                switch (aOrder.Status)
                {
                    case OrderStatus.Initial:
                        FDBExchanger.WriteOrderLog(aOrder.InternalID, "Starting transaction process.", OrderMessage.OrderMessageLevel.Info);
                        FDBExchanger.WriteOrderLog(aOrder.InternalID, "Waiting for market stop price to place order.", OrderMessage.OrderMessageLevel.StageChange);
                        break;

                    case OrderStatus.Waiting:
                        FDBExchanger.WriteOrderLog(aOrder.InternalID, "Price reached specified stop price.", OrderMessage.OrderMessageLevel.Info);
                        FDBExchanger.WriteOrderLog(aOrder.InternalID, string.Format("{0} coins sent to exchange account. Tx ID: {1}.", aOrder.SentQuantity, aOrder.CoinTxID), OrderMessage.OrderMessageLevel.Info);
                        FDBExchanger.WriteOrderLog(aOrder.InternalID, "Waiting for confirmations to place the order.", OrderMessage.OrderMessageLevel.StageChange);
                        break;

                    case OrderStatus.Placed:
                        FDBExchanger.WriteOrderLog(aOrder.InternalID, string.Format("Transaction id {0} has enough confirmations to place the order.", aOrder.CoinTxID), OrderMessage.OrderMessageLevel.Info);
                        FDBExchanger.WriteOrderLog(aOrder.InternalID, string.Format("Placed order in exchange. Uuid: {0}. Waiting for order to fulfill.", aOrder.ID.ToString()), OrderMessage.OrderMessageLevel.StageChange);
                        break;

                    case OrderStatus.Interrupted:
                        FDBExchanger.WriteOrderLog(aOrder.InternalID, "Transaction cancelled or not found on the exchange.", OrderMessage.OrderMessageLevel.FatalError);
                        break;

                    case OrderStatus.Completed:
                        FDBExchanger.WriteOrderLog(aOrder.InternalID, "Transaction completed. Waiting for withdraw.", OrderMessage.OrderMessageLevel.StageChange);
                        break;

                    case OrderStatus.Withdrawed:
                        FDBExchanger.WriteOrderLog(aOrder.InternalID, "Cryptocurrencies succesfully withdrawn to Pandora's Wallet.", OrderMessage.OrderMessageLevel.Finisher);
                        break;
                }
            }
            else FDBExchanger.WriteOrderLog(aOrder.InternalID, aMessage, aLevel);
            if (FDBExchanger.ReadOrderLogs(aOrder.InternalID, out List<OrderMessage> lOrderMessages))
                AddNewLogsToOrder(aOrder.InternalID, lOrderMessages);
        }

        private void AddNewLogsToOrder(int aOrderID, List<OrderMessage> aOrderMessages)
        {
            var lViewOrderMessages = aOrderMessages.Select(lMsg => new AppMainForm.ExchangeOrderLogViewModel { ID = lMsg.ID, Message = lMsg.Message, Time = lMsg.Time.ToLocalTime().ToString() });
            if (lViewOrderMessages.Any())
            {
                var lViewModel = MainForm.GetOrderViewModel(aOrderID);
                lViewModel.AddLog(lViewOrderMessages);
            }
        }

        public void Dispose()
        {
            MainForm.OnSelectedCurrencyChanged -= MainForm_ExchangeOnSelectedCurrencyChanged; ;
            MainForm.OnExchangeSelectedCurrencyChanged -= MainForm_OnExhangeMarketSelectionChanged;
            MainForm.OnLabelEstimatePriceClick -= MainForm_OnLabelEstimatePriceClick;
            MainForm.OnExchangeSelectionChanged -= MainForm_OnExchangeSelectionChanged;

            MainForm.OnTxtQuantityLeave -= MainForm_OnExchangeQuantityTxtChanged;
            MainForm.OnExchangeBtnClick -= MainForm_OnExchangeBtnClick;
            //MainForm.OnOrderHistorySelectionChanged -= MainForm_OnOrderHistorySelectionChanged;
            MainForm.OnCancelBtnClick -= MainForm_OnCancelBtnClick;

            MainForm.OnTxtTotalLeave -= MainForm_OnTotalReceivedChanged;
            MainForm.OnCheckAllOrderHistory -= MainForm_OnCheckAllOrderHistory;
            StopProcess();

        }
    }
}