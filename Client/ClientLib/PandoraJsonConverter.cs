//   Copyright 2017-2019 Davinci Codes
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Also use the software for non-commercial purposes.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pandora.Client.ClientLib.Contracts;
using Pandora.Client.Crypto.Currencies;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pandora.Client.ClientLib
{
    public class PandoraJsonConverter : JsonConverter
    {
        protected virtual byte[] GetIcon(JObject aItem, JsonSerializer aSerializer)
        {
            return aItem["Icon"].ToObject<byte[]>(aSerializer);
        }

        public PandoraJsonConverter()
        {
        }

        private object InternalConvert(JsonSerializer aSerializer, JObject aItem, Type aType)
        {
            object lResult;
            if (aType.IsAssignableFrom(typeof(ICurrencyItem)))
                lResult = new CurrencyItem
                {
                    Id = aItem["Id"].Value<long>(),
                    Name = aItem["Name"].Value<string>(),
                    Ticker = aItem["Ticker"].Value<string>(),
                    Precision = aItem["Precision"].Value<ushort>(),
                    LiveDate = aItem["LiveDate"].Value<DateTime>(),
                    MinConfirmations = aItem["MinConfirmations"].Value<int>(),
                    Icon = GetIcon(aItem, aSerializer),
                    FeePerKb = aItem["FeePerKb"].Value<long>(),
                    ChainParamaters = aItem["ChainParamaters"].ToObject<ChainParams>(aSerializer),
                    CurrentStatus = (CurrencyStatus) aItem["CurrentStatus"].Value<int>()
                };
            else if (aType.Equals(typeof(ChainParams)))
                lResult = new ChainParams()
                {
                    NetworkName = aItem["NetworkName"].Value<string>(),
                    Network = (ChainParams.NetworkType) aItem["Network"].Value<int>(),
                    PublicKeyAddress = aItem["PublicKeyAddress"].ToObject<byte[]>(aSerializer),
                    ScriptAddress = aItem["ScriptAddress"].ToObject<byte[]>(aSerializer),
                    SecretKey = aItem["SecretKey"].ToObject<byte[]>(aSerializer),
                    ExtPublicKey = aItem["ExtPublicKey"].ToObject<byte[]>(aSerializer),
                    ExtSecretKey = aItem["ExtSecretKey"].ToObject<byte[]>(aSerializer),
                    ForkFromId = aItem["ForkFromId"].Value<long>(),
                    EncryptedSecretKeyNoEc = aItem["EncryptedSecretKeyNoEc"].ToObject<byte[]>(aSerializer),
                    EncryptedSecretKeyEc = aItem["EncryptedSecretKeyEc"].ToObject<byte[]>(aSerializer),
                    PasspraseCode = aItem["PasspraseCode"].ToObject<byte[]>(aSerializer),
                    ConfirmationCode = aItem["ConfirmationCode"].ToObject<byte[]>(aSerializer),
                    StealthAddress = aItem["StealthAddress"].ToObject<byte[]>(aSerializer),
                    AssetId = aItem["AssetId"].ToObject<byte[]>(aSerializer),
                    Capabilities = (CapablityFlags) aItem["Capabilities"].Value<int>(),
                    ColoredAddress = aItem["ColoredAddress"].ToObject<byte[]>(aSerializer),
                    Encoder = aItem["Encoder"].Value<string>(),
                    Version = aItem["Version"].Value<long>()
                };
            else if (aType.Equals(typeof(CurrencyStatusItem)))
                lResult = new CurrencyStatusItem(aItem["StatusId"].Value<long>(),
                                                          aItem["CurrencyId"].Value<long>(),
                                                          aItem["StatusTime"].Value<DateTime>(),
                                                          (CurrencyStatus) Enum.Parse(typeof(CurrencyStatus),
                                                          aItem["Status"].Value<string>()),
                                                          aItem["ExtendedInfo"].Value<string>(),
                                                          aItem["BlockHeight"].Value<long>());
            else if (aType.Equals(typeof(CurrencyAccount)))
                lResult = new CurrencyAccount(aItem["Id"].Value<long>(), aItem["CurrencyId"].Value<long>(), aItem["Address"].Value<string>());
            else if (aType.Equals(typeof(TransactionUnit)))
                lResult = new TransactionUnit(aItem["Id"].Value<long>(), aItem["Amount"].Value<long>(), aItem["Address"].Value<string>(), aItem.TryGetValue("Index", out JToken lIndexObject) ? lIndexObject.Value<int>() : -1, aScript: aItem["Script"].Value<string>());
            else if (aType.Equals(typeof(TransactionRecord)))
            {
                TransactionRecord lTxRcd = new TransactionRecord(
                    aItem["TransactionRecordId"].Value<long>(),
                    aItem["CurrencyId"].Value<long>(),
                    aItem["TxId"].Value<string>(),
                    aItem["TxDate"].Value<DateTime>(),
                    aItem["Block"].Value<long>(),
                    aItem["Valid"].Value<bool>()
                    );
                lTxRcd.AddInput(aItem["Inputs"].ToObject<TransactionUnit[]>(aSerializer));
                lTxRcd.AddOutput(aItem["Outputs"].ToObject<TransactionUnit[]>(aSerializer));
                lTxRcd.TxFee = aItem["TxFee"].Value<long>();
                lResult = lTxRcd;
            }
            else if (aType.Equals(typeof(CurrencyTransaction)))
                lResult = new CurrencyTransaction(aItem["Inputs"].ToObject<TransactionUnit[]>(aSerializer), aItem["Outputs"].ToObject<TransactionUnit[]>(aSerializer), aItem["TxFee"].Value<long>(), aItem["CurrencyId"].Value<long>());
            else if (aType.Equals(typeof(UserStatus)))
                lResult = new UserStatus(aItem["Active"].Value<bool>(), aItem["ExtendedInfo"].Value<string>(), aItem["StatusDate"].Value<DateTime>());
            else lResult = aItem.ToObject(aType, aSerializer);
            return lResult;
        }

        public override bool CanConvert(Type aObjectType)
        {
            return aObjectType.IsAssignableFrom(typeof(ICurrencyItem)) ||
                aObjectType.IsAssignableFrom(typeof(ChainParams)) ||
                aObjectType.IsAssignableFrom(typeof(CurrencyStatusItem)) ||
               aObjectType.IsAssignableFrom(typeof(CurrencyAccount)) ||
               aObjectType.IsAssignableFrom(typeof(TransactionUnit)) ||
               aObjectType.IsAssignableFrom(typeof(TransactionRecord)) ||
               aObjectType.IsAssignableFrom(typeof(CurrencyTransaction)) ||
               aObjectType.IsAssignableFrom(typeof(UserStatus));
        }

        public override object ReadJson(JsonReader aReader, Type aObjectType, object aExistingValue, JsonSerializer aSerializer)
        {
            if (!CanConvert(aObjectType)) throw new InvalidOperationException($"Unable to convert JSON Object to type {aObjectType.Name}");
            JObject lItem = JObject.Load(aReader);
            return InternalConvert(aSerializer, lItem, aObjectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecesary because we only want to read");
        }

        public override bool CanWrite => false;
    }
}