using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.Objects
{
    public class UserTradeOrder : TradeOrderStatusInfo
    {
        public UserTradeOrder()
        {
            BaseCurrency = new BaseCurrencyInfo();
        }
        public decimal SentQuantity { get; set; }
        public DateTime OpenTime { get; set; }
        public OrderStatus Status { get; set; }
        public string CoinTxID { get; set; }
        public int InternalID { get; set; }
        public int PandoraExchangeID { get; set; }
        public BaseCurrencyInfo BaseCurrency { get; private set; }
        public string Name { get; set; }
        public int ErrorCounter { get; set; }
        public decimal StopPrice { get; set; }
        public int ProfileID { get; set; }

        public class BaseCurrencyInfo
        {
            public string Ticker { get; set; }
            public long ID { get; set; }
        }

        public UserTradeOrder Clone()
        {
            return new UserTradeOrder
            {
                SentQuantity = this.SentQuantity,
                Rate = this.Rate,
                ID = this.ID,
                OpenTime = this.OpenTime,
                Cancelled = this.Cancelled,
                Completed = this.Completed,
                ExchangeMarketName = this.ExchangeMarketName,
                Status = this.Status,
                CoinTxID = this.CoinTxID,
                InternalID = this.InternalID,
                BaseCurrency = this.BaseCurrency,
                Name = this.Name,
                ErrorCounter = this.ErrorCounter,
                StopPrice = this.StopPrice,
                ProfileID = this.ProfileID
            };
        }
    }


}