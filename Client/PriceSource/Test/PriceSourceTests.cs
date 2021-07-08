using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pandora.Client.ClientLib;
using Pandora.Client.ClientLib.Contracts;
using Pandora.Client.PandorasWallet.Dialogs.Models;
using System;
using System.Collections.Generic;
using Pandora.Client.PriceSource.SourceAPIs.Entities;
using Pandora.Client.PriceSource.SourceAPIs.Contracts;

namespace Pandora.Client.PriceSource.Test

{
    [TestClass]
    public class PriceSourceTests
    {
        private bool FCompleted = false;

        //GetSupportedCoins
        [TestMethod]
        public void TestCMCMappedCoins()
        {
            IPriceSourceAPI lApiCMCInstance = new CoinMarketCapAPI();
            var lSupportedCoins = lApiCMCInstance.GetPrices(new string[] { "biTCOIN" }).Result;
        }

        [TestMethod]
        public void TestCMCCurrencyTokenPrices()
        {
            var lApiCMCInstance = new CoinMarketCapAPI();
            var lSupportedFiat = lApiCMCInstance.GetTokenPrices(new string[] { "0x11d1d1d" }).Result;
        }

        [TestMethod]
        public void TestPriceSource()
        {
            var lPriceSource = CurrencyPriceSource.GetInstance();
            lPriceSource.OnPricesUpdated += PriceSource_OnPricesUpdated;
            var lTestCurrencies = new List<ICurrencyItem>
            {
                new CurrencyItem()
                {
                    Id = 1,
                    Name="Bitcoin",
                    Ticker = "BTC"
                },
                new CurrencyItem()
                {
                    Id=2,
                    Name ="Litecoin",
                    Ticker = "LTC"
                }
            };
            lPriceSource.AddCurrenciesToWatch(lTestCurrencies);
            while (!FCompleted) System.Threading.Thread.Sleep(1000);
        }

        private void PriceSource_OnPricesUpdated(CurrencyPriceSource aPriceSource)
        {
            var lBTCPrice = aPriceSource.GetPrice(1, FiatCurrencies.USD);
            var lLTCPrice = aPriceSource.GetPrice(2, FiatCurrencies.CLP);
            FCompleted = lBTCPrice > 0 && lLTCPrice > 0;
        }
    }
}