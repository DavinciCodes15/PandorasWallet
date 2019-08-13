using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange
{
    public class MarketOrder
    {
        public decimal SentQuantity { get; set; }
        public decimal Rate { get; set; }
        public string ID { get; set; }
        public DateTime OpenTime { get; set; }
        public bool Cancelled { get; set; }
        public bool Completed { get; set; }
        public string Market { get; set; }
        public OrderStatus Status { get; set; }
        public string CoinTxID { get; set; }
        public int InternalID { get; set; }
        public string BaseTicker { get; set; }
        public string Name { get; set; }
        public int ErrorCounter { get; set; }
        public decimal StopPrice { get; set; }
        public ExchangeMarket2 MarketInstance { get; set; }
        public int ProfileID { get; set; }

        public MarketOrder Clone()
        {
            return new MarketOrder
            {
                SentQuantity = this.SentQuantity,
                Rate = this.Rate,
                ID = this.ID,
                OpenTime = this.OpenTime,
                Cancelled = this.Cancelled,
                Completed = this.Completed,
                Market = this.Market,
                Status = this.Status,
                CoinTxID = this.CoinTxID,
                InternalID = this.InternalID,
                BaseTicker = this.BaseTicker,
                Name = this.Name,
                ErrorCounter = this.ErrorCounter,
                StopPrice = this.StopPrice,
                MarketInstance = this.MarketInstance,
                ProfileID = this.ProfileID
            };
        }
    }
}