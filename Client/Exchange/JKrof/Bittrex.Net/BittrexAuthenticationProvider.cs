﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Pandora.Client.Exchange.JKrof;
using Pandora.Client.Exchange.JKrof.Authentication;
using Newtonsoft.Json;

namespace Bittrex.Net
{
    internal class BittrexAuthenticationProvider: AuthenticationProvider
    {
        private static long nonce => DateTime.UtcNow.Ticks;
        private readonly HMACSHA512 encryptor;
        private readonly object locker;

        public BittrexAuthenticationProvider(ApiCredentials credentials) : base(credentials)
        {
            if(credentials.Secret == null)
                throw new ArgumentException("ApiKey/Secret needed");

            locker = new object();
            encryptor = new HMACSHA512(Encoding.ASCII.GetBytes(credentials.Secret.GetString()));
        }

        public override Dictionary<string, object> AddAuthenticationToParameters(string uri, HttpMethod method, Dictionary<string, object> parameters, bool signed)
        {
            if (!signed)
                return parameters;

            if (Credentials.Key == null)
                throw new ArgumentException("ApiKey/Secret needed");

            lock (locker)
                parameters.Add("apiKey", Credentials.Key.GetString());
            parameters.Add("nonce", nonce);
            return parameters;
        }

        public override Dictionary<string, string> AddAuthenticationToHeaders(string uri, HttpMethod method, Dictionary<string, object> parameters, bool signed)
        {
            if (!signed)
                return new Dictionary<string, string>();

            var result = new Dictionary<string, string>();
            lock (locker)
                result.Add("apisign", ByteToString(encryptor.ComputeHash(Encoding.UTF8.GetBytes(uri))));
            return result;
        }

        public override string Sign(string toSign)
        {
            lock(locker)
                return BitConverter.ToString(encryptor.ComputeHash(Encoding.ASCII.GetBytes(toSign))).Replace("-", string.Empty);
        }
    }

    internal class BittrexAuthenticationProviderV3 : AuthenticationProvider
    {
        private readonly HMACSHA512 encryptorHmac;
        private readonly SHA512 encryptor;
        private readonly object locker;

        public BittrexAuthenticationProviderV3(ApiCredentials credentials) : base(credentials)
        {
            if (credentials.Secret == null)
                throw new ArgumentException("ApiKey/Secret needed");

            locker = new object();
            encryptorHmac = new HMACSHA512(Encoding.ASCII.GetBytes(credentials.Secret.GetString()));
            encryptor = SHA512.Create();
        }

        public override Dictionary<string, string> AddAuthenticationToHeaders(string uri, HttpMethod method, Dictionary<string, object> parameters, bool signed)
        {
            if (!signed)
                return new Dictionary<string, string>();

            if (Credentials.Key == null)
                throw new ArgumentException("ApiKey/Secret needed");

            var result = new Dictionary<string, string>();
            lock (locker)
                result.Add("Api-Key", Credentials.Key.GetString());
            result.Add("Api-Timestamp", Math.Round((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds).ToString(CultureInfo.InvariantCulture));
            var jsonContent = "";
            if(method != HttpMethod.Get && method != HttpMethod.Delete)
                jsonContent = JsonConvert.SerializeObject(parameters.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value));
            result.Add("Api-Content-Hash", ByteToString(encryptor.ComputeHash(Encoding.UTF8.GetBytes(jsonContent))).ToLower());

            uri = WebUtility.UrlDecode(uri); // Sign needs the query parameters to not be encoded
            var sign = result["Api-Timestamp"] + uri + method + result["Api-Content-Hash"] + "";
            result.Add("Api-Signature", ByteToString(encryptorHmac.ComputeHash(Encoding.UTF8.GetBytes(sign))));
            return result;
        }

        public override string Sign(string toSign)
        {
            lock (locker)
                return BitConverter.ToString(encryptor.ComputeHash(Encoding.ASCII.GetBytes(toSign))).Replace("-", string.Empty);
        }
    }
}
