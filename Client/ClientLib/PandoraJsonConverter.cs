using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        protected Dictionary<Type, Func<JObject, JsonSerializer, object>> FTypes;

        public PandoraJsonConverter()
        {
            CreateConvertionEspecifications();
        }

        public virtual void CreateConvertionEspecifications()
        {
            FTypes = new Dictionary<Type, Func<JObject, JsonSerializer, object>>
            {
                    {
                    typeof(CurrencyItem), (JObject aItem, JsonSerializer aSerializer)=>
                    {
                        return new CurrencyItem(
                            aItem["Id"].Value<long>(),aItem["Name"].Value<string>(),
                            aItem["Ticker"].Value<string>(),aItem["Precision"].Value<ushort>(),
                            aItem["LiveDate"].Value<DateTime>(),aItem["MinConfirmations"].Value<int>(),
                            GetIcon(aItem,aSerializer), aItem["FeePerKb"].Value<int>(),
                            aItem["ChainParamaters"].ToObject<ChainParams>(aSerializer)
                            );
                    }
                },
                    {
                        typeof(ChainParams), (JObject aItem, JsonSerializer aSerializer)=>
                        {
                            return new ChainParams()
                            {
                                NetworkName = aItem["NetworkName"].Value<string>(),
                                Network = (ChainParams.NetworkType) aItem["Network"].Value<int>(),
                                PublicKeyAddress =  aItem["PublicKeyAddress"].ToObject<byte[]>(aSerializer),
                                ScriptAddress =  aItem["ScriptAddress"].ToObject<byte[]>(aSerializer),
                                SecretKey =  aItem["SecretKey"].ToObject<byte[]>(aSerializer),
                                ExtPublicKey =  aItem["ExtPublicKey"].ToObject<byte[]>(aSerializer),
                                ExtSecretKey =  aItem["ExtSecretKey"].ToObject<byte[]>(aSerializer),
                                ForkFromId =  aItem["ForkFromId"].Value<long>(),
                                EncryptedSecretKeyNoEc = aItem["EncryptedSecretKeyNoEc"].ToObject<byte[]>(aSerializer),
                                EncryptedSecretKeyEc =  aItem["EncryptedSecretKeyEc"].ToObject<byte[]>(aSerializer),
                                PasspraseCode =  aItem["PasspraseCode"].ToObject<byte[]>(aSerializer),
                                ConfirmationCode =  aItem["ConfirmationCode"].ToObject<byte[]>(aSerializer),
                                StealthAddress =  aItem["StealthAddress"].ToObject<byte[]>(aSerializer),
                                AssetId =  aItem["AssetId"].ToObject<byte[]>(aSerializer),
                                ColoredAddress =  aItem["ColoredAddress"].ToObject<byte[]>(aSerializer),
                                Encoder = aItem["Encoder"].Value<string>()
                            };
                        }
                    },
                    { typeof(CurrencyStatusItem), (JObject aItem, JsonSerializer aSerializer) => { return new CurrencyStatusItem(aItem["StatusId"].Value<long>(),aItem["CurrencyId"].Value<uint>(),aItem["StatusTime"].Value<DateTime>(),(CurrencyStatus) Enum.Parse(typeof(CurrencyStatus),aItem["Status"].Value<string>()),aItem["ExtendedInfo"].Value<string>(), aItem["BlockHeight"].Value<ulong>()); } },
                    { typeof(CurrencyAccount),  (JObject aItem, JsonSerializer aSerializer) => { return new CurrencyAccount(aItem["Id"].Value<uint>(),aItem["CurrencyId"].Value<uint>(),aItem["Address"].Value<string>()); } },
                    { typeof(TransactionUnit), (JObject aItem, JsonSerializer aSerializer) => { return new TransactionUnit(aItem["Id"].Value<ulong>(), aItem["Amount"].Value<ulong>(),aItem["Address"].Value<string>()); } },
                    { typeof(TransactionRecord),  (JObject aItem, JsonSerializer aSerializer) => { TransactionRecord lTxRcd =  new TransactionRecord(aItem["TransactionRecordId"].Value<ulong>(),aItem["TxId"].Value<string>(),aItem["TxDate"].Value<DateTime>(),aItem["Block"].Value<ulong>()); lTxRcd.AddInput(aItem["Inputs"].ToObject<TransactionUnit[]>(aSerializer)); lTxRcd.AddOutput(aItem["Outputs"].ToObject<TransactionUnit[]>(aSerializer)); lTxRcd.TxFee = aItem["TxFee"].Value<ulong>(); return lTxRcd;  }  },
                    { typeof(CurrencyTransaction), (JObject aItem,JsonSerializer aSerializer) => { return new CurrencyTransaction(aItem["Inputs"].ToObject<TransactionUnit[]>(aSerializer), aItem["Outputs"].ToObject<TransactionUnit[]>(aSerializer),aItem["TxFee"].Value<ulong>(),aItem["CurrencyId"].Value<ulong>()); } },
                    { typeof(UserStatus), (JObject aItem,JsonSerializer aSerializer) => { return new UserStatus(aItem["Active"].Value<bool>(),aItem["ExtendedInfo"].Value<string>(),aItem["StatusDate"].Value<DateTime>()); } }
            };
        }

        public override bool CanConvert(Type objectType)
        {
            return FTypes.Keys.Any(t => t == objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject item = JObject.Load(reader);

            if (!CanConvert(objectType))
            {
                throw new JsonException("Cannot convert object type provided");
            }

            return FTypes[objectType](item, serializer);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecesary because we only want to read");
        }

        public override bool CanWrite => false;
    }
}