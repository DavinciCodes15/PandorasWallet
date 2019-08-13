using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pandora.Client.Crypto.Currencies.Bitcoin;
using Pandora.Client.ClientLib;
using NBitcoin;
using Pandora.Client.Crypto.Currencies.Controls;
using System;
using System.Collections.Generic;
using Pandora.Client.Crypto.Currencies;

namespace Pandora.Client.Crypto.Test
{
    [TestClass]
    public class RootSeedWithSalt
    {
        public CurrencyControl lCurrencyControl = CurrencyControl.GetCurrencyControl();

        //[TestMethod]
        //public void RootSeed()
        //{
        //    var guid = Guid.NewGuid();
        //    string lRootSeed = lCurrencyControl.GenerateRootSeed("maan221292@gmail.com", "miguel", Guid.Parse("103e7eae-8de7-4962-9196-cffb25711055"));
        //    Assert.AreEqual(lRootSeed, "9DB7353AF6EA48350FFD7127BF65C195");
        //}

        [TestMethod]
        public void Testing()
        {
            string Hex = "9DB7353AF6EA48350FFD7127BF65CC95"; //"9DB7353AF6EA48350FFD7127BF65CC95";
            uint[] lista = Get12NumbersOf11Bits(Hex);
            string lHex = GetHexPassCode(lista);

            Assert.AreEqual("9DB7353AF6EA48350FFD7127BF65CC95", lHex.ToUpper());
        }

        public static byte[] HexStringToByteArray(string aHex)
        {
            byte[] lArray = new byte[aHex.Length / 2];
            for (int i = 0; i < aHex.Length; i += 2)
                lArray[i / 2] = Byte.Parse(aHex.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
            return lArray;
        }

        [TestMethod]
        public void TestMyKey()
        {
            var lAdvocacy = CurrencyControl.GetClientCurrencyAdvocacy(1, new ChainParams(ChainParams.NetworkType.MainNet), GetSeed);
            var lSeed = CurrencyControl.GenerateRootSeed("test@test.com", "davincij", new Guid("9DB7353AF6EA48350FFD7127BF65CC95").ToByteArray());
            if (lSeed != "AC1946EC73DB3A6C5BFB34E23300AF92")
                throw new Exception();

            var lTestAddress = lAdvocacy.GetAddress(0);
            var lTestKey = lAdvocacy.GetPrivateKey(0);
            var lTestKey2 = lAdvocacy.GetPrivateKey(1);
            var lTestAdderss2 = lAdvocacy.GetAddress(1);
            Assert.AreEqual(lTestAddress, "16G1qvnTRXdNcf7we1rHTWCKZd7Lbpbh39");
            Assert.AreEqual(lTestKey, "KxqP92g4DtsvBs9hkz32qZCEvD7p2MkCrMeBNngEDh3guVsvsj5p");
            Assert.AreEqual(lTestAdderss2, "36Q3MhZhcMz69YE6QJpeKRAJDW32DrV3yx");
            Assert.AreEqual(lTestKey2, "L2zFMfmB1harosR6wF3MGrkhphBKHmpHoJ3mGqkXPfHhaavEy2Yz");
        }

        private string GetSeed()
        {
            return "AC1946EC73DB3A6C5BFB34E23300AF92AC1946EC73DB3A6C5BFB34E23300AF92";
            throw new NotImplementedException();
        }

        private uint[] Get12NumbersOf11Bits(string aHex)
        {
            string l32ByteHex = aHex; //FKeyManager.UserGuid.ToString().Replace("-", "");
            List<uint> lResult = new List<uint>();
            uint lLast4BitSum = 0;
            ulong lCurrentNum = 0;
            int lRemainder = 0;
            if (string.IsNullOrEmpty(l32ByteHex) || l32ByteHex.Length != 32)
                throw new ArgumentOutOfRangeException("String must have a length of 32.");
            string lHex;
            for (int i = 0; i < 11; i++)
            {
                if (i == 10)
                {
                    lHex = l32ByteHex.Substring(i * 3, 2);
                }
                else
                {
                    lHex = l32ByteHex.Substring(i * 3, 3);
                }
                ushort lNum = ushort.Parse(lHex, System.Globalization.NumberStyles.HexNumber);
                lCurrentNum |= (uint)(lNum << lRemainder);
                lResult.Add((uint)(lCurrentNum & 2047));
                lCurrentNum >>= 11;
                lRemainder++;
                lLast4BitSum ^= (ushort)((lNum & 0xf) ^ ((lNum >> 4) & 0xf) ^ (lNum >> 8));
            }
            lCurrentNum |= (lLast4BitSum << 7);
            lResult.Add((uint)lCurrentNum);
            return lResult.ToArray();
        }

        private string GetHexPassCode(uint[] aNumbers)
        {
            ushort lReminder = 11;
            ushort lMoveNumber = 0;
            uint lBitsToTake = 0;
            ushort lBitsTaken = 0;
            uint lNum = 0;
            string lHex = "";
            string SumAllNumbers = "";
            uint lLast8Bits = 0;
            uint lLast4BitSum = 0;
            uint lSum8Bits = 0;
            for (int i = 0; i < 10; i++)
            {
                lBitsToTake = lBitsToTake + (uint)Math.Pow(2, i);
                lBitsTaken = (ushort)(aNumbers[i + 1] & lBitsToTake);
                lNum = (aNumbers[i] >> lMoveNumber) | (uint)(lBitsTaken << lReminder);
                SumAllNumbers = SumAllNumbers + Convert.ToString(lNum, 16);
                lLast4BitSum ^= (uint)((lNum & 0xf) ^ ((lNum >> 4) & 0xf) ^ (lNum >> 8));
                lReminder--;
                lMoveNumber++;
            }
            uint lLastBit = aNumbers[10] >> 10;
            lLast8Bits = ((aNumbers[11] & 127) << 1) | lLastBit;
            lLast4BitSum ^= (uint)((lLast8Bits & 0xf) ^ ((lLast8Bits >> 4) & 0xf));
            SumAllNumbers = SumAllNumbers + Convert.ToString(lLast8Bits, 16);
            if (lLast4BitSum != (aNumbers[11] >> 7))
                throw new ArgumentOutOfRangeException("One of the word is incorrect");
            lHex = SumAllNumbers.ToString();

            return lHex;
        }
    }
}