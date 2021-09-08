using Pandora.Client.ClientLib.Contracts;
using Pandora.Client.PriceSource.Contracts;
using Pandora.Client.PriceSource.SourceAPIs;
using Pandora.Client.Universal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pandora.Client.PriceSource
{
    public class CurrencyPriceSource
    {
        private static CurrencyPriceSource FInstance;
        private ConcurrentDictionary<string, ICurrencyToken> FTokenWatchInventory;
        private ConcurrentDictionary<long, ICurrencyItem> FCurrencyWatchInventory;
        private ConcurrentDictionary<string, ICurrencyPrice> FCurrencyPrices;
        private ConcurrentDictionary<string, ICurrencyPrice> FTokenPrices;
        private CancellationTokenSource FCancelTokenSource;
        private Timer FCoreUpdaterTimer;

        public event Action<CurrencyPriceSource> OnPricesUpdated;

        ~CurrencyPriceSource()
        {
            FCancelTokenSource.Cancel();
            FCoreUpdaterTimer.Change(Timeout.Infinite, Timeout.Infinite);
            FCoreUpdaterTimer.Dispose();
        }

        private CurrencyPriceSource()
        {
            FTokenWatchInventory = new ConcurrentDictionary<string, ICurrencyToken>();
            FCurrencyWatchInventory = new ConcurrentDictionary<long, ICurrencyItem>();
            FCurrencyPrices = new ConcurrentDictionary<string, ICurrencyPrice>();
            FTokenPrices = new ConcurrentDictionary<string, ICurrencyPrice>();
            FCoreUpdaterTimer = new Timer(DoUpdatePrices, null, 1000, Timeout.Infinite);
            FCancelTokenSource = new CancellationTokenSource();
        }

        private void DoUpdatePrices(object state)
        {
            long lPeriod = 60000;
            try
            {
                if (FCurrencyWatchInventory.Any() || FTokenWatchInventory.Any())
                {
                    var lUpdated = new ConcurrentBag<string>();
                    var lPriceSourceAPIs = APIFactory.GetPriceAPIs();
                    foreach (var lPriceSource in lPriceSourceAPIs)
                    {
                        var lCurrencyPriceTask = lPriceSource.Item2.GetPrices(FCurrencyWatchInventory.Values.Select(lCurrency => lCurrency.Ticker));
                        var lTokenPriceTask = lPriceSource.Item2.GetTokenPrices(FTokenWatchInventory.Keys);
                        var lOperationTasks = new Task[]
                        {
                        lCurrencyPriceTask.ContinueWith((lTask) => ProcessNewPrices(lTask, FCurrencyPrices, lUpdated)),
                        lTokenPriceTask.ContinueWith((lTask) => ProcessNewPrices(lTask, FTokenPrices, lUpdated))
                        };
                        Task.WaitAll(lOperationTasks, FCancelTokenSource.Token);
                    }
                    OnPricesUpdated?.BeginInvoke(this, null, null);
                }
            }
            catch (Exception ex)
            {
                Log.Write(LogLevel.Error, $"DoUpdatePrices: Exception thrown: {ex}");
                lPeriod = 15000;
            }
            finally
            {
                FCoreUpdaterTimer.Change(lPeriod, Timeout.Infinite);
            }
        }

        private void ProcessNewPrices(Task<IEnumerable<ICurrencyPrice>> aTask, ConcurrentDictionary<string, ICurrencyPrice> aPrices, ConcurrentBag<string> aUpdated)
        {
            try
            {
                if (aTask.IsCompleted)
                {
                    var lNewPrices = aTask.Result;
                    foreach (var lPrice in lNewPrices)
                    {
                        if (!aUpdated.Contains(lPrice.Id))
                        {
                            aPrices.AddOrUpdate(lPrice.Id, lPrice, (lKey, lOld) => lPrice);
                            aUpdated.Add(lPrice.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(LogLevel.Error, $"Failed to update reference prices. Exception thrown: {ex}");
            }
        }

        public static CurrencyPriceSource GetInstance()
        {
            if (FInstance == null)
                FInstance = new CurrencyPriceSource();
            return FInstance;
        }

        public void AddCurrenciesToWatch(IEnumerable<ICurrencyItem> aCurrencies)
        {
            foreach (var lCurrency in aCurrencies)
                AddCurrenciesToWatch(lCurrency);
        }

        public void AddCurrenciesToWatch(IEnumerable<ICurrencyToken> aTokens)
        {
            foreach (var lToken in aTokens)
                AddCurrenciesToWatch(lToken);
        }

        public void AddCurrenciesToWatch(ICurrencyItem aCurrency)
        {
            FCurrencyWatchInventory.TryAdd(aCurrency.Id, aCurrency);
            FCoreUpdaterTimer.Change(5000, Timeout.Infinite);
        }

        public void AddCurrenciesToWatch(ICurrencyToken aToken)
        {
            FTokenWatchInventory.TryAdd(aToken.ContractAddress, aToken);
            FCoreUpdaterTimer.Change(5000, Timeout.Infinite);
        }

        public void RemoveCurrenciesToWatch(IEnumerable<ICurrencyItem> aCurrencies)
        {
            foreach (var lCurrency in aCurrencies)
                RemoveCurrenciesToWatch(lCurrency);
        }

        public void RemoveCurrenciesToWatch(IEnumerable<ICurrencyToken> aTokens)
        {
            foreach (var lToken in aTokens)
                RemoveCurrenciesToWatch(lToken);
        }

        public void RemoveCurrenciesToWatch(ICurrencyItem aCurrency) => FCurrencyWatchInventory.TryRemove(aCurrency.Id, out _);

        public void RemoveCurrenciesToWatch(ICurrencyToken aToken) => FTokenWatchInventory.TryRemove(aToken.ContractAddress, out _);

        public void ClearAllCurrenciesToWatch()
        {
            FCurrencyWatchInventory.Clear();
            FTokenWatchInventory.Clear();
        }

        public decimal GetPrice(long aCurrencyID, FiatCurrencies aBaseFiatCurrency)
        {
            decimal lResult = 0;
            if (FCurrencyWatchInventory.TryGetValue(aCurrencyID, out ICurrencyItem lCurrency))
            {
                var lFoundPrices = FCurrencyPrices.Where((lPricePair) => lPricePair.Key.Contains(lCurrency.Ticker.ToLowerInvariant())).Select(lPricePair => lPricePair.Value);
                var lPrices = lFoundPrices.Where(lFoundPrice => string.Equals(lFoundPrice.Ticker, lCurrency.Ticker, StringComparison.OrdinalIgnoreCase)
                                             && string.Equals(lFoundPrice.Reference, aBaseFiatCurrency.ToString(), StringComparison.OrdinalIgnoreCase));
                var lPrice = lPrices.Count() > 1 ? lPrices.FirstOrDefault(lPriceItem => string.Equals(lPriceItem.Name, lCurrency.Name, StringComparison.OrdinalIgnoreCase)) : lPrices.FirstOrDefault();
                if (lPrice != null)
                    lResult = lPrice.Price;
            }
            return lResult;
        }

        public decimal GetPrice(long aCurrencyID, long aBaseCurrencyID)
        {
            decimal lResult = 0;
            if (FCurrencyWatchInventory.TryGetValue(aCurrencyID, out ICurrencyItem lCurrency) && FCurrencyWatchInventory.TryGetValue(aBaseCurrencyID, out ICurrencyItem lBaseCurrency))
            {
                var lFoundPrices = FCurrencyPrices.Where((lPricePair) => string.Equals(lPricePair.Value.Ticker, lCurrency.Ticker, StringComparison.OrdinalIgnoreCase)).Select(lPricePair => lPricePair.Value).ToArray();
                if (lFoundPrices.Any())
                {
                    var lPrices = lFoundPrices.Where(lFoundPrice => string.Equals(lFoundPrice.Reference, lBaseCurrency.Ticker, StringComparison.OrdinalIgnoreCase));
                    var lPrice = lPrices.Count() > 1 ? lPrices.FirstOrDefault(lPriceItem => string.Equals(lPriceItem.Name, lCurrency.Name, StringComparison.OrdinalIgnoreCase)) : lPrices.FirstOrDefault();
                    if (lPrice != null)
                        lResult = lPrice.Price;
                    else
                    {
                        var lFoundBasePrices = FCurrencyPrices.Where((lPricePair) => lPricePair.Key.Contains(lBaseCurrency.Ticker.ToLowerInvariant()) && lPricePair.Value.Name.ToLowerInvariant().Contains(lBaseCurrency.Name.ToLowerInvariant())).Select(lPricePair => lPricePair.Value).ToArray();
                        if (lFoundBasePrices.Any())
                        {
                            var lJoinedPrices = from lRequested in lFoundPrices
                                                join lBase in lFoundBasePrices on lRequested.Reference equals lBase.Reference
                                                select new
                                                {
                                                    Price = lRequested.Price,
                                                    BasePrice = lBase.Price
                                                };
                            if (lJoinedPrices.Any())
                            {
                                var lPriceRelation = lJoinedPrices.First();
                                if (lPriceRelation.BasePrice > 0)
                                    lResult = lPriceRelation.Price / lPriceRelation.BasePrice;
                            }
                        }
                    }
                }
            }
            else if (aCurrencyID == aBaseCurrencyID) lResult = 1;
            return lResult;
        }

        public decimal GetTokenPrice(string aTokenContract, FiatCurrencies aBaseFiatCurrency)
        {
            decimal lResult = 0;
            if (FTokenWatchInventory.TryGetValue(aTokenContract, out ICurrencyToken lCurrencyToken))
            {
                var lFoundPrices = FTokenPrices.Where((lPricePair) => lPricePair.Key.Contains(lCurrencyToken.ContractAddress.ToLowerInvariant())).Select(lPricePair => lPricePair.Value);
                var lPrice = lFoundPrices.Where(lFoundPrice => string.Equals(lFoundPrice.Name, lCurrencyToken.ContractAddress, StringComparison.OrdinalIgnoreCase)
                                             && string.Equals(lFoundPrice.Reference, aBaseFiatCurrency.ToString(), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (lPrice != null)
                    lResult = lPrice.Price;
            }
            return lResult;
        }

        public decimal GetTokenPrice(string aTokenContract, long aBaseCurrencyID)
        {
            decimal lResult = 0;
            if (FTokenWatchInventory.TryGetValue(aTokenContract, out ICurrencyToken lCurrencyToken) && FCurrencyWatchInventory.TryGetValue(aBaseCurrencyID, out ICurrencyItem lBaseCurrency))
            {
                var lFoundPrices = FTokenPrices.Where((lPricePair) => lPricePair.Key.Contains(lCurrencyToken.ContractAddress.ToLowerInvariant())).Select(lPricePair => lPricePair.Value);
                if (lFoundPrices.Any())
                {
                    var lPrice = lFoundPrices.Where(lFoundPrice => string.Equals(lFoundPrice.Name, lCurrencyToken.ContractAddress, StringComparison.OrdinalIgnoreCase)
                             && string.Equals(lFoundPrice.Reference, lBaseCurrency.Ticker, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (lPrice != null)
                        lResult = lPrice.Price;
                    else
                    {
                        var lFoundBasePrices = FCurrencyPrices.Where((lPricePair) => lPricePair.Key.Contains(lBaseCurrency.Ticker.ToLowerInvariant())).Select(lPricePair => lPricePair.Value);
                        if (lFoundBasePrices.Any())
                        {
                            var lPrices = from lRequested in lFoundPrices
                                          join lBase in lFoundBasePrices on lRequested.Reference equals lBase.Reference
                                          select new
                                          {
                                              Price = lRequested.Price,
                                              BasePrice = lBase.Price
                                          };
                            if (lPrices.Any())
                            {
                                var lPriceRelation = lPrices.First();
                                if (lPriceRelation.BasePrice > 0)
                                    lResult = lPriceRelation.Price / lPriceRelation.BasePrice;
                            }
                        }
                    }
                }
            }
            return lResult;
        }
    }
}