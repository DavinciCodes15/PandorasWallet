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

namespace Pandora.Client.PandorasWallet
{
    public partial class PandoraClientControl
    {
        private PandoraExchanger FExchanger;
        private ConcurrentBag<PandoraExchanger.ExchangeMarket> FExchangeCoinMarket;
        private LoginExchanger FExchangeLogin;
        private ConcurrentDictionary<uint, List<MarketOrder>> FExchangeCurrencyOrders;
        private CancellationTokenSource FExchangeTaskCancellationSource;
        private Task[] FExchangeTasks;
        private PandoraExchanger.ExchangeMarket FExchangeSelectedCoin;
        private ExchangeTxDBHandler FDBExchanger;
        private string FLastExchangeSelectedCurrency = "";
        private bool FDisableUpdating;

        private object FExchangeInterfaceLock = new object();

        private decimal FCacheTotal = -1;
        private decimal FCacheQuantity = -1;

        private int FPreviosOrderHistory = -1;

        partial void StartupExchangeProcess()
        {
            if (FExchanger != null && FExchanger.IsCredentialsSet)
            {
                ExchangeDisconnect();
            }
            else
            {
                Task.Run(() =>
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
            }

            FExchangeTaskCancellationSource = new CancellationTokenSource();

            FExchangeTasks = new Task[3];

            FExchangeTasks[0] = Task.Run(() => OrderHistoryRefreshTask(), FExchangeTaskCancellationSource.Token);
            FExchangeTasks[1] = Task.Run(() => LocalOrderScanTask(), FExchangeTaskCancellationSource.Token);
            FExchangeTasks[2] = Task.Run(() => RemoteOrderScanTask(), FExchangeTaskCancellationSource.Token);

            SetOrderDBHandler(FWallet.DataFolder, FWallet.InstanceId);

            List<string> lCurrencies = new List<string>
            {
                "Bittrex"
            };

            MainForm.AddExchanges(lCurrencies);

            foreach (CurrencyItem it in FWallet.UserCoins)
            {
                if (!FExchangeCurrencyOrders.ContainsKey((uint)it.Id))
                {
                    FExchangeCurrencyOrders[(uint)it.Id] = new List<MarketOrder>();
                }
                FExchangeCurrencyOrders[(uint)it.Id].AddRange(FExchanger.LoadTransactions(it.Ticker));
            }
        }

        partial void ExchangeInitialize()
        {
            FExchangeLogin = new LoginExchanger();

            FExchanger = PandoraExchanger.GetInstance();
            FExchanger.OnMarketPricesChanging += FExchanger_OnMarketPricesChanging;

            FExchangeCoinMarket = new ConcurrentBag<PandoraExchanger.ExchangeMarket>();
            FExchangeCurrencyOrders = new ConcurrentDictionary<uint, List<MarketOrder>>();

            MainForm.OnExhangeCurrencySelectionChanged += MainForm_OnExhangeMarketSelectionChanged;
            MainForm.OnLabelEstimatePriceClick += MainForm_OnLabelEstimatePriceClick;
            MainForm.OnExchangeSelectionChanged += MainForm_OnExchangeSelectionChanged;

            MainForm.OnTxtQuantityLeave += MainForm_OnExchangeQuantityTxtChanged;
            MainForm.OnExchangeBtnClick += MainForm_OnExchangeBtnClick;
            MainForm.OnOrderHistorySelectionChanged += MainForm_OnOrderHistorySelectionChanged;

            MainForm.OnTxtTotalLeave += MainForm_OnTotalReceivedChanged;
            MainForm.OnCheckAllOrderHistory += MainForm_OnCheckAllOrderHistory;
        }

        partial void ExchangePandoraCurrencyChanged(uint aCurrency)
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
            List<CurrencyItem> lUserCoins = FWallet.UserCoins;
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
        private void PandoraCurrencyChangedProcess(uint aCurrency)
        {
            MainForm.ClearListExchangeTo();
            MainForm.StatusControlOrderHistory.ClearStatusList();

            if (string.IsNullOrEmpty(MainForm.SelectedExchange) || !FExchanger.IsCredentialsSet)
                return;

            Tuple<CurrencyItem, decimal>[] lCoinsWithMarket = GetCoinsWithExchangePrice(FWallet.ActiveCurrencyItem.Ticker);
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
            MainForm.ClearOrderHistory();
            List<MarketOrder> lOrders = new List<MarketOrder>();
            if (FExchangeCurrencyOrders.ContainsKey(aCurrency))
            {
                foreach (MarketOrder it in FExchangeCurrencyOrders[aCurrency])
                {
                    if (MainForm.AllOrderHistoryChecked)
                    {
                        DisplayAllOrderHistory();
                    }
                    else
                    {
                        DisplayActiveCurrencyOrder();
                    }
                }
                lOrders = FExchangeCurrencyOrders[aCurrency];
            }
            FPreviosOrderHistory = -1;
            FLastExchangeSelectedCurrency = string.Empty;
            RefreshStatusOrderLog(lOrders);
        }

        private void MainForm_OnExchangeBtnClick(object sender, EventArgs e)
        {
            if (MainForm.ExchangeTargetPrice <= 0 || MainForm.ExchangeStopPrice <= 0)
            {
                throw new ClientExceptions.InvalidOperationException("Invalid target or stop price amount.");
            }

            if (MainForm.ExchangeQuantity > FWallet.GetBalance(FWallet.ActiveCurrencyID).Confirmed)
            {
                throw new ClientExceptions.InvalidOperationException("Not enough confirmed balance to do transaction");
            }

            if (MainForm.ExchangeQuantity < (decimal)0.0005)
            {
                throw new ClientExceptions.InvalidOperationException("Sell Quantity should be greater than 0.0005");
            }

            decimal lAmountTrade = FExchangeSelectedCoin.IsSell ? MainForm.ExchangeQuantity : MainForm.ExchangeTotalReceived;

            if (FExchangeSelectedCoin.MinimumTrade >= lAmountTrade)
            {
                throw new ClientExceptions.InvalidOperationException(string.Format("Sell minimum trade size is {0} {1}", FExchangeSelectedCoin.MinimumTrade, FExchangeSelectedCoin.BaseTicker));
            }

            TryToCreateNewExchangeTransaction(MainForm.ExchangeTargetPrice, MainForm.ExchangeStopPrice, MainForm.ExchangeQuantity, FExchangeSelectedCoin);
            MainForm.StatusControlExchange.StatusName = MainForm.ExchangeTransactionName;
        }

        private void MainForm_OnExchangeQuantityTxtChanged(object sender, EventArgs e)
        {
            if (MainForm.ExchangeTargetPrice > 0 && MainForm.ExchangeQuantity > 0 && (MainForm.ExchangeQuantity != FCacheQuantity || MainForm.ExchangeTotalReceived != FCacheTotal))
            {
                List<CurrencyItem> lCoinToSell = FWallet.UserCoins;
                CurrencyItem lPresicion = lCoinToSell.Find(x => x.Ticker == FExchangeSelectedCoin.BaseTicker);
                decimal lTotal = Math.Round(MainForm.ExchangeQuantity / (FExchangeSelectedCoin.IsSell ? 1 / MainForm.ExchangeTargetPrice : MainForm.ExchangeTargetPrice), FWallet.Precision);
                decimal lComision = lTotal * (decimal)0.0025;
                MainForm.ExchangeTotalReceived = Math.Round(lTotal - lComision, lPresicion.Precision);
                FCacheTotal = MainForm.ExchangeTotalReceived;
                FCacheQuantity = MainForm.ExchangeQuantity;
            }
        }

        private void MainForm_OnCheckAllOrderHistory(object sender, EventArgs e)
        {
            MainForm.ClearOrderHistory();
            if (FExchangeCurrencyOrders.Any())
            {
                if (MainForm.AllOrderHistoryChecked)
                {
                    DisplayAllOrderHistory();
                }
                else
                {
                    DisplayActiveCurrencyOrder();
                }
            }
        }

        private void MainForm_OnTotalReceivedChanged(object sender, EventArgs e)
        {
            if (MainForm.ExchangeTotalReceived > 0 && FCacheTotal != MainForm.ExchangeTotalReceived && (MainForm.ExchangeQuantity != FCacheQuantity || MainForm.ExchangeTotalReceived != FCacheTotal))
            {
                List<CurrencyItem> lCoinToSell = FWallet.UserCoins;
                CurrencyItem lPresicion = lCoinToSell.Find(x => x.Ticker == FExchangeSelectedCoin.BaseTicker);
                decimal lTotal = Math.Round(MainForm.ExchangeTotalReceived * (FExchangeSelectedCoin.IsSell ? 1 / MainForm.ExchangeTargetPrice : MainForm.ExchangeTargetPrice), FWallet.Precision);
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

        private void MainForm_OnOrderHistorySelectionChanged(object sender, EventArgs e)
        {
            int? lCurrentSelected = (int?)MainForm.SelectedOrderHistory?.Tag;
            if (lCurrentSelected.HasValue && lCurrentSelected != FPreviosOrderHistory)
            {
                RefreshSelectedOrderLog(lCurrentSelected.Value);
                FPreviosOrderHistory = lCurrentSelected.Value;
            }
        }

        private void MainForm_OnExchangeSelectionChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MainForm.SelectedExchange))
            {
                MainForm.ClearListExchangeTo();
                return;
            }

            if (!FExchanger.IsCredentialsSet)
            {
                if (!EncryptionPasswordDialog.Execute())
                {
                    MainForm.SelectedExchange = "0";
                    return;
                }
                try
                {
                    if (FWallet.CheckIfHaveKeys(MainForm.SelectedExchange))
                    {
                        try
                        {
                            if (FWallet.TryGetExchangeKeyPair(MainForm.SelectedExchange, out string lKey, out string lSecret))
                            {
                                SetExchangeCredentials(lKey, lSecret);
                                return;
                            }

                            throw new ClientExceptions.WalletCorruptionException("Corrupt wallet file found. Try restoring wallet.");
                        }
                        catch (ClientExceptions.WalletCorruptionException ex)
                        {
                            if (!ExecuteRestoringProccess(ex.Message))
                            {
                                MainForm.SelectedExchange = "0";
                                return;
                            }
                        }
                        catch (Exception)
                        {
                            MainForm.StandardInfoMsgBox("Invalid credentials. Please add new key pair on next window.");
                        }
                    }

                    if (FExchangeLogin.Execute())
                    {
                        SetExchangeCredentials(FExchangeLogin.ExchageKey, FExchangeLogin.ExchangeSecret);

                        FWallet.SaveExchangeCredentials(MainForm.SelectedExchange, FExchangeLogin.ExchageKey, FExchangeLogin.ExchangeSecret);

                        return;
                    }
                    MainForm.SelectedExchange = "0";
                }
                catch
                {
                    MainForm.SelectedExchange = "0";
                    throw;
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
                {
                    return;
                }

                FCacheQuantity = FCacheTotal = -1;

                MainForm.ExchangeTargetPrice = FExchangeSelectedCoin.BasePrice;
                MainForm.ExchangeStopPrice = FExchangeSelectedCoin.BasePrice;
                MainForm.ExchangeTransactionName = string.Format("{0} -> {1} - {2}", FExchangeSelectedCoin.BaseTicker, MainForm.SelectedExchangeMarket.SubItems[1].Text, DateTime.Now.ToString());

                if (!FExchangeCurrencyOrders.ContainsKey(FWallet.ActiveCurrencyID))
                {
                    FExchangeCurrencyOrders[FWallet.ActiveCurrencyID] = new List<MarketOrder>();
                }
                List<MarketOrder> lOrders = FExchangeCurrencyOrders[FWallet.ActiveCurrencyID];

                if (MainForm.AllOrderHistoryChecked)
                {
                    MainForm.ClearOrderHistory();
                    DisplayAllOrderHistory();
                }
                else
                {
                    MainForm.ClearOrderHistory();
                    DisplayActiveCurrencyOrder();
                }
                FPreviosOrderHistory = -1;
                FLastExchangeSelectedCurrency = MainForm.SelectedExchangeMarket.SubItems[1].Text;

                RefreshStatusOrderLog(lOrders);

                EnableExchangeInterface();
                MainForm_OnExchangeQuantityTxtChanged(null, null);
            }
        }

        private void DisplayAllOrderHistory()
        {
            lock (FExchangeInterfaceLock)
            {
                foreach (KeyValuePair<uint, List<MarketOrder>> currencyId in FExchangeCurrencyOrders)
                {
                    List<MarketOrder> lOrders = FExchangeCurrencyOrders[currencyId.Key];

                    foreach (MarketOrder it in lOrders)
                    {
                        List<CurrencyItem> Coins = FWallet.UserCoins;
                        PandoraExchanger.ExchangeMarket[] lMarkets = FExchanger.GetMarketCoins(it.BaseTicker);

                        PandoraExchanger.ExchangeMarket lMarket = lMarkets.ToList().Find(x => x.BaseTicker == Coins.Find(y => y.Id == currencyId.Key).Ticker);

                        decimal lRate = lMarket.IsSell ? 1 / it.Rate : it.Rate;
                        decimal lRawAmount = it.SentQuantity / lRate;

                        decimal lQuantity = Math.Round(lRawAmount - FExchanger.GetTransactionsFee(lMarket.CoinTicker) - (lRawAmount * (decimal)0.0025), FWallet.Precision);
                        MainForm?.Invoke(new MethodInvoker(delegate () { MainForm.AddOrderHistory(it.InternalID, it.Name, it.SentQuantity.ToString(), lQuantity.ToString(), it.Rate.ToString(), it.StopPrice.ToString(), it.Market.ToString(), it.OpenTime.ToLocalTime().ToString(), it.Status.ToString()); }));
                    }
                }
            }
        }

        public void OrderHistoryRefreshTask()
        {
            try
            {
                if (FWallet != null && FExchanger.IsCredentialsSet)
                {
                    if (FExchangeCurrencyOrders.ContainsKey(FWallet.ActiveCurrencyID))
                    {
                        List<MarketOrder> lOrders = new List<MarketOrder>(FExchangeCurrencyOrders[FWallet.ActiveCurrencyID]);
                        MainForm?.Invoke(new MethodInvoker(delegate ()
                        {
                            MainForm.ClearOrderHistory();
                            if (MainForm.AllOrderHistoryChecked)
                            {
                                DisplayAllOrderHistory();
                            }
                            else
                            {
                                DisplayActiveCurrencyOrder();
                            }
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write("Error on order history refresh. Details: {0}", ex);
            }
            finally
            {
                Task.Delay(15000).Wait(FExchangeTaskCancellationSource.Token);

                FExchangeTasks[0] = Task.Run(() => OrderHistoryRefreshTask(), FExchangeTaskCancellationSource.Token);
            }
        }

        private void FExchanger_OnMarketPricesChanging()
        {
            if (FWallet != null && FExchanger.IsCredentialsSet)
            {
                foreach (PandoraExchanger.ExchangeMarket it in FExchangeCoinMarket)
                {
                    if (FDisableUpdating)
                    {
                        return;
                    }

                    CurrencyItem lCurrency = FWallet.UserCoins.Find(X => X.Ticker == it.CoinTicker);
                    if (FExchangeSelectedCoin != null)
                    {
                        MainForm?.Invoke(new MethodInvoker(delegate () { MainForm.AddCoinExchangeTo(lCurrency.Id, lCurrency.Name, lCurrency.Ticker, it.BasePrice); }));
                    }
                }

                MainForm?.Invoke(new MethodInvoker(delegate () { MainForm.LabelEstimatePrice = FExchangeSelectedCoin == null ? "" : FExchangeSelectedCoin.BasePrice.ToString(); }));
            }
        }

        private void DisplayActiveCurrencyOrder()
        {
            lock (FExchangeInterfaceLock)
            {
                List<MarketOrder> lOrders = FExchangeCurrencyOrders[FWallet.ActiveCurrencyID];
                foreach (MarketOrder it in lOrders)
                {
                    PandoraExchanger.ExchangeMarket lMarket = FExchangeCoinMarket.ToList().Find(x => x.MarketName == it.Market && x.BaseTicker == it.BaseTicker);
                    if (lMarket == null)
                    {
                        throw new Exception("Invalid transaction found");
                    }

                    decimal lRate = lMarket.IsSell ? 1 / it.Rate : it.Rate;
                    decimal lRawAmount = it.SentQuantity / lRate;

                    decimal lQuantity = Math.Round(lRawAmount - FExchanger.GetTransactionsFee(lMarket.CoinTicker) - (lRawAmount * (decimal)0.0025), FWallet.Precision);
                    MainForm?.Invoke(new MethodInvoker(delegate () { MainForm.AddOrderHistory(it.InternalID, it.Name, it.SentQuantity.ToString(), lQuantity.ToString(), it.Rate.ToString(), it.StopPrice.ToString(), it.Market.ToString(), it.OpenTime.ToLocalTime().ToString(), it.Status.ToString()); }));
                }
            }
        }

        private void SetOrderDBHandler(string aDataFolder, string aInstanceID)
        {
            ExchangeTxDBHandler lOldDBExchanger = null;
            if (FDBExchanger != null)
            {
                lOldDBExchanger = FDBExchanger;
            }

            FDBExchanger = new ExchangeTxDBHandler(aDataFolder, aInstanceID);
            FExchanger.TransactionHandler = FDBExchanger;

            lOldDBExchanger?.Dispose();
        }

        public void ExchangeDisconnect()
        {
            FDisableUpdating = true;
            try
            {
                FExchangeTaskCancellationSource.Cancel();

                Task.WaitAll(FExchangeTasks);
            }
            catch (OperationCanceledException e)
            {
                Universal.Log.Write(Universal.LogLevel.Info, e.Message);
            }
            catch (AggregateException e)
            {
                Universal.Log.Write(Universal.LogLevel.Info, e.Message);
            }
            finally
            {
                FExchanger.Clear();
                MainForm.ClearListExchangeTo();
                DisableExchangeInterface();
                FExchangeCoinMarket = new ConcurrentBag<PandoraExchanger.ExchangeMarket>();
                FExchangeCurrencyOrders.Clear();
                FDBExchanger = null;
                FLastExchangeSelectedCurrency = string.Empty;
                FPreviosOrderHistory = -1;
                MainForm.LabelEstimatePrice = "";
                MainForm.SelectedExchange = "-1";
            }
        }

        private void RefreshIfSelectedOrderlog(int aId)
        {
            int? lCurrentSelected = (int?)MainForm.SelectedOrderHistory?.Tag;
            if (lCurrentSelected.HasValue && aId == lCurrentSelected.Value)
            {
                RefreshSelectedOrderLog(aId);
            }
        }

        private void RefreshSelectedOrderLog(int aSelectedInternalID)
        {
            FDBExchanger.ReadOrderLogs(aSelectedInternalID, out List<OrderMessage> lMessages);

            MainForm?.Invoke(new MethodInvoker(delegate () { MainForm.StatusControlOrderHistory.ClearStatusList(); }));

            if (lMessages == null || !lMessages.Any())
            {
                return;
            }

            foreach (OrderMessage it in lMessages)
            {
                MainForm?.Invoke(new MethodInvoker(delegate ()
                {
                    MainForm.StatusControlOrderHistory.AddStatus(it.Time, it.Message);
                }));
            }
        }

        public void RefreshStatusOrderLog(List<MarketOrder> aOrders, int aInternalID = -1)
        {
            MainForm?.Invoke(new MethodInvoker(delegate () { MainForm.StatusControlExchange.ClearStatusList(); }));

            if (FExchangeSelectedCoin == null || !aOrders.Any())
            {
                return;
            }

            IOrderedEnumerable<MarketOrder> lTx = aOrders.Where(x => x.BaseTicker == FExchangeSelectedCoin.BaseTicker && x.Market == FExchangeSelectedCoin.MarketName).OrderBy(x => x.InternalID);

            if (lTx.Any())
            {
                int lTxID = lTx.Last().InternalID;

                if (aInternalID != -1)
                {
                    if (aInternalID != lTxID)
                    {
                        return;
                    }
                }

                FDBExchanger.ReadOrderLogs(lTxID, out List<OrderMessage> lMessages);

                foreach (OrderMessage it in lMessages)
                {
                    MainForm?.Invoke(new MethodInvoker(delegate ()
                    {
                        MainForm.StatusControlExchange.AddStatus(it.Time, it.Message);
                    }));
                }
            }
        }

        private void RemoteOrderScanTask()
        {
            try
            {
                if (FWallet != null && FExchanger.IsCredentialsSet)
                {
                    foreach (List<MarketOrder> OrderList in FExchangeCurrencyOrders.Values)
                    {
                        foreach (MarketOrder lOrder in OrderList.Where(x => x.Status == OrderStatus.Placed))
                        {
                            MarketOrder lRemoteOrder = FExchanger.GetOrder(lOrder.ID);

                            if (!lRemoteOrder.Completed && !lRemoteOrder.Cancelled)
                            {
                                continue;
                            }

                            lOrder.Completed = lRemoteOrder.Completed;
                            lOrder.Cancelled = lRemoteOrder.Cancelled;

                            OrderStatus lStatus = OrderStatus.Placed;

                            if (lOrder.Completed)
                            {
                                lStatus = OrderStatus.Completed;
                            }

                            if (lOrder.Cancelled)
                            {
                                lStatus = OrderStatus.Interrupted;
                            }

                            FExchanger.UpdateOrderStatus(lOrder, lStatus);
                            WriteTransactionLogEntry(lOrder);
                        }

                        foreach (MarketOrder lOrder in OrderList.Where(x => x.Status == OrderStatus.Completed))
                        {
                            TryToWithdrawOrder(lOrder);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write("Error on Checking order statuses. Details: " + ex.Message + " on " + ex.Source);
            }
            finally
            {
                Task.Delay(5000).Wait(FExchangeTaskCancellationSource.Token);

                FExchangeTasks[2] = Task.Run(() => RemoteOrderScanTask(), FExchangeTaskCancellationSource.Token);
            }
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

            long lCurrencyID = FWallet.UserCoins.Where(x => x.Ticker == lMarket.CoinTicker).Select(x => x.Id).First();

            if (aOrder.ErrorCounter % 60 == 0)
            {
                try
                {
                    if (FExchanger.WithdrawOrder(lMarket, aOrder, FWallet.GetCoinAddress((uint)lCurrencyID), FExchanger.GetTransactionsFee(aOrder.BaseTicker)))
                        FExchanger.UpdateOrderStatus(aOrder, OrderStatus.Withdrawed);
                    WriteTransactionLogEntry(aOrder);
                    aOrder.ErrorCounter = 0;
                }
                catch (Exception ex)
                {
                    int lNumberofretrys = aOrder.ErrorCounter / 60;

                    if (lNumberofretrys >= 9)
                    {
                        aOrder.Cancelled = true;
                        FExchanger.UpdateOrderStatus(aOrder, OrderStatus.Interrupted);
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
            MainForm.ExchangeTargetPriceEnabled = true;
            MainForm.ExchangeQuantityEnabled = true;
            MainForm.ExchangeTotalReceivedEnabled = true;
            MainForm.ExchangeTransactionNameEnabled = true;
            MainForm.CheckAllOrderHistoryEnabled = true;
            MainForm.ExchangeButtonEnabled = true;
            MainForm.ExchangeStoptPriceEnabled = true;
        }

        private void SetExchangeCredentials(string aKey, string aSecret)
        {
            FExchanger.SetCredentials(aKey, aSecret);
            EnableExchangeInterface();
            ExchangePandoraCurrencyChanged(FWallet.ActiveCurrencyID);
        }

        /// <summary>
        /// Tries to form a transaction that meets the order requeriments and then sends it to the exchange user address. Also does amount verification
        /// </summary>
        /// <param name="aOrder">Order to fulfill</param>
        /// <param name="aMarket">Order's market</param>
        /// <param name="aCurrencyID">Selected currency id</param>
        private void TryToTransferMoneyToExchange(MarketOrder aOrder, PandoraExchanger.ExchangeMarket aMarket, uint aCurrencyID)
        {
            try
            {
                WriteTransactionLogEntry(aOrder, OrderMessage.OrderMessageLevel.Info, string.Format("Stop price reached, trying to send coins to exchange.", FExchanger.GetConfirmations(aMarket.BaseTicker)));
                string lExchangeAddress = FExchanger.GetDepositAddress(aMarket);
                decimal lAmount = aMarket.IsSell ? aOrder.SentQuantity : aOrder.SentQuantity + (aOrder.SentQuantity * (decimal)0.025);
                ulong lTxFee = 0;
                int lCounter = 0;
                do
                {
                    lCounter++;
                    try
                    {
                        lTxFee = FWallet.CalculateTxFee(lExchangeAddress, lAmount, aCurrencyID);
                        break;
                    }
                    catch
                    {
                        continue;
                    }
                } while (lCounter < 6);
                if (lTxFee == 0) throw new Exception("Failed to get transaction txfee");
                BalanceViewModel lBalanceModel = FWallet.GetBalance(aCurrencyID);
                decimal lBalance = Convert.ToDecimal(lBalanceModel.ToString());
                decimal lDecimalTxFee = (lTxFee / (decimal)FWallet.Coin);
                if (lBalance == 0 || (lAmount + lDecimalTxFee) > lBalance) throw new Exception("Not enough balance to transfer to the exchange");
                string lTxID = ExecuteSendTx(lAmount, aCurrencyID, lTxFee, lExchangeAddress);
                if (string.IsNullOrEmpty(lTxID)) throw new Exception("Unable to broadcast transaction");
                aOrder.CoinTxID = lTxID;
                FExchanger.UpdateOrderStatus(aOrder, OrderStatus.Waiting);
                WriteTransactionLogEntry(aOrder);
                WriteTransactionLogEntry(aOrder, OrderMessage.OrderMessageLevel.Info, string.Format("Number of confirmations needed: {0} confirmations", FExchanger.GetConfirmations(aMarket.BaseTicker)));
            }
            catch (Exception ex)
            {
                FExchanger.UpdateOrderStatus(aOrder, OrderStatus.Interrupted);
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
        private string ExecuteSendTx(decimal aAmount, uint aActiveCurrencyID, ulong aTxFee, string aExchangeAddress)
        {
            FWallet.InitializeRootSeed();
            string lTx = FWallet.PrepareNewTransaction(aExchangeAddress, aAmount, aActiveCurrencyID, aTxFee);
            long lTxHandle = FWallet.SendNewTransaction(lTx, aActiveCurrencyID);
            string lReturnedTxID = null;

            try
            {
                string lTxID;
                while (!FWallet.CheckTransactionHandle(lTxHandle, out lTxID)) //Waits endlessly until server response
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
            if (!EncryptionPasswordDialog.Execute())
                return;

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
            if (!FExchangeCurrencyOrders.ContainsKey(FWallet.ActiveCurrencyID))
                FExchangeCurrencyOrders[FWallet.ActiveCurrencyID] = new List<MarketOrder>();
            FExchangeCurrencyOrders[FWallet.ActiveCurrencyID].Add(lOrderWithID);
            WriteTransactionLogEntry(lOrderWithID);
        }

        /// <summary>
        /// Performs a scan of all orders and executes actions as needed
        /// </summary>
        private void LocalOrderScanTask()
        {
            try
            {
                if (FWallet != null && FExchanger.IsCredentialsSet)
                {
                    Dictionary<uint, List<TransactionViewModel>> lTxs = new Dictionary<uint, List<TransactionViewModel>>(FWallet.TransactionsByCurrency);
                    List<CurrencyItem> lCoins = FWallet.UserCoins;
                    IEnumerable<CurrencyItem> lCoinWithTx = lCoins.Where(lCoin => lTxs.ContainsKey((uint)lCoin.Id) && lTxs[(uint)lCoin.Id].Any());

                    foreach (CurrencyItem lCoin in lCoinWithTx)
                    {
                        uint lCoinId = (uint)lCoin.Id;
                        string lTicker = lCoin.Ticker;

                        List<PandoraExchanger.ExchangeMarket> lExchangeCoinMarkets;
                        try
                        {
                            lExchangeCoinMarkets = FExchanger.GetMarketCoins(lTicker).ToList();
                        }
                        catch (Exception ex)
                        {
                            Universal.Log.Write(Universal.LogLevel.Error, string.Format("{0}: Exception in getmarketcoins. {1}", lCoin.Name, ex));
                            continue;
                        }

                        if (FExchangeCurrencyOrders.TryGetValue(lCoinId, out List<MarketOrder> lOrders))
                        {
                            List<MarketOrder> lOrdersInitial = lOrders.Where(lOrder => lOrder.Status == OrderStatus.Initial).ToList();
                            VerifyInitialOrders(lOrdersInitial, lExchangeCoinMarkets, lTicker, lCoinId);
                            List<MarketOrder> lOrderWaiting = lOrders.Where(lOrder => lOrder.Status == OrderStatus.Waiting).ToList();
                            VerifyWaitingOrders(lOrderWaiting, lTicker, lTxs[lCoinId], lExchangeCoinMarkets);
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
                Task.Delay(1000).Wait(FExchangeTaskCancellationSource.Token);

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
        private void VerifyInitialOrders(List<MarketOrder> aInitialOrders, List<PandoraExchanger.ExchangeMarket> aListExchangeCoinMarket, string aTicker, uint aCurrencyID)
        {
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
                {
                    TryToTransferMoneyToExchange(lOrder, lMarket, aCurrencyID);
                }
            }
        }

        /// <summary>
        /// Checks order that are in waiting state, and changes it to placed state if is needed (Here transaction confirmations are verified))
        /// </summary>
        /// <param name="aWaitingOrders">Orders with waiting state</param>
        /// <param name="aCoinTicker">Order currency ticker</param>
        /// <param name="aCurrencyTransactions">Pandora's currency transactions</param>
        /// <param name="aListExchangeCoinMarket">Markets to look for</param>
        private void VerifyWaitingOrders(List<MarketOrder> aWaitingOrders, string aCoinTicker, IEnumerable<TransactionViewModel> aCurrencyTransactions, List<PandoraExchanger.ExchangeMarket> aListExchangeCoinMarket)
        {
            if (!aWaitingOrders.Any())
                return;

            int lExchangeminConf = FExchanger.GetConfirmations(aCoinTicker);

            if (lExchangeminConf < 0)
                return;

            IEnumerable<TransactionViewModel> lConfirmedTxs = aCurrencyTransactions.Where(x => x.Confirmation >= (ulong)lExchangeminConf);

            foreach (TransactionViewModel lTx in lConfirmedTxs)
            {
                MarketOrder lItem = aWaitingOrders.Find(x => x.CoinTxID == lTx.TransactionID);

                if (lItem == null)
                    continue;

                PandoraExchanger.ExchangeMarket lMarket = aListExchangeCoinMarket.Find(x => x.BaseTicker == aCoinTicker && lItem.Market == x.MarketName);

                if (lItem.Status == OrderStatus.Waiting && lMarket != null)
                {
                    TryToPlaceOrder(lItem, lMarket);
                }
            }
        }

        private void TryToPlaceOrder(MarketOrder aOrder, PandoraExchanger.ExchangeMarket aMarket)
        {
            if (aOrder.ErrorCounter % 60 == 0)
            {
                try
                {
                    WriteTransactionLogEntry(aOrder, OrderMessage.OrderMessageLevel.Info, "Attempting to place order in exchange.");
                    if (FExchanger.PlaceOrder(aOrder, aMarket))
                        FExchanger.UpdateOrderStatus(aOrder, OrderStatus.Placed);
                    WriteTransactionLogEntry(aOrder);
                    aOrder.ErrorCounter = 0;
                }
                catch (Exception ex)
                {
                    int lNumberofretrys = aOrder.ErrorCounter / 60;

                    if (lNumberofretrys >= 9)
                    {
                        aOrder.Cancelled = true;
                        FExchanger.UpdateOrderStatus(aOrder, OrderStatus.Interrupted);
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

        private void WriteTransactionLogEntry(MarketOrder aOrder, OrderMessage.OrderMessageLevel aLevel = OrderMessage.OrderMessageLevel.Info, string aMessage = null)
        {
            if (!string.IsNullOrEmpty(aMessage))
            {
                FDBExchanger.WriteOrderLog(aOrder.InternalID, aMessage, aLevel);
                return;
            }

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
            List<MarketOrder> lOrders = FExchangeCurrencyOrders[FWallet.ActiveCurrencyID];

            RefreshStatusOrderLog(lOrders, aOrder.InternalID);

            RefreshIfSelectedOrderlog(aOrder.InternalID);
        }
    }
}