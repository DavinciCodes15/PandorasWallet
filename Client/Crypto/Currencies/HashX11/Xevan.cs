using System.Collections.Generic;
using System.Linq;

namespace Pandora.Client.Crypto.Currencies.HashX11
{
    public class Xevan
    {
        private enum HashEnum
        {
            Blake, Bmw, Groestl, skein, jh, keccak, luffa,
            CubeHash,
            shavite,
            simd,
            echo,
            hamsi,
            fugue,
            shabal,
            whirlpool,
            Haval,
            sha
        }

        private Dictionary<HashEnum, HashLib.IHash> _hashers;

        public Xevan()
        {
            HashLib.IHash blake512 = HashLib.HashFactory.Crypto.SHA3.CreateBlake512();
            HashLib.IHash bmw512 = HashLib.HashFactory.Crypto.SHA3.CreateBlueMidnightWish512();
            HashLib.IHash skein512 = HashLib.HashFactory.Crypto.SHA3.CreateSkein512();
            HashLib.IHash jh512 = HashLib.HashFactory.Crypto.SHA3.CreateJH512();
            HashLib.IHash keccak512 = HashLib.HashFactory.Crypto.SHA3.CreateKeccak512();
            HashLib.IHash groestl512 = HashLib.HashFactory.Crypto.SHA3.CreateGroestl512();
            HashLib.IHash luffa512 = HashLib.HashFactory.Crypto.SHA3.CreateLuffa512();
            HashLib.IHash cubehash512 = HashLib.HashFactory.Crypto.SHA3.CreateCubeHash512();
            HashLib.IHash shavite512 = HashLib.HashFactory.Crypto.SHA3.CreateSHAvite3_512();
            HashLib.IHash simd512 = HashLib.HashFactory.Crypto.SHA3.CreateSIMD512();
            HashLib.IHash echo512 = HashLib.HashFactory.Crypto.SHA3.CreateEcho512();
            HashLib.IHash hamsi512 = HashLib.HashFactory.Crypto.SHA3.CreateHamsi512();
            HashLib.IHash fugue512 = HashLib.HashFactory.Crypto.SHA3.CreateFugue512();
            HashLib.IHash shabal512 = HashLib.HashFactory.Crypto.SHA3.CreateShabal512();
            HashLib.IHash whirpool = HashLib.HashFactory.Crypto.CreateWhirlpool();
            HashLib.IHash sha512 = HashLib.HashFactory.Crypto.BuildIn.CreateSHA512Managed();
            HashLib.IHash haval256 = HashLib.HashFactory.Crypto.CreateHaval_5_256();

            _hashers = new Dictionary<HashEnum, HashLib.IHash>()
            {
                {HashEnum.Blake, blake512 },
                {HashEnum.Bmw, bmw512 },
                { HashEnum.Groestl, groestl512 },
                {HashEnum.skein, skein512 },
                {HashEnum.jh, jh512 },
                {HashEnum.keccak, keccak512 },
                {HashEnum.luffa, luffa512 },
                {HashEnum.CubeHash, cubehash512 },
                {HashEnum.shavite, shavite512 },
                {HashEnum.simd, simd512 },
                {HashEnum.echo, echo512 },
                {HashEnum.hamsi,  hamsi512},
                {HashEnum.fugue, fugue512 },
                {HashEnum.shabal, shabal512 },
                {HashEnum.sha, sha512 },
                {HashEnum.whirlpool, whirpool },
                {HashEnum.Haval, haval256 }
            };
        }

        public byte[] ComputeBytes(byte[] input)
        {
            byte[] lHashResult = input;

            for (int it = 0; it < 2; it++)
            {
                _hashers[HashEnum.Blake].Initialize();
                _hashers[HashEnum.Blake].TransformBytes(lHashResult);

                HashLib.HashResult lresult = _hashers[HashEnum.Blake].TransformFinal();

                lHashResult = lresult.GetBytes();

                _hashers[HashEnum.Bmw].Initialize();
                _hashers[HashEnum.Bmw].TransformBytes(lHashResult);

                lresult = _hashers[HashEnum.Bmw].TransformFinal();

                lHashResult = lresult.GetBytes();

                _hashers[HashEnum.Groestl].Initialize();
                _hashers[HashEnum.Groestl].TransformBytes(lHashResult);

                lresult = _hashers[HashEnum.Groestl].TransformFinal();

                lHashResult = lresult.GetBytes();

                _hashers[HashEnum.skein].Initialize();
                _hashers[HashEnum.skein].TransformBytes(lHashResult);

                lresult = _hashers[HashEnum.skein].TransformFinal();

                lHashResult = lresult.GetBytes();

                _hashers[HashEnum.jh].Initialize();
                _hashers[HashEnum.jh].TransformBytes(lHashResult);

                lresult = _hashers[HashEnum.jh].TransformFinal();

                lHashResult = lresult.GetBytes();

                _hashers[HashEnum.keccak].Initialize();
                _hashers[HashEnum.keccak].TransformBytes(lHashResult);

                lresult = _hashers[HashEnum.keccak].TransformFinal();

                lHashResult = lresult.GetBytes();

                _hashers[HashEnum.luffa].Initialize();
                _hashers[HashEnum.luffa].TransformBytes(lHashResult);

                lresult = _hashers[HashEnum.luffa].TransformFinal();

                lHashResult = lresult.GetBytes();

                _hashers[HashEnum.CubeHash].Initialize();
                _hashers[HashEnum.CubeHash].TransformBytes(lHashResult);

                lresult = _hashers[HashEnum.CubeHash].TransformFinal();

                lHashResult = lresult.GetBytes();

                _hashers[HashEnum.shavite].Initialize();
                _hashers[HashEnum.shavite].TransformBytes(lHashResult);

                lresult = _hashers[HashEnum.shavite].TransformFinal();

                lHashResult = lresult.GetBytes();

                _hashers[HashEnum.simd].Initialize();
                _hashers[HashEnum.simd].TransformBytes(lHashResult);

                lresult = _hashers[HashEnum.simd].TransformFinal();

                lHashResult = lresult.GetBytes();

                lHashResult = _hashers[HashEnum.echo].ComputeBytes(lHashResult).GetBytes();
                lHashResult = _hashers[HashEnum.hamsi].ComputeBytes(lHashResult).GetBytes();
                lHashResult = _hashers[HashEnum.fugue].ComputeBytes(lHashResult).GetBytes();
                lHashResult = _hashers[HashEnum.shabal].ComputeBytes(lHashResult).GetBytes();
                lHashResult = _hashers[HashEnum.whirlpool].ComputeBytes(lHashResult).GetBytes();
                lHashResult = _hashers[HashEnum.sha].ComputeBytes(lHashResult).GetBytes();
                lHashResult = _hashers[HashEnum.Haval].ComputeBytes(lHashResult).GetBytes();
            }

            return lHashResult.Take(32).ToArray();
        }
    }
}