﻿using Pandora.Client.Exchange.SaveManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Exchange
{
    public interface IExchangeKeyValueHandled
    {
        void WriteKeyValue(string aKey, string aValue, int aProfileID);

        void WriteKeyValues(Dictionary<string, string> aKeyValues, int aProfileID);

        string ReadKeyValue(string aKey, int aProfileID);
    }

    public interface IExchangeKeyValueObject
    {
        int ProfileID { get; set; }
    }

    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ExchangeKeyName : System.Attribute
    {
        public string Name { get; private set; }

        /// <summary>
        /// They key name entered do not distinguish between uppercase and lowercase
        /// </summary>
        /// <param name="aName"></param>
        public ExchangeKeyName(string aName)
        {
            Name = aName;
        }
    }

    public class ExchangeKeyValueHelper<T> where T : IExchangeKeyValueObject, new()
    {
        private IExchangeKeyValueHandled FSaveManager;
        private Dictionary<ExchangeKeyName, PropertyInfo> FKeyNameProperties;

        public ExchangeKeyValueHelper(IExchangeKeyValueHandled aSaveManager)
        {
            FKeyNameProperties = new Dictionary<ExchangeKeyName, PropertyInfo>();
            var lTypeProperties = typeof(T).GetProperties();
            foreach (var lProperty in lTypeProperties)
            {
                var lCustomAttr = lProperty.GetCustomAttributes(true);
                foreach (var lAttr in lCustomAttr)
                {
                    var lExchangeKeyName = lAttr as ExchangeKeyName;
                    if (lExchangeKeyName != null)
                        FKeyNameProperties.Add(lExchangeKeyName, lProperty);
                }
            }
            FSaveManager = aSaveManager;
        }

        public void SaveChanges(T aObjectToSave)
        {
            Dictionary<string, string> lDicToSave = new Dictionary<string, string>();
            foreach (var lTypeProperty in FKeyNameProperties)
                lDicToSave.Add(lTypeProperty.Key.Name.ToLower(), Convert.ToString(lTypeProperty.Value.GetValue(aObjectToSave) ?? string.Empty));
            FSaveManager.WriteKeyValues(lDicToSave, aObjectToSave.ProfileID);
        }

        public T LoadKeyValues(int aProfileID)
        {
            var lResult = new T();
            foreach (var lTypeProperty in FKeyNameProperties)
            {
                string lValue = FSaveManager.ReadKeyValue(lTypeProperty.Key.Name.ToLower(), aProfileID);
                lTypeProperty.Value.SetValue(lResult, lValue);
            }
            lResult.ProfileID = aProfileID;
            return lResult;
        }

        public void WriteKey(string aKey, string aValue, int aProfileID)
        {
            var lValidAttributes = FKeyNameProperties.Keys.Select(lKey => lKey.Name.ToLower());
            if (!lValidAttributes.Contains(aKey.ToLower())) throw new Exception("KeyValueHelper: Key not found inside declared object class");
            FSaveManager.WriteKeyValue(aKey.ToLower(), aValue, aProfileID);
        }
    }

    public class ExchangeKeyValueObject : IExchangeKeyValueObject
    {
        public int ProfileID { get; set; }

        [ExchangeKeyName("EXCHANGE_PUBLIC")]
        public string PublicKey { get; set; }

        [ExchangeKeyName("EXCHANGE_PRIVATE")]
        public string PrivateKey { get; set; }
    }
}