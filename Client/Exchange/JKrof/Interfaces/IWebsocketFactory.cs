using System.Collections.Generic;
using Pandora.Client.Exchange.JKrof.Logging;

namespace Pandora.Client.Exchange.JKrof.Interfaces
{
    public interface IWebsocketFactory
    {
        IWebsocket CreateWebsocket(Log log, string url);
        IWebsocket CreateWebsocket(Log log, string url, IDictionary<string, string> cookies, IDictionary<string, string> headers);
    }
}
