using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange
{
    public class PandoraExchangeProfile
    {
        public uint ExchangeID { get; set; }
        public PandoraExchangeEntity ExchangeEntity { get; set; }
        public string Name { get; set; }
        public int ProfileID { get; set; }

        public void Close()
        {
            ExchangeEntity.Dispose();
        }
    }
}