using System;
using System.Net;
using System.Net.Http;
using Pandora.Client.Exchange.JKrof.Interfaces;
using Pandora.Client.Exchange.JKrof.Objects;

namespace Pandora.Client.Exchange.JKrof.Requests
{
    /// <summary>
    /// WebRequest factory
    /// </summary>
    public class RequestFactory : IRequestFactory
    {
        private HttpClient httpClient;

        /// <inheritdoc />
        public void Configure(TimeSpan requestTimeout, ApiProxy proxy)
        {
            HttpMessageHandler handler = new HttpClientHandler()
            {
                Proxy = proxy == null ? null : new WebProxy
                {
                    Address = new Uri($"{proxy.Host}:{proxy.Port}"),
                    Credentials = proxy.Password == null ? null : new NetworkCredential(proxy.Login, proxy.Password)
                }
            };

            httpClient = new HttpClient(handler) {Timeout = requestTimeout};
        }

        /// <inheritdoc />
        public IRequest Create(HttpMethod method, string uri)
        {
            if (httpClient == null)
                throw new InvalidOperationException("Cant create request before configuring http client");

            return new Request(new HttpRequestMessage(method, uri), httpClient);
        }
    }
}
