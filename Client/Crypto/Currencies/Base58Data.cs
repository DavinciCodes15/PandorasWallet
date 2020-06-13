using Pandora.Client.Crypto.Currencies.DataEncoders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinaryTextEncoderType = System.Int32;

namespace Pandora.Client.Crypto.Currencies
{

    public interface IBinaryEncodable : ICryptoCurrencyString
	{
		BinaryTextEncoderType Type
		{
			get;
		}
	}

    public abstract class BinaryEncoding : IBinaryEncodable
    {
        public virtual BinaryTextEncoderType Type
        {
            get;
            protected set;
        }

        public abstract Network Network { get; }

    }
	/// <summary>
	/// Base class for all Base58 check representation of data
	/// </summary>
    public abstract class Base58Data : BinaryEncoding, ICryptoCurrencyString
	{
		protected byte[] vchData = new byte[0];
		protected byte[] vchVersion = new byte[0];
		protected string wifData = "";
		private Network _Network;
		public override Network Network
		{
			get
			{
				return _Network;
			}
		}
		protected void Init<T>(string base64, Network expectedNetwork = null) where T : Base58Data
		{
			_Network = expectedNetwork;
			SetString<T>(base64);
		}

     	protected Base58Data(string base64, Network expectedNetwork)
		{
			_Network = expectedNetwork;
			SetString(base64);
		}

		protected Base58Data(byte[] rawBytes, Network network)
		{
			if(network == null)
				throw new ArgumentNullException("network");
			_Network = network;
			SetData(rawBytes);
		}

		private void SetString<T>(string psz) where T : Base58Data
		{
			if (_Network == null)
			{
				_Network = Network.GetNetworkFromBase58Data(psz, Type);
				if (_Network == null)
					throw new FormatException("Invalid " + this.GetType().Name);
			}

			byte[] vchTemp = _Network.NetworkStringParser.GetBase58CheckEncoder().DecodeData(psz);
#if HAS_SPAN
			if (!(_Network.GetVersionMemory(Type, false) is ReadOnlyMemory<byte> expectedVersion))
				throw new FormatException("Invalid " + this.GetType().Name);
#else
			var expectedVersion = _Network.GetVersionBytes(Type, false);
			if (expectedVersion is null)
				throw new FormatException("Invalid " + this.GetType().Name);
#endif

#if HAS_SPAN
			var vchTempMemory = vchTemp.AsMemory();
			vchVersion = vchTempMemory.Slice(0, expectedVersion.Length);
#else
			vchVersion = vchTemp.SafeSubarray(0, expectedVersion.Length);
#endif
#if HAS_SPAN
			if (!vchVersion.Span.SequenceEqual(expectedVersion.Span))
#else
			if (!Utils.ArrayEqual(vchVersion, expectedVersion))
#endif
			{
				if (_Network.NetworkStringParser.TryParse(psz, Network, out T other))
				{
					this.vchVersion = other.vchVersion;
					this.vchData = other.vchData;
					this.wifData = other.wifData;
				}
				else
				{
					throw new FormatException("The version prefix does not match the expected one " + String.Join(",", expectedVersion));
				}
			}
			else
			{
#if HAS_SPAN
				vchData = vchTempMemory.Slice(expectedVersion.Length).ToArray();
#else
				vchData = vchTemp.SafeSubarray(expectedVersion.Length);
#endif
				wifData = psz;
			}

			if (!IsValid)
				throw new FormatException("Invalid " + this.GetType().Name);

		}



		private void SetString(string psz)
		{
			if(_Network == null)
			{
				_Network = Network.GetNetworkFromBase58Data(psz, Type);
				if(_Network == null)
					throw new FormatException("Invalid " + this.GetType().Name);
			}

			byte[] vchTemp = Encoders.Base58Check.DecodeData(psz);
			var expectedVersion = _Network.GetVersionBytes(Type, true);


			vchVersion = vchTemp.SafeSubarray(0, expectedVersion.Length);
			if(!Utils.ArrayEqual(vchVersion, expectedVersion))
				throw new FormatException("The version prefix does not match the expected one " + String.Join(",", expectedVersion));

			vchData = vchTemp.SafeSubarray(expectedVersion.Length);
			wifData = psz;

			if(!IsValid)
				throw new FormatException("Invalid " + this.GetType().Name);

		}


		private void SetData(byte[] vchData)
		{
			this.vchData = vchData;
			this.vchVersion = _Network.GetVersionBytes(Type, true);
			wifData = Encoders.Base58Check.EncodeData(vchVersion.Concat(vchData).ToArray());

			if(!IsValid)
				throw new FormatException("Invalid " + this.GetType().Name);
		}


		protected virtual bool IsValid
		{
			get
			{
				return true;
			}
		}


		public string ToWif()
		{
			return wifData;
		}
		public byte[] ToBytes()
		{
			return vchData.ToArray();
		}
		public override string ToString()
		{
			return wifData;
		}

		public override bool Equals(object obj)
		{
			Base58Data item = obj as Base58Data;
			if(item == null)
				return false;
			return ToString().Equals(item.ToString());
		}
		public static bool operator ==(Base58Data a, Base58Data b)
		{
			if(System.Object.ReferenceEquals(a, b))
				return true;
			if(((object)a == null) || ((object)b == null))
				return false;
			return a.ToString() == b.ToString();
		}

		public static bool operator !=(Base58Data a, Base58Data b)
		{
			return !(a == b);
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}
	}
}
