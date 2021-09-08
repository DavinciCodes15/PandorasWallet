using Pandora.Client.ClientLib.Contracts;
using Pandora.Client.Exchange.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.Models
{
    public class UserTradeOrder : TradeOrderStatusInfo
    {
        public UserTradeOrder()
        {
        }

        public decimal SentQuantity { get; set; }
        public DateTime OpenTime { get; set; }
        public OrderStatus Status { get; set; }
        public string CoinTxID { get; set; }
        public int InternalID { get; set; }
        public string Name { get; set; }
        public int ErrorCounter { get; set; }
        public decimal StopPrice { get; set; }
        public int ProfileID { get; set; }
        public IExchangeMarket Market { get; set; }

        /// <summary>
        /// Tx fee retrieved when creating the tx. This value only exists in memory, so if it is null it must be recalculated
        /// </summary>
        public decimal? CoinSendingTxFee { get; set; }

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
                Market = this.Market,
                Status = this.Status,
                CoinTxID = this.CoinTxID,
                InternalID = this.InternalID,
                Name = this.Name,
                ErrorCounter = this.ErrorCounter,
                StopPrice = this.StopPrice,
                ProfileID = this.ProfileID,
                CoinSendingTxFee = this.CoinSendingTxFee
            };
        }
    }
}