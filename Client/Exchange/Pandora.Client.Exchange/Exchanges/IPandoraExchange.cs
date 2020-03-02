using Pandora.Client.Exchange.Objects;
using Pandora.Client.Exchange.SaveManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.Exchanges
{
    public delegate long? GetWalletIDDelegate(string aCurrencyName, string aTicker);
    public interface IPandoraExchanger
    {
        string Name { get; }

        int ID { get; }
        string UID { get; }
        bool IsCredentialsSet { get; }
        event Action<IEnumerable<string>, int> OnMarketPricesChanging;
        void SetCredentials(string aApiKey, string aApiSecret);
        void Clear();
        decimal GetTransactionsFee(string aCurrencyName, string aTicker);
        int GetConfirmations(string aCurrencyName, string aTicker);
        decimal GetBalance(ExchangeMarket aMarket);
        bool PlaceOrder(UserTradeOrder aOrder, ExchangeMarket aMarket, bool aUseProxy = true);
        void CancelOrder(UserTradeOrder aOrder, bool aUseProxy = true);
        ExchangeMarket[] GetMarketCoins(string aCurrencyName, string aTicker, GetWalletIDDelegate aGetWalletIDFunction = null);
        string GetDepositAddress(ExchangeMarket aMarket);
        void StartMarketPriceUpdating();
        TradeOrderStatusInfo GetOrderStatus(string lUuid);
        bool RefundOrder(ExchangeMarket aMarket, UserTradeOrder aOrder, string aAddress, bool aUseProxy = true);
        bool WithdrawOrder(ExchangeMarket aMarket, UserTradeOrder aOrder, string aAddress, decimal aTxFee, bool aUseProxy = true);
        MarketPriceInfo GetMarketPrice(string aMarketName);
        void StopMarketUpdating();

    }
}