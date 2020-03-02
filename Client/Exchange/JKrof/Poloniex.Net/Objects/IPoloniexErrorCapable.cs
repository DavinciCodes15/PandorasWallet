using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.JKrof.Poloniex.Net.Objects
{
    internal interface IPoloniexErrorCapable
    {
        string error { get; set; }
    }
}
