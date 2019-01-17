using Microsoft.VisualStudio.TestTools.UnitTesting;
using NBitcoin;
using Pandora.Client.Crypto.Currencies;
using System;
using System.Collections.Generic;

namespace Pandora.Client.PandorasWallet.TestUnit
{
    [TestClass]
    public class AddressValidation
    {
        [TestMethod]
        public void CheckBitcoinAddress()
        {
            var CoinCurrency = Crypto.Currencies.Controls.CurrencyControl.GetCurrencyControl().GetCryptoCurrency(10001, "");

            List<string> GoodAddress = new List<string>();
            GoodAddress.Add("2NETQxWm2JGG1inn4UjoQUo9oXczYmjGYH2");
            GoodAddress.Add("mj4bcqYcnqJTEiP3iDbQVws8XsmSHzx892");

            List<string> BadAddress = new List<string>();
            BadAddress.Add("MPctwaAHZ3LSA8QTYpEZAGZ9u5jbTwJYQA");
            BadAddress.Add("QY48Q45ucnW8bxiZPui5QBYMEcWzXu8fFA");//Litecoin Testnet
            BadAddress.Add("QUqGzoo5oCEkLu1WbB8npRNdhdFWe9jnmD");
            BadAddress.Add("MPctwaAHZ3LSA8QTYpEZAGZ9u5jbTwJYQA"); //Litecoin MainNet

            foreach (string address in GoodAddress)
            {
                if (!CoinCurrency.IsValidAddress(address))
                {
                    Assert.IsTrue(false);
                }
            }

            foreach (string address in BadAddress)
            {
                if (CoinCurrency.IsValidAddress(address))
                {
                    Assert.IsTrue(false);
                }
            }
        }

        [TestMethod]
        public void CheckLitecoinAddress()
        {
            var CoinCurrency = Crypto.Currencies.Controls.CurrencyControl.GetCurrencyControl().GetCryptoCurrency(10002, "");

            List<string> GoodAddress = new List<string>();
            GoodAddress.Add("QY48Q45ucnW8bxiZPui5QBYMEcWzXu8fFA");
            GoodAddress.Add("QUqGzoo5oCEkLu1WbB8npRNdhdFWe9jnmD");
            GoodAddress.Add("mj4bcqYcnqJTEiP3iDbQVws8XsmSHzx892");

            List<string> BadAddress = new List<string>();
            BadAddress.Add("MPctwaAHZ3LSA8QTYpEZAGZ9u5jbTwJYQA"); //Litecoin MainNet
            BadAddress.Add("yfyzM58VsmjfbXtTzNrYH14TmJvY3Nn3Ms"); //Dash TestNet

            foreach (string address in GoodAddress)
            {
                if (!CoinCurrency.IsValidAddress(address))
                {
                    Assert.IsTrue(false);
                }
            }

            foreach (string address in BadAddress)
            {
                if (CoinCurrency.IsValidAddress(address))
                {
                    Assert.IsTrue(false);
                }
            }
        }

        [TestMethod]
        public void CheckDashAddress()
        {
            var CoinCurrency = Crypto.Currencies.Controls.CurrencyControl.GetCurrencyControl().GetCryptoCurrency(10003, "");

            List<string> GoodAddress = new List<string>();
            GoodAddress.Add("yfyzM58VsmjfbXtTzNrYH14TmJvY3Nn3Ms");
            GoodAddress.Add("ygvfnN78GrFFtGcp8ALxBz7jEXVRn5Z4gz");

            List<string> BadAddress = new List<string>();
            BadAddress.Add("QY48Q45ucnW8bxiZPui5QBYMEcWzXu8fFA");//Litecoin Testnet
            BadAddress.Add("QUqGzoo5oCEkLu1WbB8npRNdhdFWe9jnmD");
            BadAddress.Add("n3gPfqW6u3AmNKDcW6hjV8VcwyPWH3JXMV"); //Bitcoin Testnet
            BadAddress.Add("MPctwaAHZ3LSA8QTYpEZAGZ9u5jbTwJYQA"); //Litecoin MainNet

            foreach (string address in GoodAddress)
            {
                if (!CoinCurrency.IsValidAddress(address))
                {
                    Assert.IsTrue(false);
                }
            }

            foreach (string address in BadAddress)
            {
                if (CoinCurrency.IsValidAddress(address))
                {
                    Assert.IsTrue(false);
                }
            }
        }

        [TestMethod]
        public void CheckDogecoinAddress()
        {
            var CoinCurrency = Crypto.Currencies.Controls.CurrencyControl.GetCurrencyControl().GetCryptoCurrency(10004, "");

            List<string> GoodAddress = new List<string>();
            GoodAddress.Add("niowepDK9aoo7rqU71Zdqe3vgZe2HHQM2N");

            List<string> BadAddress = new List<string>();
            BadAddress.Add("QY48Q45ucnW8bxiZPui5QBYMEcWzXu8fFA");//Litecoin Testnet
            BadAddress.Add("QUqGzoo5oCEkLu1WbB8npRNdhdFWe9jnmD");
            BadAddress.Add("n3gPfqW6u3AmNKDcW6hjV8VcwyPWH3JXMV"); //Bitcoin Testnet
            BadAddress.Add("MPctwaAHZ3LSA8QTYpEZAGZ9u5jbTwJYQA"); //Litecoin MainNet
            BadAddress.Add("yfyzM58VsmjfbXtTzNrYH14TmJvY3Nn3Ms");
            BadAddress.Add("ygvfnN78GrFFtGcp8ALxBz7jEXVRn5Z4gz");

            foreach (string address in GoodAddress)
            {
                if (!CoinCurrency.IsValidAddress(address))
                {
                    Assert.IsTrue(false);
                }
            }

            foreach (string address in BadAddress)
            {
                if (CoinCurrency.IsValidAddress(address))
                {
                    Assert.IsTrue(false);
                }
            }
        }
    }
}