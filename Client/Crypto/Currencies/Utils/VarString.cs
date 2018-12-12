using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Currencies
{
	public class VarString : ICoinSerializable
	{
		public VarString()
		{

		}
		byte[] _Bytes = new byte[0];
		public int Length
		{
			get
			{
				return _Bytes.Length;
			}
		}
		public VarString(byte[] bytes)
		{
			if(bytes == null)
				throw new ArgumentNullException("bytes");
			_Bytes = bytes;
		}
		public byte[] GetString()
		{
			return GetString(false);
		}
		public byte[] GetString(bool @unsafe)
		{
			if(@unsafe)
				return _Bytes;
			return _Bytes.ToArray();
		}
		#region ICoinSerializable Members

		public void ReadWrite(CoinStream stream)
		{
			var len = new VarInt((ulong)_Bytes.Length);
			stream.ReadWrite(ref len);
			if(!stream.Serializing)
			{
				if(len.ToLong() > (uint)stream.MaxArraySize)
					throw new ArgumentOutOfRangeException("Array size not big");
				_Bytes = new byte[len.ToLong()];
			}
			stream.ReadWrite(ref _Bytes);
		}

		#endregion
	}
}
