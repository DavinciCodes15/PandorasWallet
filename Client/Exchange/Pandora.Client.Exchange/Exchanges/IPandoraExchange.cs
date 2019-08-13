using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pandora.Client.Exchange.PandoraExchanger;

namespace Pandora.Client.Exchange.Exchanges
{
    public interface IPandoraExchange : IDisposable
    {
        string Name { get; }

        uint ID { get; }

        bool IsCredentialSet { get; }

        event Action OnMarketPricesChanging;

        MarketOrder GetOrderStatus(string lOrderUid);

        void Withdraw(string aAddress, string aCoinTicker, decimal aQuantity, decimal aFee);

        MarketOrder PlaceOrder(bool isSell, string aMarketName, decimal aQuantity, decimal aRate);

        ExchangeMarket2[] GetCoinExchangeMarkets(string aCoinIdentifier);

        string GetDepositAddress(string aCoinIdentifier);

        decimal GetMarketPrice(string aMarket);

        void SetCredentials(params string[] aKeys);

        decimal GetExchangeCoinTxFee(string aCoinIdentifier);

        bool TestCredentials();

        decimal GetExchangeTxMinConfirmations(string aCoinIdentifier);

        decimal GetUserAvailableBalance(string aCoinIdentifier);

        decimal CalculateExchangeTradingFee(decimal aValue);

        bool CancelOrder(string aUid);
    }
}