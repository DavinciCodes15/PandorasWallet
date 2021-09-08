using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange
{
    public enum OrderStatus
    {
        Waiting, Placed, Interrupted, Completed, Withdrawn, Initial
    }

    public class OrderMessage : System.Collections.IComparer
    {
        public enum OrderMessageLevel
        {
            None = 0, Info = 1, StageChange = 2, Error = 3, FatalError = 4, Finisher = 5
        }

        public int ID { get; set; }
        public string Message { get; set; }
        public DateTime Time { get; set; }
        public OrderMessageLevel Level { get; set; }

        public int Compare(object x, object y)
        {
            OrderMessage lx = (OrderMessage)x;
            OrderMessage ly = (OrderMessage)y;

            return lx.Time.CompareTo(ly.Time);
        }
    }
}