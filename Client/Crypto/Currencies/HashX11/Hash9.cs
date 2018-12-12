using System.Linq;

namespace Pandora.Client.Crypto.Currencies.HashX11
{
    public class Hash9
    {
        private IHash[] _hashers;

        public Hash9()
        {
            IHash blake512 = HashFactory.Crypto.SHA3.CreateBlake512();
            IHash bmw512 = HashFactory.Crypto.SHA3.CreateBlueMidnightWish512();
            IHash groestl512 = HashFactory.Crypto.SHA3.CreateGroestl512();
            IHash skein512 = HashFactory.Crypto.SHA3.CreateSkein512_Custom();
            IHash jh512 = HashFactory.Crypto.SHA3.CreateJH512();
            IHash keccak512 = HashFactory.Crypto.SHA3.CreateKeccak512();
            IHash luffa512 = HashFactory.Crypto.SHA3.CreateLuffa512();
            IHash cubehash512 = HashFactory.Crypto.SHA3.CreateCubeHash512();
            IHash shavite512 = HashFactory.Crypto.SHA3.CreateSHAvite3_512_Custom();
            IHash simd512 = HashFactory.Crypto.SHA3.CreateSIMD512();
            IHash echo512 = HashFactory.Crypto.SHA3.CreateEcho512();
            IHash hamsi512 = HashFactory.Crypto.SHA3.CreateHamsi512();
            IHash fugue512 = HashFactory.Crypto.SHA3.CreateFugue512();

            _hashers = new IHash[]
            {
                blake512, bmw512, groestl512, skein512, jh512, keccak512,
                luffa512, cubehash512, shavite512, simd512, echo512, hamsi512,
                fugue512
            };
        }

        public byte[] ComputeBytes(byte[] input)
        {
            byte[] hashResult = input;
            for (int i = 0; i < _hashers.Length; i++)
            {
                hashResult = _hashers[i].ComputeBytes(hashResult).GetBytes();
            }

            return hashResult.Take(32).ToArray();
        }
    }
}