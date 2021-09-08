using Pandora.Client.ClientLib.Contracts;
using Pandora.Client.Exchange.Contracts;
using Pandora.Client.Exchange.Models;
using Pandora.Client.Exchange.SaveManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.Exchangers.Contracts
{
    public delegate long? GetWalletIDDelegate(string aTicker);

    public enum ChartInterval
    {
        Daily, Hourly, FiveMinutes
    }

    public interface IPandoraExchanger
    {
        event Action<IEnumerable<IExchangeMarket>> OnMarketPricesChanging;

        string Name { get; }

        int ID { get; }
        string UID { get; }
        bool IsCredentialsSet { get; }

        void SetCredentials(string aApiKey, string aApiSecret);

        void Clear();

        decimal GetTransactionsFee(ICurrencyIdentity aCurrency);

        int GetConfirmations(ICurrencyIdentity aCurrency);

        decimal GetBalance(IExchangeMarket aMarket);

        bool PlaceOrder(UserTradeOrder aOrder, bool aUseProxy = true);

        void CancelOrder(UserTradeOrder aOrder, bool aUseProxy = true);

        IEnumerable<IExchangeMarket> GetMarketCoins(ICurrencyIdentity aCurrencyIdentity, GetWalletIDDelegate aGetWalletIDFunction);

        string GetDepositAddress(IExchangeMarket aMarket);

        void StartMarketPriceUpdating();

        TradeOrderStatusInfo GetOrderStatus(string lUuid);

        bool RefundOrder(UserTradeOrder aOrder, string aAddress, bool aUseProxy = true);

        bool WithdrawOrder(UserTradeOrder aOrder, string aAddress, decimal aTxFee, bool aUseProxy = true);

        IMarketPriceInfo GetMarketPrice(IExchangeMarket aMarketExchange);

        IEnumerable<CandlestickPoint> GetCandleStickChart(IExchangeMarket aMarket, DateTime aStartTime, DateTime aEndTime, ChartInterval aChartInterval);

        void StopMarketUpdating();
    }
}