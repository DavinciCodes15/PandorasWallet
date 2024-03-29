﻿using Nethereum.Signer;
using Nethereum.Web3;
using Org.BouncyCastle.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using BigInteger = System.Numerics.BigInteger;

namespace Pandora.Client.Crypto.Currencies.Ethereum
{
    public class EthereumCurrencyAdvocacy : ICryptoCurrencyAdvocacy
    {
        private readonly Func<string> FGetRootSeed;
        private IEthereumChainParams FChainParams;
        private Dictionary<string, long> FAddresses = new Dictionary<string, long>();

        public static void Register()
        {
            CurrencyControl.GetCurrencyControl().AddCurrencyAdvocacy(EthGetCurrencyAdvocacy);
        }

        private static ICryptoCurrencyAdvocacy EthGetCurrencyAdvocacy(long aCurrencyId, ChainParams aChainParams, Func<string> aGetRootSeed)
        {
            if (aChainParams.Capabilities.HasFlag(CapablityFlags.EthereumProtocol))
                return new EthereumCurrencyAdvocacy(aCurrencyId, new EthChainParams(aChainParams), aGetRootSeed);
            else
                return null;
        }

        public EthereumCurrencyAdvocacy(long aId, IEthereumChainParams aChainparams, Func<string> aGetRootSeed)
        {
            Id = aId;
            ForkFromId = aChainparams.ForkFromId;
            TestNet = aChainparams.Network == ChainParams.NetworkType.TestNet;
            FChainParams = aChainparams;
            if (aGetRootSeed == null) throw new ArgumentNullException("aGetRootSeed function must exist.");
            FGetRootSeed = aGetRootSeed;
        }

        public bool TestNet { get; protected set; }

        public virtual string Name => FChainParams.NetworkName;

        public virtual long Id { get; private set; }

        public virtual long ForkFromId { get; protected set; }

        public string GetRootSeed()
        {
            return FGetRootSeed();
        }

        protected virtual string GetIndexKey(long aIndex)
        {
            string lRootSeed = GetRootSeed();
            if (lRootSeed.Length != 64)
                throw new Exception("RootKey must be 64 Characters long");

            string lKey = lRootSeed;
            string lData = string.Format("{0}FF{1}", aIndex.ToString("X"), Id.ToString("X"));
            lKey = lRootSeed.Substring(0, 64 - lData.Length) + lData;
            return lKey;
        }

        public CCKey GetCCKey(long aIndex)
        {
            CCKey lKey;
            if (aIndex == 0)
                lKey = new CCKey(Encoding.ASCII.GetBytes(GetIndexKey(aIndex)));
            else
                lKey = new CCKey(HexStringToByteArray(GetIndexKey(aIndex)));
            return lKey;
        }

        public virtual string GetAddress(long aIndex)
        {
            var lKey = new Nethereum.Signer.EthECKey(GetIndexKey(aIndex)); // may need to fix this to support the new keygen
            var lWallet = new Nethereum.HdWallet.Wallet(HexStringToByteArray(GetIndexKey(0)));
            var lAddress = lWallet.GetAccount((int)aIndex).Address; // do not lower case as it has checksum
            if (!FAddresses.ContainsKey(lAddress.ToLower())) // lowercase here for look up
                FAddresses.Add(lAddress.ToLower(), aIndex);
            return lAddress;
        }

        public virtual string GetPrivateKey(long aIndex)
        {
            var lBuff = GetBinaryPrivateKey(aIndex);

            return ByteArrayToString(lBuff);      // not sure if this is the right output text
        }

        private byte[] GetBinaryPrivateKey(long aIndex)
        {
            var lWallet = new Nethereum.HdWallet.Wallet(HexStringToByteArray(GetIndexKey(0)));
            return lWallet.GetPrivateKey((int)aIndex);
        }

        public static string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-", "");
        }

        public static byte[] HexStringToByteArray(string aHex)
        {
            if (aHex.Length % 2 != 0) throw new ArgumentException("Hex string must be divisable by 2.");
            byte[] lArray = new byte[aHex.Length / 2];
            for (int i = 0; i < aHex.Length; i += 2)
                lArray[i / 2] = Byte.Parse(aHex.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
            return lArray;
        }

        private int GetChainId(long aCurrencyId)
        {
            var lResult = -1;

            switch (aCurrencyId)
            {
                case 10194: //Ethereum Main
                    lResult = 1;
                    break;

                case 10196: //Ethereum Ropsten
                    lResult = 3;
                    break;

                case 10198: //Binance Testnet
                    lResult = 97;
                    break;

                case 10200: //Binance Main
                    lResult = 56;
                    break;

                case 10202: //Ethereum Goerli
                    lResult = 5;
                    break;
            }

            return lResult;
        }

        public virtual string SignTransaction(string aTxData, ICurrencyTransaction aValidationInfo)
        {
            if (aValidationInfo.Outputs.Length > 2)
                throw new ArgumentException("Invalid output.");
            var lTxOutput = aValidationInfo.Outputs.First();
            var lFromAddress = aValidationInfo.Inputs[0].Address.ToLower();
            var lToAddress = lTxOutput.Address;
            var lKeyIndex = FAddresses[lFromAddress];
            var lAmount = lTxOutput.Amount;
            var lTokenData = !string.IsNullOrEmpty(lTxOutput.Script) ? lTxOutput.Script : null;
            //var lNonceStr = Encoding.Default.GetString(HexStringToByteArray(aTxData)); //I removed this line because biginteger already accepts hex
            string lCleanHexNonce = string.Concat("0", aTxData.Replace("0x", string.Empty));
            var lNonce = BigInteger.Parse(lCleanHexNonce, System.Globalization.NumberStyles.HexNumber);
            var lGasLimit = lTokenData == null ? 21000 : 120000; // number of gass units you can use
            BigInteger lGasPrice = aValidationInfo.TxFee / lGasLimit;
            var lChainID = GetChainId(Id); //This may be moved as part of the chain params
            var lResult = (new LegacyTransactionSigner()).SignTransaction(GetBinaryPrivateKey((int)lKeyIndex), lChainID, lToAddress, lAmount, lNonce, lGasPrice, lGasLimit, lTokenData);
            return lResult;
        }

        public virtual bool IsValidAddress(string aAddress)
        {
            var lEthRegex = new Regex(@"^(0x)?[0-9a-fA-F]{40}$");
            return lEthRegex.IsMatch(aAddress);
        }

        public virtual string BinaryAddress(string aAddress)
        {
            return null;
        }

        public string SignMessage(string aMessage, string aPublicAddress)
        {
            if (!FAddresses.Any())
            {
                GetAddress(0);
                GetAddress(1);
            }
            if (!FAddresses.TryGetValue(aPublicAddress.ToLowerInvariant(), out long lIndex))
                throw new ArgumentException("No private key found for provided address");
            var lPrivateKey = GetPrivateKey(lIndex);
            var lSigner = new EthereumMessageSigner();
            return lSigner.EncodeUTF8AndSign(aMessage, new EthECKey(lPrivateKey));
        }

        public bool VerifyMessage(string aMessage, string aSignature, out string aAddress)
        {
            bool lResult = false;
            if (!FAddresses.Any())
            {
                GetAddress(0);
                GetAddress(1);
            }
            string lOutputAddress;
            try
            {
                var lSigner = new EthereumMessageSigner();
                lOutputAddress = lSigner.EncodeUTF8AndEcRecover(aMessage, aSignature);
            }
            catch
            {
                lOutputAddress = null;
            }
            if (lOutputAddress != null)
                lResult = FAddresses.Keys.Any(lAddress => string.Equals(lAddress.ToLowerInvariant(), lOutputAddress.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase));
            aAddress = lOutputAddress;
            return lResult;
        }
    }
}