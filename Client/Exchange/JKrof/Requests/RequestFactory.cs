using System.Net;
using Pandora.Client.Exchange.JKrof.Interfaces;

namespace Pandora.Client.Exchange.JKrof.Requests
{
    public class RequestFactory : IRequestFactory
    {
        public IRequest Create(string uri)
        {
            return new Request(WebRequest.Create(uri));
        }
    }
}
