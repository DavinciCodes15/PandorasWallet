using Newtonsoft.Json;

namespace Pandora.Client.Exchange.JKrof.Sockets
{
    public class SocketRequest
    {
        [JsonIgnore]
        public bool Signed { get; set; }
    }
}
