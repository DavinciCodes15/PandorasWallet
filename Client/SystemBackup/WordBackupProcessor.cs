//   Copyright 2017-2019 Davinci Codes
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Also use the software for non-commercial purposes.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandora.Client.SystemBackup
{
    public static class WordBackupProcessor
    {
        private static WordList FWordList = new WordList();

        public static string[] GetWordsFromSeed(string aRootSeed)
        {
            var lIntWords = GetMnemonicNumbersFromString(aRootSeed);
            return FWordList.GetWords(lIntWords);
        }

        public static string GetSeedFromWords(string[] aSetOfWords)
        {
            var lInts = FWordList.GetNumbers(aSetOfWords);
            return GetHexSeed(lInts);
        }

        public static string[] EnglishWordList { get => WordList.englishWords; }

        private static uint[] GetMnemonicNumbersFromString(string aSeed)
        {
            string lSeedWithoutHypen = aSeed.Replace("-", "");
            if (string.IsNullOrEmpty(lSeedWithoutHypen) || (lSeedWithoutHypen.Length % 32) != 0) throw new Exception("Phrase length should be a multiple of 32. Please review words written");
            List<uint> lResult = new List<uint>();
            for (int j = 0; j < (lSeedWithoutHypen.Length / 32); j++)
            {
                var lSplitSeed = lSeedWithoutHypen.Substring(32 * j, 32);
                long lLast4BitSum = 0;
                long lCurrentNum = 0;
                int lRemainder = 0;
                string lHex;
                for (int i = 0; i < 11; i++)
                {
                    if (i == 10)
                        lHex = lSplitSeed.Substring(i * 3, 2);
                    else
                        lHex = lSplitSeed.Substring(i * 3, 3);
                    long lNum = long.Parse(lHex, System.Globalization.NumberStyles.HexNumber);
                    lCurrentNum |= lNum << lRemainder;
                    lResult.Add((uint)(lCurrentNum & 2047));
                    lCurrentNum >>= 11;
                    lRemainder++;
                    lLast4BitSum ^= (lNum & 0xf) ^ ((lNum >> 4) & 0xf) ^ (lNum >> 8);
                }
                lCurrentNum |= (lLast4BitSum << 7);
                lResult.Add((uint)lCurrentNum);
            }
            return lResult.ToArray();
        }

        private static string GetHexSeed(uint[] aNumbers)
        {
            if (aNumbers?.Length % 12 != 0) throw new ArgumentException("Number of Words should be a multiple of 12. Please review words written");
            int lCounter = 0;
            int lIndexCounter = 0;
            var lWordNumbers = aNumbers.GroupBy((lNumber) =>
            {
                if (lIndexCounter++ % 12 == 0) lCounter++;
                return lCounter;
            }).Select(lGroup => lGroup.ToArray()).ToArray();
            var lResult = new StringBuilder();
            foreach (var l12Numbers in lWordNumbers)
                lResult.Append(ExtractHexFrom12Words(l12Numbers));
            return lResult.ToString();
        }

        private static string ExtractHexFrom12Words(uint[] a12WordNumbers)
        {
            if (a12WordNumbers?.Length != 12) throw new ArgumentException(nameof(a12WordNumbers), "Numbers should be exactly 12");
            StringBuilder lResultBuilder = new StringBuilder();
            ushort lReminder = 11;
            ushort lMoveNumber = 0;
            long lBitsToTake = 0;
            long lBitsTaken = 0;
            long lNum = 0;
            string lHex = "";
            long lLast8Bits = 0;
            long lLast4BitSum = 0;
            for (int i = 0; i < 10; i++)
            {
                lBitsToTake = lBitsToTake + (long)Math.Pow(2, i);
                lBitsTaken = a12WordNumbers[i + 1] & lBitsToTake;
                lNum = (a12WordNumbers[i] >> lMoveNumber) | lBitsTaken << lReminder;
                if (Convert.ToString(lNum, 16).Length == 2)
                    lResultBuilder.Append($"0{Convert.ToString(lNum, 16)}");

                if (Convert.ToString(lNum, 16).Length == 1)
                    lResultBuilder.Append($"00{Convert.ToString(lNum, 16)}");

                if (Convert.ToString(lNum, 16).Length == 3)
                    lResultBuilder.Append(Convert.ToString(lNum, 16));

                lLast4BitSum ^= (lNum & 0xf) ^ ((lNum >> 4) & 0xf) ^ (lNum >> 8);
                lReminder--;
                lMoveNumber++;
            }
            long lLastBit = a12WordNumbers[10] >> 10;
            lLast8Bits = ((a12WordNumbers[11] & 127) << 1) | lLastBit;
            lLast4BitSum ^= (lLast8Bits & 0xf) ^ ((lLast8Bits >> 4) & 0xf);
            if (lLast8Bits < 16)
            {
                lResultBuilder.Append(0);
            }
            lResultBuilder.Append(Convert.ToString(lLast8Bits, 16));
            if (lLast4BitSum != (a12WordNumbers[11] >> 7) && (((lLast8Bits >> 1) | (lLast4BitSum << 7)) != a12WordNumbers[11]))
                throw new ArgumentOutOfRangeException("One or more words are incorrect");
            lHex = lResultBuilder.ToString();
            return lHex;
        }
    }
}