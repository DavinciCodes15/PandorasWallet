using Newtonsoft.Json;
using Pandora.Client.PriceSource.Contracts;
using Pandora.Client.PriceSource.Models;
using Pandora.Client.PriceSource.SourceAPIs.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Pandora.Client.PriceSource.SourceAPIs.Entities
{
    internal class CoinGeckoAPI : IPriceSourceAPI
    {
        private const string FBaseAPIAddress = @"https://api.coingecko.com/api/v3";

        private HttpClient FHTTPClient;
        private CancellationTokenSource FCancellationTokenSource;

        /// <summary>
        /// This element is just a cache and should not be accesed directly. Use GetSupportedCoins instead
        /// </summary>
        private ConcurrentBag<CoinGeckoCoin> FCacheSupportedCoins;

        /// <summary>
        /// This element is just a cache and should not be accesed directly. Use GetSupportedVsCurrencies instead
        /// </summary>
        private ConcurrentBag<string> FCacheVsCoins;

        public int Id => 2;

        public string Name => "CoinGecko";

        public CoinGeckoAPI()
        {
            FHTTPClient = new HttpClient();
            FHTTPClient.Timeout = new TimeSpan(0, 0, 15);
            FCancellationTokenSource = new CancellationTokenSource();
        }

        public void Dispose()
        {
            FCancellationTokenSource.Cancel();
            FHTTPClient.Dispose();
        }

        public async Task<IEnumerable<ICurrencyPrice>> GetPrices(IEnumerable<string> aCurrencyTickers)
        {
            try
            {
                List<ICurrencyPrice> lResult = new List<ICurrencyPrice>();
                var lAPICurrencies = await GetSupportedCoins();
                var lVsCurrencies = await GetSupportedVsCurrencies();
                if (lAPICurrencies.Any() && lVsCurrencies.Any() && aCurrencyTickers.Any())
                {
                    var lCoinGeckoIds = lAPICurrencies.Where(lAPICurrency => aCurrencyTickers.Any(lTicker => string.Equals(lAPICurrency.Symbol, lTicker, StringComparison.OrdinalIgnoreCase))).Select(lAPICurrency => lAPICurrency.Id);
                    var lIdsQueryParam = lCoinGeckoIds.Aggregate((lId1, lId2) => string.Concat(lId1, "%2C", lId2));
                    var lVsIds = lVsCurrencies.Where(lSymbol => aCurrencyTickers.Any(lTicker => string.Equals(lSymbol, lTicker, StringComparison.OrdinalIgnoreCase)) || Enum.TryParse<FiatCurrencies>(lSymbol.ToUpperInvariant(), out _));
                    var lVsQueryParam = lVsIds.Aggregate((lSymbol1, lSymbol2) => string.Concat(lSymbol1, "%2C", lSymbol2));
                    var lPricesRequest = await FHTTPClient.GetAsync($"https://api.coingecko.com/api/v3/simple/price?ids={lIdsQueryParam}&vs_currencies={lVsQueryParam}");
                    if (lPricesRequest.IsSuccessStatusCode)
                    {
                        var lPrices = JsonConvert.DeserializeObject<IDictionary<string, IDictionary<string, decimal>>>(await lPricesRequest.Content.ReadAsStringAsync());
                        foreach (var lPrice in lPrices)
                        {
                            var lSupportedCoin = lAPICurrencies.SingleOrDefault(lCurrency => string.Equals(lCurrency.Id, lPrice.Key, StringComparison.OrdinalIgnoreCase));
                            if (lSupportedCoin != null)
                                lResult.AddRange(lPrice.Value.Select(lSubPrice => new PriceModel
                                {
                                    Name = lSupportedCoin.Name,
                                    Ticker = lSupportedCoin.Symbol,
                                    Reference = lSubPrice.Key,
                                    Price = lSubPrice.Value
                                }
                                ));
                        }
                    }
                }
                return lResult;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get currency prices for {Name}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ICurrencyPrice>> GetTokenPrices(IEnumerable<string> aContracts)
        {
            try
            {
                var lResult = new List<ICurrencyPrice>();
                var lVsCurrencies = await GetSupportedVsCurrencies();
                if (lVsCurrencies.Any() && aContracts.Any())
                {
                    var lContractQueryParam = aContracts.Aggregate((lId1, lId2) => string.Concat(lId1, "%2C", lId2));
                    var lVsQueryParam = lVsCurrencies.Aggregate((lSymbol1, lSymbol2) => string.Concat(lSymbol1, "%2C", lSymbol2));
                    var lPricesRequest = await FHTTPClient.GetAsync($"https://api.coingecko.com/api/v3/simple/token_price/ethereum?contract_addresses={lContractQueryParam}&vs_currencies={lVsQueryParam}");
                    if (lPricesRequest.IsSuccessStatusCode)
                    {
                        var lPrices = JsonConvert.DeserializeObject<IDictionary<string, IDictionary<string, decimal>>>(await lPricesRequest.Content.ReadAsStringAsync());
                        foreach (var lPrice in lPrices)
                        {
                            lResult.AddRange(lPrice.Value.Select(lSubPrice => new PriceModel
                            {
                                Name = lPrice.Key,
                                Reference = lSubPrice.Key,
                                Price = lSubPrice.Value
                            }
                            ));
                        }
                    }
                }
                return lResult;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get token prices for {Name}: {ex.Message}", ex);
            }
        }

        private async Task<IEnumerable<CoinGeckoCoin>> GetSupportedCoins()
        {
            var lSupportedCurrencies = new List<CoinGeckoCoin>();
            if (FCacheVsCoins == null || !FCacheSupportedCoins.Any())
            {
                var lCoinRequest = await FHTTPClient.GetAsync($"{FBaseAPIAddress}/coins/list?include_platform=true");
                if (lCoinRequest.IsSuccessStatusCode)
                    lSupportedCurrencies.AddRange(JsonConvert.DeserializeObject<IEnumerable<CoinGeckoCoin>>(await lCoinRequest.Content.ReadAsStringAsync()));
                FCacheSupportedCoins = new ConcurrentBag<CoinGeckoCoin>(lSupportedCurrencies);
            }
            return FCacheSupportedCoins;
        }

        private async Task<IEnumerable<string>> GetSupportedVsCurrencies()
        {
            var lVsCurrencies = new List<string>();
            if (FCacheVsCoins == null || !FCacheVsCoins.Any())
            {
                var lSupportedCoinsRequest = await FHTTPClient.GetAsync($"{FBaseAPIAddress}/simple/supported_vs_currencies");
                if (lSupportedCoinsRequest.IsSuccessStatusCode)
                    lVsCurrencies.AddRange(JsonConvert.DeserializeObject<IEnumerable<string>>(await lSupportedCoinsRequest.Content.ReadAsStringAsync()));
                FCacheVsCoins = new ConcurrentBag<string>(lVsCurrencies);
            }
            return FCacheVsCoins;
        }

        public bool TestConnection()
        {
            try
            {
                var lPingResult = FHTTPClient.GetAsync($"{FBaseAPIAddress}/ping", FCancellationTokenSource.Token).Result;
                return lPingResult.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private class CoinGeckoCoin
        {
            public string Id { get; set; }
            public string Symbol { get; set; }
            public string Name { get; set; }
            public Dictionary<string, string> Platforms { get; set; }
        }
    }
}