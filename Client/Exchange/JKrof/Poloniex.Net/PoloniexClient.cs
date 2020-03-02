using Pandora.Client.Exchange.JKrof.Authentication;
using Pandora.Client.Exchange.JKrof.Interfaces;
using Pandora.Client.Exchange.JKrof.Logging;
using Pandora.Client.Exchange.JKrof.Objects;
using Pandora.Client.Exchange.JKrof.Poloniex.Net.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange.JKrof.Poloniex.Net
{
    public class PoloniexClient : RestClient
    {
        private static PoloniexClientOptions defaultOptions = new PoloniexClientOptions();
        private static PoloniexClientOptions DefaultOptions => defaultOptions.Copy();

        private const string FPrivateApiUrl = "https://poloniex.com/tradingApi";

        /// <summary>
        /// Create a new instance of BittrexClient using the default options
        /// </summary>
        public PoloniexClient() : this(DefaultOptions)
        {
            requestBodyFormat = RequestBodyFormat.FormData;
        }

        /// <summary>
        /// Create a new instance of the BittrexClient with the provided options
        /// </summary>
        public PoloniexClient(PoloniexClientOptions options) : base(options, options.ApiCredentials == null ? null : new PoloniexAuthenticationProvider(options.ApiCredentials))
        {
            requestBodyFormat = RequestBodyFormat.FormData;
        }

        /// <summary>
        /// Sets the default options to use for new clients
        /// </summary>
        /// <param name="options">The options to use for new clients</param>
        public static void SetDefaultOptions(PoloniexClientOptions options)
        {
            defaultOptions = options;
        }

        /// <summary>
        /// Set the API key and secret. Api keys can be managed at https://bittrex.com/Manage#sectionApi
        /// </summary>
        /// <param name="apiKey">The api key</param>
        /// <param name="apiSecret">The api secret</param>
        public void SetApiCredentials(string apiKey, string apiSecret)
        {
            SetAuthenticationProvider(new PoloniexAuthenticationProvider(new ApiCredentials(apiKey, apiSecret)));
        }

        public async Task<WebCallResult<Dictionary<string, PoloniexCurrencyPairSummary>>> GetTickerMarketsAsync(CancellationToken act = default)
        {
            return await Execute<Dictionary<string,PoloniexCurrencyPairSummary>>(new Uri("https://poloniex.com/public?command=returnTicker"),HttpMethod.Get, act).ConfigureAwait(false);
        }

        public WebCallResult<Dictionary<string, PoloniexCurrencyPairSummary>> GetTickerMarkets()
        {
            return GetTickerMarketsAsync().Result;
        }
               
        public async Task<WebCallResult<Dictionary<string, PoloniexCurrency>>> GetCurrenciesAsync(CancellationToken act = default)
        {
            return await Execute<Dictionary<string, PoloniexCurrency>>(new Uri("https://poloniex.com/public?command=returnCurrencies"), HttpMethod.Get, act).ConfigureAwait(false);
        }

        public WebCallResult<Dictionary<string, PoloniexCurrency>> GetCurrencies()
        {
            return GetCurrenciesAsync().Result;
        }

        public async Task<WebCallResult<Dictionary<string,string>>> GetDepositAddressesAsync(CancellationToken act = default)
        {
            var lParamethers = new Dictionary<string, object>
            {
                {"command", "returnDepositAddresses"}
            };
            return await Execute<Dictionary<string, string>>(new Uri(FPrivateApiUrl), HttpMethod.Post, act, true, lParamethers).ConfigureAwait(false);
        }

        public WebCallResult<Dictionary<string,string>> GetDepositAddresses()
        {
            return GetDepositAddressesAsync().Result;
        }

        public async Task<WebCallResult<PoloniexOrderStatusResult>> GetOrderStatusAsync(long aOrderNumber, CancellationToken act = default)
        {
            var lParamethers = new Dictionary<string, object>
            {
                {"command", "returnOrderStatus"},
                {"orderNumber", aOrderNumber.ToString(System.Globalization.CultureInfo.InvariantCulture) }
            };
            var lResponse = await Execute<PoloniexOrderStatusResult>(new Uri(FPrivateApiUrl), HttpMethod.Post, act, true, lParamethers).ConfigureAwait(false);
            return lResponse;
        }

        public WebCallResult<PoloniexOrderStatusResult> GetOrderStatus(long aOrderNumber)
        {
            return GetOrderStatusAsync(aOrderNumber).Result;
        }

        public async Task<WebCallResult<PoloniexOrderTradeResult>> GetOrderTradesAsync(long aOrderNumber, CancellationToken act = default)
        {
            var lParamethers = new Dictionary<string, object>
            {
                {"command", "returnOrderTrades"},
                {"orderNumber", aOrderNumber.ToString(System.Globalization.CultureInfo.InvariantCulture) }
            };
            var lResponse = await Execute<PoloniexOrderTradeResult>(new Uri(FPrivateApiUrl), HttpMethod.Post, act, true, lParamethers).ConfigureAwait(false);
            return lResponse;
        }

        public WebCallResult<PoloniexOrderTradeResult> GetOrderTrades(long aOrderNumber)
        {
            return GetOrderTradesAsync(aOrderNumber).Result;
        }

        public async Task<WebCallResult<PoloniexGenerateNewAddress>> GenerateNewAddressAsync(string aCurrencyTicker, CancellationToken act = default)
        {
            var lParamethers = new Dictionary<string, object>
            {
                {"command", "generateNewAddress"},
                {"currency", aCurrencyTicker }
            };
            var lResponse = await Execute<PoloniexGenerateNewAddress>(new Uri(FPrivateApiUrl), HttpMethod.Post, act, true, lParamethers).ConfigureAwait(false);
            return lResponse;

        }

        public WebCallResult<PoloniexGenerateNewAddress> GenerateNewAddress(string aCurrencyTicker)
        {
            return GenerateNewAddressAsync(aCurrencyTicker).Result;
        }

        public async Task<WebCallResult<PoloniexOrderPlaceResult>> PlaceBuyOrderAsync(string aCurrencyPair, decimal aRate, decimal aAmount, bool aFillOrKill = false, bool aImmediateOrCancel = false, bool aPostOnly = false, long aClientOrderId = -1, CancellationToken act = default)
        {
            var lParamethers = new Dictionary<string, object>
            {
                {"command", "buy"},
                {"currencyPair", aCurrencyPair },
                {"rate", aRate.ToString(System.Globalization.CultureInfo.InvariantCulture) },
                {"amount", aAmount.ToString(System.Globalization.CultureInfo.InvariantCulture) }
            };
            if (aFillOrKill) lParamethers.Add("fillOrKill", "1");
            if (aImmediateOrCancel) lParamethers.Add("immediateOrCancel", "1");
            if (aPostOnly) lParamethers.Add("postOnly", "1");
            if (aClientOrderId >= 0) lParamethers.Add("clientOrderId", aClientOrderId.ToString());
            var lResponse = await Execute<PoloniexOrderPlaceResult>(new Uri(FPrivateApiUrl), HttpMethod.Post, act, true, lParamethers).ConfigureAwait(false);
            return lResponse;
        }

        public async Task<WebCallResult<Dictionary<string,decimal>>> GetBalancesAsync(CancellationToken act = default)
        {
            var lParamethers = new Dictionary<string, object>
            {
                {"command", "returnBalances"}
            };
            return await Execute<Dictionary<string, decimal>>(new Uri(FPrivateApiUrl),  HttpMethod.Post, act, true, lParamethers).ConfigureAwait(false);

        }

        public WebCallResult<Dictionary<string, decimal>> GetBalances()
        {
            return GetBalancesAsync().Result;
        }

        public WebCallResult<PoloniexOrderPlaceResult> PlaceBuyOrder(string aCurrencyPair, decimal aRate, decimal aAmount, bool aFillOrKill = false, bool aImmediateOrCancel = false, bool aPostOnly = false, long aClientOrderId = -1)
        {
            return PlaceBuyOrderAsync(aCurrencyPair, aRate, aAmount, aFillOrKill, aImmediateOrCancel, aPostOnly, aClientOrderId).Result;
        }

        public async Task<WebCallResult<PoloniexOrderPlaceResult>> PlaceSellOrderAsync(string aCurrencyPair, decimal aRate, decimal aAmount, bool aFillOrKill = false, bool aImmediateOrCancel = false, bool aPostOnly = false, long aClientOrderId = -1, CancellationToken act = default)
        {
            var lParamethers = new Dictionary<string, object>
            {
                {"command", "sell"},
                {"currencyPair", aCurrencyPair },
                {"rate", aRate.ToString(System.Globalization.CultureInfo.InvariantCulture) },
                {"amount", aAmount.ToString(System.Globalization.CultureInfo.InvariantCulture) }
            };
            if (aFillOrKill) lParamethers.Add("fillOrKill", "1");
            if (aImmediateOrCancel) lParamethers.Add("immediateOrCancel", "1");
            if (aPostOnly) lParamethers.Add("postOnly", "1");
            if (aClientOrderId >= 0) lParamethers.Add("clientOrderId", aClientOrderId.ToString());
            var lResponse = await Execute<PoloniexOrderPlaceResult>(new Uri(FPrivateApiUrl), HttpMethod.Post, act, true, lParamethers).ConfigureAwait(false);
            return lResponse;
        }

        public async Task<WebCallResult<PoloniexCancelOrderResult>> CancelOrderAsync(long aOrderNumber, long aClientOrderId = -1, CancellationToken act = default)
        {
            var lParamethers = new Dictionary<string, object>
            {
                {"command", "cancelOrder"}
            };
            if (aClientOrderId >= 0) lParamethers.Add("clientOrderId", aClientOrderId.ToString());
            else lParamethers.Add("orderNumber", aOrderNumber.ToString());
            var lResponse = await Execute<PoloniexCancelOrderResult>(new Uri(FPrivateApiUrl),  HttpMethod.Post, act, true, lParamethers).ConfigureAwait(false);
            return lResponse;
        }

        public WebCallResult<PoloniexCancelOrderResult> CancelOrder(long aOrderNumber, long aClientOrderId = -1)
        {
            return CancelOrderAsync(aOrderNumber, aClientOrderId).Result;
        }

        public async Task<WebCallResult<PoloniexWithdrawResult>> WithdrawAsync(string aCurrencyTicker, decimal aAmount, string aAddress, CancellationToken act = default)
        {
            var lParamethers = new Dictionary<string, object>
            {
                {"command", "withdraw"},
                {"currency", aCurrencyTicker },
                {"amount", aAmount.ToString(System.Globalization.CultureInfo.InvariantCulture) },
                {"address", aAddress }
            };
            var lResponse = await Execute<PoloniexWithdrawResult>(new Uri(FPrivateApiUrl), HttpMethod.Post, act, true, lParamethers).ConfigureAwait(false);
            return lResponse;
        }

        public WebCallResult<PoloniexWithdrawResult> Withdraw(string aCurrencyTicker, decimal aAmount, string aAddress)
        {
            return WithdrawAsync(aCurrencyTicker, aAmount, aAddress).Result;
        }


        public WebCallResult<PoloniexOrderPlaceResult> PlaceSellOrder(string aCurrencyPair, decimal aRate, decimal aAmount, bool aFillOrKill = false, bool aImmediateOrCancel = false, bool aPostOnly = false, long aClientOrderId = -1)
        {
            return PlaceSellOrderAsync(aCurrencyPair, aRate, aAmount, aFillOrKill, aImmediateOrCancel, aPostOnly, aClientOrderId).Result;
        }

        private async Task<WebCallResult<T>> Execute<T>(Uri uri, HttpMethod method, CancellationToken ct, bool signed = false, Dictionary<string, object> parameters = null) where T : class
        {
            return GetResult(await SendRequest<T>(uri, method, ct, parameters, signed).ConfigureAwait(false));
        }

        protected async override Task<WebCallResult<T>> GetResponse<T>(IRequest request, CancellationToken cancellationToken)
        {
            try
            {
                TotalRequestsMade++;
                var response = await request.GetResponse(cancellationToken).ConfigureAwait(false);
                var statusCode = response.StatusCode;
                var headers = response.ResponseHeaders;
                var responseStream = await response.GetResponseStream().ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    var lPoloniexSerializer = PoloniexJsonSerializer.GetSerializer();
                    var desResult = await Deserialize<T>(responseStream, lPoloniexSerializer).ConfigureAwait(false);
                    responseStream.Close();
                    response.Close();

                    return new WebCallResult<T>(statusCode, headers, desResult.Data, desResult.Error);
                }
                else
                {
                    using (var reader = new StreamReader(responseStream))
                    {
                        var data = await reader.ReadToEndAsync().ConfigureAwait(false);
                        responseStream.Close();
                        response.Close();
                        var parseResult = ValidateJson(data);
                        return new WebCallResult<T>(statusCode, headers, default, parseResult.Success ? ParseErrorResponse(parseResult.Data) : new ServerError(data));
                    }
                }
            }
            catch (HttpRequestException requestException)
            {
                log.Write(LogVerbosity.Warning, "Request exception: " + requestException.Message);
                return new WebCallResult<T>(null, null, default, new ServerError(requestException.Message));
            }
            catch (TaskCanceledException canceledException)
            {
                if (canceledException.CancellationToken == cancellationToken)
                {
                    // Cancellation token cancelled
                    log.Write(LogVerbosity.Warning, "Request cancel requested");
                    return new WebCallResult<T>(null, null, default, new CancellationRequestedError());
                }
                else
                {
                    // Request timed out
                    log.Write(LogVerbosity.Warning, "Request timed out");
                    return new WebCallResult<T>(null, null, default, new WebError("Request timed out"));
                }
            }
        }


        private static WebCallResult<T> GetResult<T>(WebCallResult<T> result) where T : class
        {
            if (result.Error != null || result.Data == null)
                return new WebCallResult<T>(result.ResponseStatusCode, result.ResponseHeaders, null, result.Error);
            ServerError lServerError = null; 
            if(!string.IsNullOrEmpty(result.Data.ToString()))
            {
                var lResultType = typeof(T);
                if (lResultType.IsGenericType && lResultType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                        var lData = result.Data as IDictionary<string, string>;
                        lServerError = lData != null && lData.ContainsKey("error") ? new ServerError(lData["error"].ToString()) : null;
                }
            }

            return new WebCallResult<T>(result.ResponseStatusCode, result.ResponseHeaders, lServerError != null? null : result.Data, lServerError); ;;
        }
    }
}
