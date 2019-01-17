using Pandora.Client.Crypto.Currencies.DataEncoders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bech32Type = System.Int32;

namespace Pandora.Client.Crypto.Currencies
{
    public interface IBech32Data : ICryptoCurrencyString
    {
        Bech32Type Type
        {
            get;
        }
    }

    public class CoinWitPubKeyAddress : CoinAddress, IBech32Data
	{
        private const int EncoderType = Network.BEACH32_WITNESS_PUBKEY_ADDRESS;
		public CoinWitPubKeyAddress(string bech32, Network expectedNetwork = null)
				: base(Validate(bech32, ref expectedNetwork), expectedNetwork)
		{
            this.Type = EncoderType;
            var encoder = expectedNetwork.GetBech32Encoder(EncoderType, true);
			byte witVersion;
			var decoded = encoder.Decode(bech32, out witVersion);
			_Hash = new WitKeyId(decoded);
		}

		private static string Validate(string bech32, ref Network expectedNetwork)
		{
			if(bech32 == null)
				throw new ArgumentNullException("bech32");
            var networks = expectedNetwork == null ? expectedNetwork.Networks : new[] { expectedNetwork };
			foreach(var network in networks)
			{
                var encoder = expectedNetwork.GetBech32Encoder(EncoderType, false);
				if(encoder == null)
					continue;
				try
				{
					byte witVersion;
					var data = encoder.Decode(bech32, out witVersion);
					if(data.Length == 20 && witVersion == 0)
					{
						return bech32;
					}
				}
				catch(Bech32FormatException) { throw; }
				catch(FormatException) { continue; }
			}
			throw new FormatException("Invalid CoinWitPubKeyAddress");
		}

		public CoinWitPubKeyAddress(WitKeyId segwitKeyId, Network network) :
            base(NotNull(segwitKeyId) ?? Network.CreateBech32(EncoderType, segwitKeyId.ToBytes(), 0, network), network)
		{
            Type = EncoderType;
			_Hash = segwitKeyId;
		}

		private static string NotNull(WitKeyId segwitKeyId)
		{
			if(segwitKeyId == null)
				throw new ArgumentNullException("segwitKeyId");
			return null;
		}

		WitKeyId _Hash;
		public WitKeyId Hash
		{
			get
			{
				return _Hash;
			}
		}


		protected override Script GeneratePaymentScript()
		{
			return PayToWitTemplate.Instance.GenerateScriptPubKey(OpcodeType.OP_0, Hash._DestBytes);
		}

        //public Bech32Type Type
        //{
        //    get
        //    {
        //        return Network.BEACH32_WITNESS_PUBKEY_ADDRESS;
        //    }
        //}
	}

    public class CoinWitScriptAddress : CoinAddress, IBech32Data
	{
        private const int EncoderType = Network.BEACH32_WITNESS_SCRIPT_ADDRESS;
        public CoinWitScriptAddress(string bech32, Network expectedNetwork = null)
				: base(Validate(bech32, ref expectedNetwork), expectedNetwork)
		{
            Type = EncoderType;
            var encoder = expectedNetwork.GetBech32Encoder(EncoderType, true);
			byte witVersion;
			var decoded = encoder.Decode(bech32, out witVersion);
			_Hash = new WitScriptId(decoded);
		}

		private static string Validate(string bech32, ref Network expectedNetwork)
		{
			if(bech32 == null)
				throw new ArgumentNullException("bech32");
            var networks = expectedNetwork == null ? expectedNetwork.Networks : new[] { expectedNetwork };
			foreach(var network in networks)
			{
                var encoder = expectedNetwork.GetBech32Encoder(EncoderType, false);
				if(encoder == null)
					continue;
				try
				{
					byte witVersion;
					var data = encoder.Decode(bech32, out witVersion);
					if(data.Length == 32 && witVersion == 0)
					{
						return bech32;
					}
				}
				catch(Bech32FormatException) { throw; }
				catch(FormatException) { continue; }
			}
			throw new FormatException("Invalid CoinWitScriptAddress");
		}

		public CoinWitScriptAddress(WitScriptId segwitScriptId, Network network)
            : base(NotNull(segwitScriptId) ?? Network.CreateBech32(EncoderType, segwitScriptId.ToBytes(), 0, network), network)
		{
			_Hash = segwitScriptId;
		}


		private static string NotNull(WitScriptId segwitScriptId)
		{
			if(segwitScriptId == null)
				throw new ArgumentNullException("segwitScriptId");
			return null;
		}

		WitScriptId _Hash;
		public WitScriptId Hash
		{
			get
			{
				return _Hash;
			}
		}		

		protected override Script GeneratePaymentScript()
		{
			return PayToWitTemplate.Instance.GenerateScriptPubKey(OpcodeType.OP_0, Hash._DestBytes);
		}

        //public Bech32Type Type
        //{
        //    get
        //    {
        //        return Bech32Type.WITNESS_SCRIPT_ADDRESS;
        //    }
        //}
	}
}
