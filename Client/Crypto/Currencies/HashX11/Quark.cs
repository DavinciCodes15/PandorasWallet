using System.Collections.Generic;
using System.Linq;

namespace Pandora.Client.Crypto.Currencies.HashX11
{
    public class Quark
    {
        private enum HashEnum
        {
            Blake, Bmw, Groestl, skein, jh, keccak
        }

        private Dictionary<HashEnum, IHash> _hashers;

        public Quark()
        {
            IHash blake512 = HashFactory.Crypto.SHA3.CreateBlake512();
            IHash bmw512 = HashFactory.Crypto.SHA3.CreateBlueMidnightWish512();
            IHash skein512 = HashFactory.Crypto.SHA3.CreateSkein512_Custom();
            IHash jh512 = HashFactory.Crypto.SHA3.CreateJH512();
            IHash keccak512 = HashFactory.Crypto.SHA3.CreateKeccak512();
            IHash groestl512 = HashFactory.Crypto.SHA3.CreateGroestl512();

            _hashers = new Dictionary<HashEnum, IHash>()
            {
                {HashEnum.Blake, blake512 },
                {HashEnum.Bmw, bmw512 },
                { HashEnum.Groestl, groestl512 },
                {HashEnum.skein, skein512 },
                {HashEnum.jh, jh512 },
                {HashEnum.keccak, keccak512 }
            };
        }

        public byte[] ComputeBytes(byte[] input)
        {
            byte[] lHashResult = input;

            uint512 lZero = new uint512();
            uint512 lMask = new uint512(8);
            uint512 lResult;

            lHashResult = _hashers[HashEnum.Blake].ComputeBytes(lHashResult).GetBytes();
            lHashResult = _hashers[HashEnum.Bmw].ComputeBytes(lHashResult).GetBytes();

            lResult = new uint512(lHashResult);

            if ((lResult & lMask) != lZero)
            {
                lHashResult = _hashers[HashEnum.Groestl].ComputeBytes(lHashResult).GetBytes();
            }
            else
            {
                lHashResult = _hashers[HashEnum.skein].ComputeBytes(lHashResult).GetBytes();
            }

            lHashResult = _hashers[HashEnum.Groestl].ComputeBytes(lHashResult).GetBytes();

            lHashResult = _hashers[HashEnum.jh].ComputeBytes(lHashResult).GetBytes();

            lResult = new uint512(lHashResult);

            if ((lResult & lMask) != lZero)
            {
                lHashResult = _hashers[HashEnum.Blake].ComputeBytes(lHashResult).GetBytes();
            }
            else
            {
                lHashResult = _hashers[HashEnum.Bmw].ComputeBytes(lHashResult).GetBytes();
            }

            lHashResult = _hashers[HashEnum.keccak].ComputeBytes(lHashResult).GetBytes();
            lHashResult = _hashers[HashEnum.skein].ComputeBytes(lHashResult).GetBytes();

            lResult = new uint512(lHashResult);

            if ((lResult & lMask) != lZero)
            {
                lHashResult = _hashers[HashEnum.keccak].ComputeBytes(lHashResult).GetBytes();
            }
            else
            {
                lHashResult = _hashers[HashEnum.jh].ComputeBytes(lHashResult).GetBytes();
            }

            return lHashResult.Take(32).ToArray();
        }
    }
}