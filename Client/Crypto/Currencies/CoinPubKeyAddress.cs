using Pandora.Client.Crypto.Currencies.DataEncoders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Currencies
{
	/// <summary>
	/// Base58 representation of a pubkey hash and base class for the representation of a script hash
	/// </summary>
	public class CoinPubKeyAddress : CoinAddress
	{
        private const int Base58Type = Network.BASE58_PUBKEY_ADDRESS;
        public CoinPubKeyAddress(string base58, Network expectedNetwork)
			: base(Validate(base58, ref expectedNetwork), expectedNetwork)
		{
            Type = Base58Type;
			var decoded = Encoders.Base58Check.DecodeData(base58);
            _KeyId = new KeyId(new uint160(decoded.Skip(expectedNetwork.GetVersionBytes(Base58Type, true).Length).ToArray()));
		}

		private static string Validate(string base58, ref Network expectedNetwork)
		{
			if(base58 == null)
				throw new ArgumentNullException("base58");
            var networks = new[] { expectedNetwork };
			var data = Encoders.Base58Check.DecodeData(base58);
			foreach(var network in networks)
			{
                var versionBytes = network.GetVersionBytes(Base58Type, false);
				if(versionBytes != null && data.StartWith(versionBytes))
				{
					if(data.Length == versionBytes.Length + 20)
					{
						expectedNetwork = network;
						return base58;
					}
				}
			}
			throw new FormatException("Invalid CoinPubKeyAddress "+ expectedNetwork.NetworkName);
		}

		

		public CoinPubKeyAddress(KeyId keyId, Network network) :
            base(NotNull(keyId) ?? Network.CreateBase58(Base58Type, keyId.ToBytes(), network), network)
		{
            Type = Base58Type;
			_KeyId = keyId;
		}

		private static string NotNull(KeyId keyId)
		{
			if(keyId == null)
				throw new ArgumentNullException("keyId");
			return null;
		}

		public bool VerifyMessage(string message, string signature)
		{
			var key = PubKey.RecoverFromMessage(message, signature);
			return key.Hash == Hash;
		}

		protected KeyId _KeyId;
		public KeyId Hash
		{
			get
			{
				return _KeyId;
			}
		}



		protected override Script GeneratePaymentScript()
		{
			return PayToPubkeyHashTemplate.Instance.GenerateScriptPubKey((KeyId)this.Hash);
		}
	}
}
