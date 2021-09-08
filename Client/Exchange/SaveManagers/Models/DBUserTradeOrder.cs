using Pandora.Client.Exchange.Models;
using System;

namespace Pandora.Client.Exchange.SaveManagers.Models
{
    public class DBUserTradeOrder : TradeOrderStatusInfo
    {
        public decimal SentQuantity { get; set; }
        public DateTime OpenTime { get; set; }
        public OrderStatus Status { get; set; }
        public string CoinTxID { get; set; }
        public int InternalID { get; set; }
        public string Name { get; set; }
        public decimal StopPrice { get; set; }
        public int ProfileID { get; set; }
        public string MarketID { get; set; }
        public long CurrencyId { get; set; }
        public string CurrencyTicker { get; set; }
        public int ExchangeID { get; set; }
    }
}