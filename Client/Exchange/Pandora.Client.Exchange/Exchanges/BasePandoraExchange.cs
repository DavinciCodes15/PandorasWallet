using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.Exchanges
{
    public abstract class BasePandoraExchange : IPandoraExchange
    {
        protected static string ExchangeName => null;
        protected static uint ExchangeID => 0;

        public virtual string Name => (string)this.GetType().GetProperty("ExchangeName", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null);
        public virtual uint ID => (uint)this.GetType().GetProperty("ExchangeID", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null);

        protected string[] FUserCredentials;

        public bool IsCredentialSet => FUserCredentials != null && FUserCredentials.Length > 0;

        public virtual void SetCredentials(params string[] aKeys)
        {
            FUserCredentials = aKeys;
        }

        public abstract bool TestCredentials();

        public abstract string GetDepositAddress(string aCoinIdentifier);

        public abstract decimal GetExchangeTxMinConfirmations(string aCoinIdentifier);

        public abstract decimal GetExchangeCoinTxFee(string aCoinIdentifier);

        public abstract ExchangeMarket2[] GetCoinExchangeMarkets(string aCoinIdentifier);

        public abstract decimal GetMarketPrice(string aMarket);

        public abstract MarketOrder GetOrderStatus(string lOrderUid);

        public abstract decimal GetUserAvailableBalance(string aCoinIdentifier);

        public abstract MarketOrder PlaceOrder(bool isSell, string aMarketName, decimal aQuantity, decimal aRate);

        public abstract void Withdraw(string aAddress, string aCoinTicker, decimal aQuantity, decimal aFee);

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        public abstract event Action OnMarketPricesChanging;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    FUserCredentials = null;
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public abstract decimal CalculateExchangeTradingFee(decimal aValue);

        public abstract bool CancelOrder(string aUid);

        #endregion IDisposable Support
    }
}