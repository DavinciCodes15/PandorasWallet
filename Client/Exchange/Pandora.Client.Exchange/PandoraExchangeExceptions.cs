using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange
{
    public static class PandoraExchangeExceptions
    {
        public class InvalidMarketException : Exception
        {
            public InvalidMarketException(string aMessage) : base(aMessage)
            {
            }
        }

        public class InvalidExchangeCredentials : Exception
        {
            public InvalidExchangeCredentials(string aMessage) : base(aMessage)
            {
            }
        }
    }
}