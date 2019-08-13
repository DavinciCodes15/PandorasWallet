using Pandora.Client.Exchange.Exchanges;
using Pandora.Client.Exchange.Factories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange
{
    public class PandoraExchangeEntity : IDisposable
    {
        private const int FOrderTrackingPeriod = 5000;
        private uint FExchangeID;
        private IPandoraExchangeFactory FExchangeFactory;
        private IPandoraExchange FExchange;
        private Dictionary<int, string> FOrdersTracked;
        private Timer FOrderTrackingTimer;

        public delegate void OrderStateChangeDelegate(int[] aInternalID);

        public event OrderStateChangeDelegate OnOrderCompleted;

        public event OrderStateChangeDelegate OnOrderCancelled;

        public event Action OnMarketPriceUpdating;

        public bool OnOrderCompletedSet { get => OnOrderCompleted != null; }
        public bool OnOrderCancellaedSet { get => OnOrderCompleted != null; }

        public PandoraExchangeEntity(uint aExchangeID)
        {
            FExchangeID = aExchangeID;
            FOrdersTracked = new Dictionary<int, string>();
            FExchangeFactory = PandoraExchangeFactoryProducer.GetInstance().GetExchangeFactory(aExchangeID);
            FOrderTrackingTimer = new Timer(OrderTrackingCoreThread, null, FOrderTrackingPeriod, Timeout.Infinite);
        }

        private void OrderTrackingCoreThread(object aState)
        {
            try
            {
                Dictionary<int, string> lOrdersToTrack;
                List<int> lOrdersCompleted = new List<int>();
                List<int> lOrdersCancelled = new List<int>();
                lock (FOrdersTracked)
                    lOrdersToTrack = new Dictionary<int, string>(FOrdersTracked);
                foreach (var lOrder in lOrdersToTrack)
                {
                    var lRemoteOrder = FExchange.GetOrderStatus(lOrder.Value);
                    if (lRemoteOrder.Completed)
                        lOrdersCompleted.Add(lOrder.Key);
                    else if (lRemoteOrder.Cancelled)
                        lOrdersCancelled.Add(lOrder.Key);
                }
                if (lOrdersCompleted.Any())
                    OnOrderCompleted?.Invoke(lOrdersCompleted.ToArray());
                if (lOrdersCancelled.Any())
                    OnOrderCancelled?.Invoke(lOrdersCancelled.ToArray());
                lock (FOrdersTracked)
                    foreach (var lInternalId in lOrdersCompleted.Concat(lOrdersCancelled))
                        lOrdersToTrack.Remove(lInternalId);
            }
            finally
            {
                FOrderTrackingTimer?.Change(FOrderTrackingPeriod, Timeout.Infinite);
            }
        }

        public ExchangeMarket2[] GetExchangeMarkets(string aCoinIdentifier)
        {
            if (FExchange == null || !FExchange.IsCredentialSet)
                throw new Exception("Credentials not set");
            if (string.IsNullOrEmpty(aCoinIdentifier))
                throw new ArgumentException("Coin identifier must have a value", nameof(aCoinIdentifier));
            return FExchange.GetCoinExchangeMarkets(aCoinIdentifier);
        }

        public decimal GetMarketPrice(string aMarketIdentifier)
        {
            if (FExchange == null || !FExchange.IsCredentialSet)
                throw new Exception("Credentials not set");
            if (string.IsNullOrEmpty(aMarketIdentifier))
                throw new ArgumentException("A market name must be provided", nameof(aMarketIdentifier));
            return FExchange.GetMarketPrice(aMarketIdentifier);
        }

        public void SetCredentials(params string[] aCredentials)
        {
            var lExchange = FExchangeFactory.GetNewPandoraExchange(aCredentials);
            if (!lExchange.TestCredentials())
                throw new PandoraExchangeExceptions.InvalidExchangeCredentials("API Keys provided not valid");
            Interlocked.Exchange(ref FExchange, lExchange);
            FExchange.OnMarketPricesChanging += OnMarketPriceUpdating;
        }

        public int GetMinimumConfirmations(string aCoinTicker)
        {
            return (int)FExchange.GetExchangeTxMinConfirmations(aCoinTicker);
        }

        public string PlaceOrder(MarketOrder aOrderToBePlaced)
        {
            bool lIsSell = aOrderToBePlaced.MarketInstance.IsSell;
            decimal lOrderQuantity;

            if (FExchange == null || !FExchange.IsCredentialSet)
                throw new Exception("Credentials not set");
            if (aOrderToBePlaced == null)
                throw new ArgumentException("Order cannot be null", nameof(aOrderToBePlaced));
            if (!string.IsNullOrEmpty(aOrderToBePlaced.ID))
                throw new Exception("Order already placed. Order uid is present");
            if (lIsSell)
                lOrderQuantity = aOrderToBePlaced.SentQuantity;
            else
                lOrderQuantity = aOrderToBePlaced.SentQuantity / aOrderToBePlaced.Rate;
            var lUserBalance = FExchange.GetUserAvailableBalance(aOrderToBePlaced.MarketInstance.BaseTicker);
            if (lOrderQuantity > lUserBalance)
                throw new Exception("Not enough available balance present in exchange to place order");
            var lRemoteOrder = FExchange.PlaceOrder(lIsSell, aOrderToBePlaced.MarketInstance.MarketName, lOrderQuantity, aOrderToBePlaced.Rate);
            if (string.IsNullOrEmpty(lRemoteOrder.ID))
                throw new Exception("Failed to get Exchange Order UID");
            return lRemoteOrder.ID;
        }

        public string GetDepositAddress(string aCoinIdentifier)
        {
            if (FExchange == null || !FExchange.IsCredentialSet)
                throw new Exception("Credentials not set");
            if (string.IsNullOrEmpty(aCoinIdentifier))
                throw new ArgumentException("Coin identifier must be provided", nameof(aCoinIdentifier));
            return FExchange.GetDepositAddress(aCoinIdentifier);
        }

        public void WithdrawOrder(MarketOrder aOrderToWithdraw, string aAddress)
        {
            if (FExchange == null || !FExchange.IsCredentialSet)
                throw new Exception("Credentials not set");
            if (aOrderToWithdraw == null)
                throw new ArgumentException("Order cannot be null", nameof(aOrderToWithdraw));
            if (string.IsNullOrEmpty(aOrderToWithdraw.ID))
                throw new Exception("Order must have an uid.");
            var lOrderMarket = aOrderToWithdraw.MarketInstance;
            decimal lRate = lOrderMarket.IsSell ? 1 / aOrderToWithdraw.Rate : aOrderToWithdraw.Rate;
            decimal lRawAmount = aOrderToWithdraw.SentQuantity / lRate;
            decimal lTradingFee = Math.Round(FExchange.CalculateExchangeTradingFee(lRawAmount), 8);
            decimal lTxFee = FExchange.GetExchangeCoinTxFee(lOrderMarket.CoinTicker);
            decimal lTradedAmount = Math.Round(lRawAmount - lTradingFee, 8);
            var lUserBalance = FExchange.GetUserAvailableBalance(aOrderToWithdraw.MarketInstance.CoinTicker);
            if (lTradedAmount > lUserBalance)
                throw new Exception("Not enough available balance present in exchange to place order");
            FExchange.Withdraw(aAddress, lOrderMarket.CoinTicker, lTradedAmount, lTxFee);
        }

        public void AddOrderToTrack(MarketOrder aOrderToBeTracked)
        {
            if (FExchange == null || !FExchange.IsCredentialSet)
                throw new Exception("Credentials not set");
            if (string.IsNullOrEmpty(aOrderToBeTracked.ID))
                throw new Exception("Order needs to be placed before tracking");
            if (FOrdersTracked.ContainsKey(aOrderToBeTracked.InternalID))
                throw new Exception("Order already tracked");
            lock (FOrdersTracked)
                FOrdersTracked.Add(aOrderToBeTracked.InternalID, aOrderToBeTracked.ID);
        }

        public bool CancelOrder(string aUid)
        {
            if (FExchange == null || !FExchange.IsCredentialSet)
                throw new Exception("Credentials not set");

            return FExchange.CancelOrder(aUid);
        }

        public void Dispose()
        {
            FExchange.OnMarketPricesChanging -= OnMarketPriceUpdating;
            FOrderTrackingTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            FOrderTrackingTimer.Dispose();
            lock (FOrdersTracked)
                FOrdersTracked.Clear();
        }
    }
}