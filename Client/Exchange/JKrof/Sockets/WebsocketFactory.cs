using System.Collections.Generic;
using Pandora.Client.Exchange.JKrof.Interfaces;
using Pandora.Client.Exchange.JKrof.Logging;

namespace Pandora.Client.Exchange.JKrof.Sockets
{
    public class WebsocketFactory : IWebsocketFactory
    {
        public IWebsocket CreateWebsocket(Log log, string url)
        {
            return new BaseSocket(log, url);
        }

        public IWebsocket CreateWebsocket(Log log, string url, IDictionary<string, string> cookies, IDictionary<string, string> headers)
        {
            return new BaseSocket(log, url, cookies, headers);
        }
    }
}
