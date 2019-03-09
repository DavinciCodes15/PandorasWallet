using System.IO;
using System.Net;
using Pandora.Client.Exchange.JKrof.Interfaces;

namespace Pandora.Client.Exchange.JKrof.Requests
{
    public class Response : IResponse
    {
        private readonly HttpWebResponse response;

        public HttpStatusCode StatusCode => response.StatusCode;

        public Response(HttpWebResponse response)
        {
            this.response = response;
        }

        public Stream GetResponseStream()
        {
            return response.GetResponseStream();
        }

        public void Close()
        {
            response.Close();
        }
    }
}
