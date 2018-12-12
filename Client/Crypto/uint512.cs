using Pandora.Client.Crypto.Currencies;
using System;
using System.Linq;

namespace Pandora.Client.Crypto
{
    public class uint512
    {
        private readonly uint256[] FData = new uint256[2];

        public uint256 FirstPart => FData[0];
        public uint256 SecondPart => FData[1];

        public uint512()
        {
            FData[0] = new uint256();
            FData[1] = new uint256();
        }

        public uint512(ulong aNumber)
        {
            FData[0] = new uint256(aNumber);
            FData[1] = new uint256();
        }

        public uint512(byte[] avch)
        {
            if (avch.Length != 64)
            {
                throw new System.Exception("The byte array should be of 64 bytes");
            }

            FData[0] = new uint256(avch.Take(32).ToArray());
            FData[1] = new uint256(avch.Skip(32).ToArray());
        }

        public static uint512 operator &(uint512 a, uint512 b)
        {
            uint[] lByteFirst = new uint[8];
            uint[] lByteSecond = new uint[8];

            for (short it = 0; it < 8; it++)
            {
                lByteFirst[it] = (uint)a.FirstPart.GetByte(it) & b.FirstPart.GetByte(it);
                lByteSecond[it] = (uint)a.SecondPart.GetByte(it) & b.SecondPart.GetByte(it);
            }

            return new uint512(lByteSecond.Concat(lByteFirst).SelectMany(BitConverter.GetBytes).ToArray());
        }

        public static bool operator ==(uint512 a, uint512 b)
        {
            return (a.FirstPart == b.FirstPart) && (a.SecondPart == b.SecondPart);
        }

        public static bool operator !=(uint512 a, uint512 b)
        {
            return !(a == b);
        }

        public byte[] ToBytes()
        {
            byte[] lp1 = FData[0].ToBytes();
            byte[] lp2 = FData[1].ToBytes();

            return lp2.Concat(lp1);
        }
    }
}