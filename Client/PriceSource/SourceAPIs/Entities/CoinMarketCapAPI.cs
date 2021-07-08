using Pandora.Client.PriceSource.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandora.Client.PriceSource.SourceAPIs.Contracts;
using System.Net.Http;
using System.Threading;
using System.Web;
using System.Net;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Pandora.Client.PriceSource.Models;
using System.Net.Http.Headers;

namespace Pandora.Client.PriceSource.SourceAPIs.Entities
{
  
    public class CoinMarketCapAPI : IPriceSourceAPI
    { 
        //<summary>Uncompleted Code, cmc want money for each api call. 
        //Luis told me, leave code on standby. expensive fee</summary>

        private const string FBaseAPIAddress = @"https://pro-api.coinmarketcap.com/v1";

        public int Id => 1;

        public string Name => "CoinMarketCap";
        private const string FAPI_KEY = "48c59cd8-bc23-4f70-bab6-805e9b6aea2e";

        private HttpClient FHTTPClient;
        private CancellationTokenSource FCancellationTokenSource;

        /// <summary>
        /// This element is just a cache and should not be accesed directly. Use GetSupportedVsCurrencies instead
        /// </summary>
        //private ConcurrentBag<FiatCurrencies> FCacheVsCoins;

        /// <summary>
        /// This element is just a cache and should not be accesed directly. Use GetSupportedCoins instead
        /// </summary>

        private ConcurrentBag<FiatCMC> FCacheFiat;

        /// <summary>
        /// This element is just a cache and should not be accesed directly. Use GetSupportedCoins instead
        /// </summary>
        private ConcurrentBag<CoinInfoCMC> FCacheCoinsInfo;

        public CoinMarketCapAPI()
        {
            FHTTPClient = new HttpClient();
            FHTTPClient.Timeout = new TimeSpan(0, 0, 15);
            FCancellationTokenSource = new CancellationTokenSource();
            FHTTPClient.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", FAPI_KEY);
            FHTTPClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            ///*Each HTTP request must contain the header Accept: application/json.
            ///You should also send an Accept-Encoding: deflate, gzip header to receive data fast and efficiently.
        }

        public void Dispose()
        {
            FCancellationTokenSource.Cancel();
            FHTTPClient.Dispose();
        }

        public bool TestConnection()
        {
            try
            {
                //var lPingResult = FHTTPClient.GetAsync("/ping", FCancellationTokenSource.Token).Result;
                return false;// Disabled by default as this is just a mockup of this API
            }
            catch
            {
                return false;
            }
        }

        public Task<IEnumerable<ICurrencyPrice>> GetPrices(IEnumerable<string> aCurrencyTicker)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ICurrencyPrice>> GetTokenPrices(IEnumerable<string> aContracts)
        {
            throw new NotImplementedException();
        }

        private async Task<IEnumerable<CoinPriceCMC>> GetTokenPrices()
        {
            var lCoinPrice = new List<CoinPriceCMC>();

            var lCurrencyCoinPriceRequest = await FHTTPClient.GetAsync($"{FBaseAPIAddress}/cryptocurrency/listings/latest?sort=date_added&cryptocurrency_type=tokens&convert_id=2781"); //id USD	2781  CLP 2786 EUR 2790
            if (lCurrencyCoinPriceRequest.IsSuccessStatusCode)
            {
                var lText = await lCurrencyCoinPriceRequest.Content.ReadAsStringAsync();
                var lResponse = JsonConvert.DeserializeObject<ResponseCurrencyPrice>(lText);
            }
            return lCoinPrice;
        }

        private async Task<IEnumerable<CoinPriceCMC>> GetCurrencyPrices()
        {
            var lCoinPrice = new List<CoinPriceCMC>();

            var lCurrencyCoinPriceRequest = await FHTTPClient.GetAsync($"{FBaseAPIAddress}/cryptocurrency/listings/latest?sort=date_added&cryptocurrency_type=coins&convert_id=2781"); //id USD	2781  CLP 2786 EUR 2790
            if (lCurrencyCoinPriceRequest.IsSuccessStatusCode)
            {
                var lText = await lCurrencyCoinPriceRequest.Content.ReadAsStringAsync();
                var lResponse = JsonConvert.DeserializeObject<ResponseCurrencyPrice>(lText);
            }

            return lCoinPrice;
        }

        private async Task<IEnumerable<CoinInfoCMC>> GetSupportedCoins()
        {
            var lCurrencyInfo = new List<CoinInfoCMC>();
            if (FCacheCoinsInfo == null || !FCacheCoinsInfo.Any())
            {
                var lCurrencyInfoRequest = await FHTTPClient.GetAsync($"{FBaseAPIAddress}/cryptocurrency/map?sort=cmc_rank&listing_status=active");
                if (lCurrencyInfoRequest.IsSuccessStatusCode)
                {
                    var lText = await lCurrencyInfoRequest.Content.ReadAsStringAsync();
                    var lResponse = JsonConvert.DeserializeObject<ResponseMappedCoins>(lText);
                }
                FCacheCoinsInfo = new ConcurrentBag<CoinInfoCMC>(lCurrencyInfo);
            }
            return FCacheCoinsInfo;
        }

        private async Task<IEnumerable<FiatCMC>> GetFiatInfo()
        {
            var lSupportedFiat = new List<FiatCMC>();
            if (FCacheFiat == null || !FCacheFiat.Any())
            {
                var lFiatRequest = await FHTTPClient.GetAsync($"{FBaseAPIAddress}/fiat/map?sort=name&include_metals=false");
                if (lFiatRequest.IsSuccessStatusCode)
                {
                    // lSupportedFiat.AddRange(JsonConvert.DeserializeObject<IEnumerable<FiatCMC>>(await lCoinRequest.Content.ReadAsStringAsync()));
                    var lText = await lFiatRequest.Content.ReadAsStringAsync();
                    var lResponse = JsonConvert.DeserializeObject<ResponseMappedFiat>(lText);
                }
                FCacheFiat = new ConcurrentBag<FiatCMC>(lSupportedFiat);
            }
            return lSupportedFiat;
        }

        internal class ResponseMappedCoins
        {
            [JsonProperty("status")]
            private StatusAPI status { get; set; }

            [JsonProperty("data")]
            private IEnumerable<CoinInfoCMC> data { get; set; }
        }

        internal class ResponseMappedFiat
        {
            [JsonProperty("status")]
            private StatusAPI status { get; set; }

            [JsonProperty("data")]
            private IEnumerable<FiatCMC> data { get; set; }
        }

        internal class ResponseCurrencyPrice
        {
            [JsonProperty("status")]
            private StatusAPI status { get; set; }

            [JsonProperty("data")]
            private IEnumerable<CoinPriceCMC> data { get; set; }
        }

        internal class ResponseMappedTokens
        {
            [JsonProperty("status")]
            private StatusAPI status { get; set; }

            [JsonProperty("data")]
            private IEnumerable<CoinPriceCMC> data { get; set; }
        }

        private class PlatformCurrency
        {
            public int id { get; set; }
            public string name { get; set; }
            public string token_adress { get; set; }
            public string symbol { get; set; }
            public string slug { get; set; }
        }

        private class FiatCMC

        {
            public string id { get; set; }
            public string name { get; set; }
            public string sign { get; set; }
            public string symbol { get; set; }
        }

        private class CoinPriceCMC : CoinInfoCMC
        {
            //ids: USD	2781  CLP 2786 EUR 2790
            public Dictionary<string, PriceObj> quote { get; set; }
        }

        private class PriceObj
        {
            public decimal price { get; set; }
        }

        private class QuoteFiat : FiatCMC
        {
            public string price { get; set; }
            public string volume_24h { get; set; }
            public string last_updated { get; set; }
        }

        public class CoinInfoCMC
        {
            public int id { get; set; }
            public string name { get; set; }
            public string symbol { get; set; }
            public string slug { get; set; }
            public dynamic platform { get; set; }
        }

        private class StatusAPI
        {
            public DateTime timestamp { get; set; }
            public int error_code { get; set; }
            public string error_message { get; set; }
            public int elapsed { get; set; }
            public int credit_count { get; set; }
        }
    }
}