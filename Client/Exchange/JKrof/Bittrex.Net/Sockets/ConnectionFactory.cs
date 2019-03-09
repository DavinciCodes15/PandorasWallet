using System.Collections.Generic;
using Pandora.Client.Exchange.JKrof.Interfaces;
using Pandora.Client.Exchange.JKrof.Logging;
using Microsoft.AspNet.SignalR.Client;

namespace Bittrex.Net.Sockets
{
    public class ConnectionFactory : IWebsocketFactory
    {
        public IWebsocket CreateWebsocket(Log log, string url)
        {
            return new BittrexHubConnection(log, new HubConnection(url));
        }

        public IWebsocket CreateWebsocket(Log log, string url, IDictionary<string, string> cookies, IDictionary<string, string> headers)
        {
            throw new System.NotImplementedException();
        }
    }
}
