using Pandora.Client.Crypto.Currencies.DataEncoders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Currencies
{
    public interface ISecret
    {
        CCKey PrivateKey
        {
            get;
        }
    }
    
    public class CoinSecret : Base58Data, IDestination, ISecret
	{
		public CoinSecret(CCKey key, Network network)
			: base(ToBytes(key), network)
		{
		}

		public CoinSecret(string base58, Network expectedAddress = null)
			: base(base58, expectedAddress)
		{
		}

		private CoinPubKeyAddress _address;

		public CoinPubKeyAddress GetAddress()
		{
			return _address ?? (_address = PrivateKey.PubKey.GetAddress(Network));
		}

		public virtual KeyId PubKeyHash
		{
			get
			{
				return PrivateKey.PubKey.Hash;
			}
		}

		public PubKey PubKey
		{
			get
			{
				return PrivateKey.PubKey;
			}
		}

		#region ISecret Members
		CCKey _Key;
		public CCKey PrivateKey
		{
			get
			{
				return _Key ?? (_Key = new CCKey(vchData, 32, IsCompressed));
			}
		}
		#endregion

		protected override bool IsValid
		{
			get
			{
				if(vchData.Length != 33 && vchData.Length != 32)
					return false;

				if(vchData.Length == 33 && IsCompressed)
					return true;
				if(vchData.Length == 32 && !IsCompressed)
					return true;
				return false;
			}
		}

		public CoinEncryptedSecret Encrypt(string password)
		{
			return PrivateKey.GetEncryptedCoinSecret(password, Network);
		}


		public CoinSecret Copy(bool? compressed)
		{
			if(compressed == null)
				compressed = IsCompressed;

			if(compressed.Value && IsCompressed)
			{
				return new CoinSecret(wifData, Network);
			}
			else
			{
				byte[] result = Encoders.Base58Check.DecodeData(wifData);
				var resultList = result.ToList();

				if(compressed.Value)
				{
					resultList.Insert(resultList.Count, 0x1);
				}
				else
				{
					resultList.RemoveAt(resultList.Count - 1);
				}
				return new CoinSecret(Encoders.Base58Check.EncodeData(resultList.ToArray()), Network);
			}
		}

		public bool IsCompressed
		{
			get
			{
				return vchData.Length > 32 && vchData[32] == 1;
			}
		}

        private static byte[] ToBytes(CCKey key)
        {
            var keyBytes = key.ToBytes();
            if (!key.IsCompressed)
                return keyBytes;
            else
                return keyBytes.Concat(new byte[] { 0x01 }).ToArray();
        }
        
        public override int Type
		{
			get
			{
				return Network.BASE58_SECRET_KEY;
			}
		}

		#region IDestination Members

		public Script ScriptPubKey
		{
			get
			{
				return GetAddress().ScriptPubKey;
			}
		}

		#endregion


	}
}
