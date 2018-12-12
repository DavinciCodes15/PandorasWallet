using Pandora.Client.Crypto.Currencies;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Crypto.Protocol
{
    public class VarString : ICoinSerializable
    {
        public VarString()
        {
            _Bytes = new byte[0];
        }

        private byte[] _Bytes;

        public int Length
        {
            get
            {
                return _Bytes.Length;
            }
        }

        public VarString(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            _Bytes = bytes;
        }

        public byte[] GetString()
        {
            return GetString(false);
        }

        public byte[] GetString(bool @unsafe)
        {
            if (@unsafe)
                return _Bytes;
            return _Bytes.ToArray();
        }

        #region ICoinSerializable Members

        public void ReadWrite(Currencies.CoinStream stream)
        {
            var len = new VarInt((ulong)_Bytes.Length);
            stream.ReadWrite(ref len);
            if (!stream.Serializing)
            {
                if (len.ToLong() > (uint)stream.MaxArraySize)
                    throw new ArgumentOutOfRangeException("Array size not big");
                _Bytes = new byte[len.ToLong()];
            }
            stream.ReadWrite(ref _Bytes);
        }

        internal static void StaticWrite(CoinStream bs, byte[] bytes)
        {
            var len = bytes == null ? 0 : (ulong)bytes.Length;
            if (len > (uint)bs.MaxArraySize)
                throw new ArgumentOutOfRangeException("Array size too big");
            VarInt.StaticWrite(bs, len);
            if (bytes != null)
                bs.ReadWrite(ref bytes);
        }

        internal static void StaticRead(CoinStream bs, ref byte[] bytes)
        {
            var len = VarInt.StaticRead(bs);
            if (len > (uint)bs.MaxArraySize)
                throw new ArgumentOutOfRangeException("Array size too big");
            bytes = new byte[len];
            bs.ReadWrite(ref bytes);
        }

        #endregion ICoinSerializable Members
    }
}