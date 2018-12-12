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
        private Dictionary<uint, List<MarketOrder>> FExchangeCurrencyOrders;
        private CancellationTokenSource FExchangeTaskCancellationSource;
        private Task[] FExchangeTasks;
        private PandoraExchanger.ExchangeMarket FExchangeSelectedCoin;
        private ExchangeTxDBHandler FDBExchanger;
        private string FLastExchangeSelectedCurrency = "";
        private bool FDisableUpdating;

        private object FExchangeInterfaceLock = new object();

        partial void ExchangeInitialize()
        {
            FExchangeLogin = new LoginExchanger();

            FExchanger = PandoraExchanger.GetInstance();
            FExchanger.OnMarketPricesChanging += FExchanger_OnMarketPricesChanging;

            FExchangeCoinMarket = new ConcurrentBag<PandoraExchanger.ExchangeMarket>();
            FExchangeCurrencyOrders = new Dictionary<uint, List<MarketOrder>>();

            MainForm.OnExhangeCurrencySelectionChanged += MainForm_OnExhangeMarketSelectionChanged;
            MainForm.OnLabelEstimatePriceClick += MainForm_OnLabelEstimatePriceClick;
            MainForm.OnExchangeSelectionChanged += MainForm_OnExchangeSelectionChanged;
            MainForm.OnTxtQuantityLeave += MainForm_OnExchangeQuantityTxtChanged;
            MainForm.OnExchangeBtnClick += MainForm_OnExchangeBtnClick;
            MainForm.OnOrderHistorySelectionChanged += MainForm_OnOrderHistorySelectionChanged;
            MainForm.OnTxtTotalLeave += MainForm_OnTotalReceivedChanged;
            MainForm.OnCheckAllOrderHistory += MainForm_OnCheckAllOrderHistory;
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
                        MainForm.AddOrderHistory(it.InternalID, it.Name, it.SentQuantity.ToString(), lQuantity.ToString(), it.Rate.ToString(), it.Market.ToString(), it.OpenTime.ToLocalTime().ToString(), it.Status.ToString());
                    }
                }
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
                    MainForm.AddOrderHistory(it.InternalID, it.Name, it.SentQuantity.ToString(), lQuantity.ToString(), it.Rate.ToString(), it.Market.ToString(), it.OpenTime.ToLocalTime().ToString(), it.Status.ToString());
                }
            }
        }

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
                        Utils.PandoraLog.GetPandoraLog().Write("Error starting market price updating. Details: " + ex.Message + " on " + ex.Source);
                    }
                });
            }

            FExchangeTaskCancellationSource = new CancellationTokenSource();

            FExchangeTasks = new Task[3];

            FExchangeTasks[0] = Task.Run(() => OrderHistoryRefresh(), FExchangeTaskCancellationSource.Token);
            FExchangeTasks[1] = Task.Run(() => UpdateExchangeOrderTxRelationship(), FExchangeTaskCancellationSource.Token);
            FExchangeTasks[2] = Task.Run(() => CheckOrderStatuses(), FExchangeTaskCancellationSource.Token);

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

        private int FPreviosOrderHistory = -1;

        private void MainForm_OnOrderHistorySelectionChanged(object sender, EventArgs e)
        {
            int? lCurrenSelected = (int?)MainForm.SelectedOrderHistory?.Tag;
            if (lCurrenSelected.HasValue && lCurrenSelected != FPreviosOrderHistory)
            {
                RefreshSelectedOrderLog();
                FPreviosOrderHistory = lCurrenSelected.Value;
            }
        }

        private void RefreshSelectedOrderLog(int aId = -1)
        {
            if (MainForm.SelectedOrderHistory == null)
            {
                return;
            }

            int lInternalID = (int)MainForm.SelectedOrderHistory.Tag;

            if (aId != -1)
            {
                if (lInternalID != aId)
                {
                    return;
                }
            }

            FDBExchanger.ReadOrderLogs(lInternalID, out List<OrderMessage> lMessages);

            MainForm.StatusControlOrderHistory.ClearStatusList();

            if (lMessages == null || !lMessages.Any())
            {
                return;
            }

            foreach (OrderMessage it in lMessages)
            {
                MainForm.StatusControlOrderHistory.AddStatus(it.Time, it.Message);
            }
        }

        private void MainForm_OnLabelEstimatePriceClick(object sender, EventArgs e)
        {
            MainForm.ExchangeTargetPrice = Convert.ToDecimal(MainForm.LabelEstimatePrice);
        }

        private void MainForm_OnExhangeMarketSelectionChanged(object sender, EventArgs e)
        {
            if (MainForm.SelectedExchangeMarket != null)
            {
                FExchangeSelectedCoin = FExchangeCoinMarket.ToList().Find(x => MainForm.SelectedExchangeMarket.SubItems[1].Text == x.CoinTicker);
                MainForm.LabelEstimatePrice = FExchangeSelectedCoin.BasePrice.ToString();
                MainForm.LabelPriceInCoin = "BTC";
                MainForm.LabelTotalCoinReceived = string.Format("{0} ({1})", MainForm.SelectedExchangeMarket.SubItems[0].Text, MainForm.SelectedExchangeMarket.SubItems[1].Text);

                if (!string.IsNullOrEmpty(FLastExchangeSelectedCurrency) && FLastExchangeSelectedCurrency == MainForm.SelectedExchangeMarket.SubItems[1].Text)
                {
                    return;
                }

                MainForm.ExchangeTargetPrice = FExchangeSelectedCoin.BasePrice;
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
                FLastExchangeSelectedCurrency = MainForm.SelectedExchangeMarket.SubItems[1].Text;

                RefreshStatusOrderLog(lOrders);

                EnableExchangeInterface();
            }
        }

        public void OrderHistoryRefresh()
        {
            try
            {
                if (FWallet != null && FExchanger.IsCredentialsSet)
                {
                    if (FExchangeCurrencyOrders.ContainsKey(FWallet.ActiveCurrencyID))
                    {
                        List<MarketOrder> lOrders = FExchangeCurrencyOrders[FWallet.ActiveCurrencyID];

                        MainForm?.BeginInvoke(new MethodInvoker(delegate ()
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
                Utils.PandoraLog.GetPandoraLog().Write("Error on order history refresh. Details: " + ex.Message + " on " + ex.Source);
            }
            finally
            {
                Task.Delay(15000).Wait(FExchangeTaskCancellationSource.Token);

                FExchangeTasks[0] = Task.Run(() => OrderHistoryRefresh(), FExchangeTaskCancellationSource.Token);
            }
        }

        public void RefreshStatusOrderLog(List<MarketOrder> aOrders, int aInternalID = -1)
        {
            MainForm?.BeginInvoke(new MethodInvoker(delegate () { MainForm.StatusControlExchange.ClearStatusList(); }));

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
                    MainForm?.BeginInvoke(new MethodInvoker(delegate () { MainForm.StatusControlExchange.AddStatus(it.Time, it.Message); }));
                }
            }
        }

        private void CheckOrderStatuses()
        {
            try
            {
                if (FWallet != null && FExchanger.IsCredentialsSet)
                {
                    foreach (List<MarketOrder> OrderList in FExchangeCurrencyOrders.Values)
                    {
                        foreach (MarketOrder itOrder in OrderList.Where(x => x.Status == OrderStatus.Placed))
                        {
                            MarketOrder lOrder = FExchanger.GetOrder(itOrder.ID);

                            if (!lOrder.Completed && !lOrder.Cancelled)
                            {
                                continue;
                            }

                            itOrder.Completed = lOrder.Completed;
                            itOrder.Cancelled = lOrder.Cancelled;

                            OrderStatus lStatus = OrderStatus.Placed;

                            if (itOrder.Completed)
                            {
                                lStatus = OrderStatus.Completed;
                            }

                            if (itOrder.Cancelled)
                            {
                                lStatus = OrderStatus.Interrupted;
                            }

                            FExchanger.UpdateOrder(itOrder, lStatus);
                            WriteTransactionLogEntry(itOrder);
                        }

                        foreach (MarketOrder it2 in OrderList.Where(x => x.Status == OrderStatus.Completed))
                        {
                            TryToWithdrawOrder(it2);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.PandoraLog.GetPandoraLog().Write("Error on Checking order statuses. Details: " + ex.Message + " on " + ex.Source);
            }
            finally
            {
                Task.Delay(5000).Wait(FExchangeTaskCancellationSource.Token);

                FExchangeTasks[2] = Task.Run(() => CheckOrderStatuses(), FExchangeTaskCancellationSource.Token);
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
                    FExchanger.WithdrawOrder(lMarket, aOrder, FWallet.GetCoinAddress((uint)lCurrencyID), FExchanger.GetTransactionsFee(aOrder.BaseTicker));
                    WriteTransactionLogEntry(aOrder);
                    aOrder.ErrorCounter = 0;
                }
                catch (Exception ex)
                {
                    aOrder.ErrorCounter += 60;

                    WriteTransactionLogEntry(aOrder, OrderMessage.OrderMessageLevel.Error, "Error on Withdraw Order: " + ex.Message);
                    WriteTransactionLogEntry(aOrder, OrderMessage.OrderMessageLevel.Info, "Retrying in 1 minute. Attempt: " + aOrder.ErrorCounter / 60 + "/10");
                }
            }
            else
            {
                aOrder.ErrorCounter++;
            }
        }

        private void MainForm_OnExchangeBtnClick(object sender, EventArgs e)
        {
            if (MainForm.ExchangeTargetPrice <= 0)
            {
                throw new Exception("Invalid target price amount.");
            }

            if (MainForm.ExchangeQuantity > FWallet.GetBalance(FWallet.ActiveCurrencyID).Confirmed)
            {
                throw new Exception("Not enough confirmed balance to do transaction");
            }

            if (MainForm.ExchangeQuantity < (decimal)0.0005)
            {
                throw new Exception("Sell Quantity should be greater than 0.0005");
            }

            TryToCreateNewExchangeTransaction(MainForm.ExchangeTargetPrice, MainForm.ExchangeQuantity, FExchangeSelectedCoin);
            MainForm.StatusControlExchange.StatusName = MainForm.ExchangeTransactionName;
        }

        private void MainForm_OnExchangeQuantityTxtChanged(object sender, EventArgs e)
        {
            if (MainForm.ExchangeTargetPrice > 0 && MainForm.ExchangeQuantity > 0)
            {
                List<CurrencyItem> lCoinToSell = FWallet.UserCoins;
                CurrencyItem lPresicion = lCoinToSell.Find(x => x.Ticker == MainForm.LabelCoinQuantity.Split(' ')[1].Replace("(", "").Replace(")", ""));
                decimal lTotal = Math.Round(MainForm.ExchangeQuantity / (FExchangeSelectedCoin.IsSell ? 1 / MainForm.ExchangeTargetPrice : MainForm.ExchangeTargetPrice), FWallet.Precision);
                decimal lComision = lTotal * (decimal)0.0025;
                MainForm.ExchangeTotalReceived = Math.Round(lTotal - lComision, lPresicion.Precision);
            }
        }

        private void MainForm_OnTotalReceivedChanged(object sender, EventArgs e)
        {
            if (MainForm.ExchangeTotalReceived > 0)
            {
                List<CurrencyItem> lCoinToSell = FWallet.UserCoins;
                CurrencyItem lPresicion = lCoinToSell.Find(x => x.Ticker == MainForm.LabelTotalCoinReceived.Split(' ')[1].Replace("(", "").Replace(")", ""));
                decimal lTotal = Math.Round(MainForm.ExchangeTotalReceived * (FExchangeSelectedCoin.IsSell ? 1 / MainForm.ExchangeTargetPrice : MainForm.ExchangeTargetPrice), FWallet.Precision);
                decimal lComision = lTotal * (decimal)0.0025;
                MainForm.ExchangeQuantity = Math.Round(lTotal - lComision, lPresicion.Precision);
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
                        MainForm?.BeginInvoke(new MethodInvoker(delegate () { MainForm.AddCoinExchangeTo(lCurrency.Id, lCurrency.Name, lCurrency.Ticker, it.BasePrice); }));
                    }
                }

                MainForm?.BeginInvoke(new MethodInvoker(delegate () { MainForm.LabelEstimatePrice = FExchangeSelectedCoin == null ? "" : FExchangeSelectedCoin.BasePrice.ToString(); }));
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
        }

        private void EnableExchangeInterface()
        {
            MainForm.ExchangeTargetPriceEnabled = true;
            MainForm.ExchangeQuantityEnabled = true;
            MainForm.ExchangeTotalReceivedEnabled = true;
            MainForm.ExchangeTransactionNameEnabled = true;
            MainForm.CheckAllOrderHistoryEnabled = true;
            MainForm.ExchangeButtonEnabled = true;
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

        private void SetExchangeCredentials(string aKey, string aSecret)
        {
            FExchanger.SetCredentials(aKey, aSecret);
            EnableExchangeInterface();
            UpdateExchange(FWallet.ActiveCurrencyID);
        }

        partial void UpdateExchange(uint aCurrency)
        {
            FDisableUpdating = true;
            try
            {
                UpdateProcess(aCurrency);
            }
            finally
            {
                FDisableUpdating = false;
            }
        }

        private void UpdateProcess(uint aCurrency)
        {
            MainForm.ClearListExchangeTo();

            MainForm.StatusControlOrderHistory.ClearStatusList();

            if (string.IsNullOrEmpty(MainForm.SelectedExchange) || !FExchanger.IsCredentialsSet)
            {
                return;
            }

            List<CurrencyItem> lUserCoins = FWallet.UserCoins;

            PandoraExchanger.ExchangeMarket[] lMarkets = FExchanger.GetMarketCoins(FWallet.ActiveCurrencyItem.Ticker);

            ConcurrentBag<PandoraExchanger.ExchangeMarket> lConcurrentBag = new ConcurrentBag<PandoraExchanger.ExchangeMarket>();

            foreach (PandoraExchanger.ExchangeMarket it in lMarkets.Where(x => lUserCoins.Exists(y => y.Ticker == x.CoinTicker)))
            {
                lConcurrentBag.Add(it);
            }

            FExchangeCoinMarket = lConcurrentBag;

            foreach (PandoraExchanger.ExchangeMarket it in FExchangeCoinMarket)
            {
                CurrencyItem lCurrency = lUserCoins.Find(X => X.Ticker == it.CoinTicker);
                MainForm.AddCoinExchangeTo(lCurrency.Id, lCurrency.Name, lCurrency.Ticker, it.BasePrice);
            }

            MainForm.ExchangeSelectCurrency(FExchangeCoinMarket.FirstOrDefault()?.CoinTicker);

            if (FExchangeCoinMarket.Any())
            {
                if (FExchangeSelectedCoin == null || FExchangeSelectedCoin.CoinTicker == MainForm.SelectedExchangeMarket.SubItems[1].Text)
                {
                    return;
                }
            }
            else
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
                    /*
                    var lMarket = FExchangeCoinMarket.ToList().Find(x => x.MarketName == it.Market && x.BaseTicker == it.BaseTicker);
                    if (lMarket == null)
                        throw new Exception("Invalid transaction found");
                    decimal lRate = lMarket.IsSell ? 1 / it.Rate : it.Rate;
                    MainForm.AddOrderHistory(it.InternalID, it.Name, it.SentQuantity.ToString(), (it.SentQuantity * lRate).ToString(), lRate.ToString(), lMarket.MarketName, it.OpenTime.ToLocalTime().ToString(), it.Status.ToString());
                    */
                }
                lOrders = FExchangeCurrencyOrders[aCurrency];
            }

            RefreshStatusOrderLog(lOrders);
        }

        private void TryToCreateNewExchangeTransaction(decimal aPriceTarget, decimal aAmount, PandoraExchanger.ExchangeMarket aMarket)
        {
            string lExchangeAddress = FExchanger.GetDepositAddress(aMarket);

            decimal lAmount = aMarket.IsSell ? aAmount : aAmount + (aAmount * (decimal)0.025);

            ulong lTxFee = FWallet.CalculateTxFee(lExchangeAddress, lAmount, FWallet.ActiveCurrencyID);

            BalanceViewModel lBalanceModel = FWallet.GetBalance(FWallet.ActiveCurrencyID);
            decimal lBalance = Convert.ToDecimal(lBalanceModel.ToString());

            string lTxID = ExecuteSendTxDialog(lAmount, FWallet.ActiveCurrencyID, lTxFee, lBalance, lExchangeAddress);

            if (string.IsNullOrEmpty(lTxID))
            {
                throw new Exception("Failed to create new Exchange Transaction");
            }

            decimal lCoinSentAmount = TransactionDetailDialog.isSubstractFeeChecked ? aAmount - (lTxFee / (decimal)FWallet.Coin) : aAmount;

            MarketOrder lNewOrder = new MarketOrder
            {
                CoinTxID = lTxID,
                SentQuantity = lCoinSentAmount,
                Market = aMarket.MarketName,
                Rate = aPriceTarget,
                Status = OrderStatus.Waiting,
                BaseTicker = aMarket.BaseTicker,
                OpenTime = DateTime.UtcNow,
                Name = MainForm.ExchangeTransactionName
            };

            FDBExchanger.WriteTransaction(lNewOrder);

            FDBExchanger.ReadTransactions(out MarketOrder[] lOrders, aMarket.BaseTicker);

            MarketOrder lOrderWithID = lOrders.Where(x => x.CoinTxID == lTxID).First();

            if (!FExchangeCurrencyOrders.ContainsKey(FWallet.ActiveCurrencyID))
            {
                FExchangeCurrencyOrders[FWallet.ActiveCurrencyID] = new List<MarketOrder>();
            }

            FExchangeCurrencyOrders[FWallet.ActiveCurrencyID].Add(lOrderWithID);

            WriteTransactionLogEntry(lOrderWithID);
        }

        private void UpdateExchangeOrderTxRelationship()
        {
            try
            {
                if (FWallet != null && FExchanger.IsCredentialsSet)
                {
                    Dictionary<uint, List<TransactionViewModel>> aTxs = FWallet.TransactionsByCurrency;
                    List<CurrencyItem> lCoins = FWallet.UserCoins;

                    foreach (uint iter in aTxs.Keys)
                    {
                        string lCurrentTicker = lCoins.Where(x => x.Id == iter).Select(x => x.Ticker).FirstOrDefault();

                        if (lCurrentTicker == null)
                        {
                            continue;
                        }

                        int lConfirmations = FExchanger.GetConfirmations(lCurrentTicker);

                        if (lConfirmations < 0)
                        {
                            continue;
                        }

                        foreach (TransactionViewModel it in aTxs[iter].Where(x => x.Confirmation >= (ulong)lConfirmations))
                        {
                            if (!FExchangeCurrencyOrders.ContainsKey(iter))
                            {
                                continue;
                            }

                            MarketOrder lItem = FExchangeCurrencyOrders[iter].Find(x => x.CoinTxID == it.TransactionID);

                            if (lItem == null)
                            {
                                continue;
                            }

                            List<PandoraExchanger.ExchangeMarket> lExchangeCoinMarket;

                            try
                            {
                                lExchangeCoinMarket = FExchanger.GetMarketCoins(lCurrentTicker).ToList();
                            }
                            catch
                            {
                                continue;
                            }

                            PandoraExchanger.ExchangeMarket lMarket = lExchangeCoinMarket.Find(x => x.BaseTicker == lCurrentTicker && lItem.Market == x.MarketName);

                            if (lItem.Status == OrderStatus.Waiting && lMarket != null)
                            {
                                if (lItem.ErrorCounter % 60 == 0)
                                {
                                    try
                                    {
                                        FExchanger.PlaceOrder(lItem, lMarket);
                                        WriteTransactionLogEntry(lItem);
                                        lItem.ErrorCounter = 0;
                                    }
                                    catch (Exception ex)
                                    {
                                        lItem.ErrorCounter += 60;

                                        WriteTransactionLogEntry(lItem, OrderMessage.OrderMessageLevel.Error, "Error Placing Order: " + ex.Message);
                                        WriteTransactionLogEntry(lItem, OrderMessage.OrderMessageLevel.Info, "Retrying in 1 minute. Attempt: " + lItem.ErrorCounter / 60 + "/10");
                                    }
                                }
                                else
                                {
                                    lItem.ErrorCounter++;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.PandoraLog.GetPandoraLog().Write("Failed to Update Exchange Order Relationship. Details: " + ex.Message + " on " + ex.Source);
            }
            finally
            {
                Task.Delay(1000).Wait(FExchangeTaskCancellationSource.Token);

                FExchangeTasks[1] = Task.Run(() => UpdateExchangeOrderTxRelationship(), FExchangeTaskCancellationSource.Token);
            }
        }

        private void WriteTransactionLogEntry(MarketOrder aMarket, OrderMessage.OrderMessageLevel aLevel = OrderMessage.OrderMessageLevel.Info, string aMessage = null)
        {
            if (!string.IsNullOrEmpty(aMessage))
            {
                FDBExchanger.WriteOrderLog(aMarket.InternalID, aMessage, aLevel);
                return;
            }

            switch (aMarket.Status)
            {
                case OrderStatus.Waiting:
                    FDBExchanger.WriteOrderLog(aMarket.InternalID, "Starting Transaction Process", OrderMessage.OrderMessageLevel.Info);
                    FDBExchanger.WriteOrderLog(aMarket.InternalID, "Coins Sended to Exchange account", OrderMessage.OrderMessageLevel.Info);
                    FDBExchanger.WriteOrderLog(aMarket.InternalID, "Waiting for confirmations to place order", OrderMessage.OrderMessageLevel.StageChange);
                    break;

                case OrderStatus.Placed:
                    FDBExchanger.WriteOrderLog(aMarket.InternalID, "Transaction confirmed", OrderMessage.OrderMessageLevel.Info);
                    FDBExchanger.WriteOrderLog(aMarket.InternalID, "Placing Transaction on exchange. Uuid: " + aMarket.ID.ToString(), OrderMessage.OrderMessageLevel.StageChange);
                    break;

                case OrderStatus.Interrupted:
                    FDBExchanger.WriteOrderLog(aMarket.InternalID, "Transaction cancelled or not found on Exchange.", OrderMessage.OrderMessageLevel.FatalError);
                    break;

                case OrderStatus.Completed:
                    FDBExchanger.WriteOrderLog(aMarket.InternalID, "Transaction completed. Waiting for withdraw", OrderMessage.OrderMessageLevel.StageChange);
                    break;

                case OrderStatus.Withdrawed:
                    FDBExchanger.WriteOrderLog(aMarket.InternalID, "Money withdrawed to account", OrderMessage.OrderMessageLevel.Finisher);
                    break;
            }
            List<MarketOrder> lOrders = FExchangeCurrencyOrders[FWallet.ActiveCurrencyID];

            RefreshStatusOrderLog(lOrders, aMarket.InternalID);

            RefreshSelectedOrderLog(aMarket.InternalID);
        }
    }
}