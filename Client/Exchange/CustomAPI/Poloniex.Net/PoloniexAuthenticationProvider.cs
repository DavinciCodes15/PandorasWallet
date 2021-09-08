using CryptoExchange.Net;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.CustomAPI.Poloniex.Net
{
    public class PoloniexAuthenticationProvider : AuthenticationProvider
    {
        private static long nonce => DateTime.UtcNow.Ticks;
        private readonly HMACSHA512 encryptor;
        private readonly object locker;

        public PoloniexAuthenticationProvider(ApiCredentials credentials) : base(credentials)
        {
            locker = new object();
            encryptor = new HMACSHA512(Encoding.ASCII.GetBytes(credentials.Secret.GetString()));
        }

        public override Dictionary<string, string> AddAuthenticationToHeaders(string uri, HttpMethod method, Dictionary<string, object> parameters, bool signed, PostParameters postParameterPosition, ArrayParametersSerialization arraySerialization)
        {
            if (!signed)
                return new Dictionary<string, string>();

            var result = new Dictionary<string, string>();
            lock (locker)
            {
                parameters.Add("nonce", nonce);
                result.Add("Key", Credentials.Key.GetString());
                var lDataToSign = ConstructDataToSign(parameters);
                var lSignedValue = ByteToString(encryptor.ComputeHash(Encoding.UTF8.GetBytes(lDataToSign)));
                result.Add("Sign", lSignedValue);
            }
            return result;
        }

        public override Dictionary<string, object> AddAuthenticationToParameters(string uri, HttpMethod method, Dictionary<string, object> parameters, bool signed, PostParameters postParameterPosition, ArrayParametersSerialization arraySerialization)
        {
            return parameters;
        }

        private string ConstructDataToSign(Dictionary<string, object> aData)
        {
            StringBuilder lBuilder = new StringBuilder();
            var lData = aData.OrderBy(p => p.Key).ToArray();
            lBuilder.Append($"{lData[0].Key}={lData[0].Value.ToString()}");
            for (var lCounter = 1; lCounter < lData.Length; lCounter++)
                lBuilder.Append($"&{lData[lCounter].Key}={lData[lCounter].Value.ToString()}");
            return lBuilder.ToString();
        }

        public override string Sign(string toSign)
        {
            lock (locker)
                return BitConverter.ToString(encryptor.ComputeHash(Encoding.ASCII.GetBytes(toSign))).Replace("-", string.Empty);
        }
    }
}