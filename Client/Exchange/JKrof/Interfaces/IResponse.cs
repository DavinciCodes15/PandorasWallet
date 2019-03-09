using System.IO;
using System.Net;

namespace Pandora.Client.Exchange.JKrof.Interfaces
{
    public interface IResponse
    {
        HttpStatusCode StatusCode { get; }
        Stream GetResponseStream();
        void Close();
    }
}
