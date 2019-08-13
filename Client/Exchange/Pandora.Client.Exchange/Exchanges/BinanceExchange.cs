using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.Exchanges
{
    public class BinanceExchange : BasePandoraExchange
    {
#pragma warning disable IDE0051 // Remove unused private members
        private static new string ExchangeName => "Binance";
#pragma warning restore IDE0051 // Remove unused private members
        private static new uint ExchangeID => 12;

        public override event Action OnMarketPricesChanging;

        public override string GetDepositAddress(string aCoinIdentifier)
        {
            throw new NotImplementedException();
        }

        public override decimal GetExchangeTxMinConfirmations(string aCoinIdentifier)
        {
            throw new NotImplementedException();
        }

        public override decimal GetExchangeCoinTxFee(string aCoinIdentifier)
        {
            throw new NotImplementedException();
        }

        public override ExchangeMarket2[] GetCoinExchangeMarkets(string aCoinIdentifier)
        {
            throw new NotImplementedException();
        }

        public override decimal GetMarketPrice(string aMarket)
        {
            throw new NotImplementedException();
        }

        public override MarketOrder GetOrderStatus(string lOrderUid)
        {
            throw new NotImplementedException();
        }

        public override decimal GetUserAvailableBalance(string aCoinIdentifier)
        {
            throw new NotImplementedException();
        }

        public override MarketOrder PlaceOrder(bool isSell, string aMarketName, decimal aQuantity, decimal aRate)
        {
            throw new NotImplementedException();
        }

        public override void Withdraw(string aAddress, string aCoinTicker, decimal aQuantity, decimal aFee)
        {
            throw new NotImplementedException();
        }

        public override bool TestCredentials()
        {
            throw new NotImplementedException();
        }

        public override decimal CalculateExchangeTradingFee(decimal aValue)
        {
            throw new NotImplementedException();
        }

        public override bool CancelOrder(string aUid)
        {
            throw new NotImplementedException();
        }
    }
}