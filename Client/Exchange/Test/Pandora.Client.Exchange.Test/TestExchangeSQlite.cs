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
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pandora.Client.Exchange.Factories;

namespace Pandora.Client.Exchange.Test
{
    [TestClass]
    public class TestExchangeSQlite
    {
        private const string FTestFileName = "ExchangeSqliteTest";
        private const string FUsername = "user";
        private const string FEmail = "useremail@email.co.cl";

        public string DebugPath => AppContext.BaseDirectory;
        public string FullTestFilePath => Path.Combine(DebugPath, string.Concat(FTestFileName, ".exchange"));

        [TestMethod]
        public void TestDBCreation()
        {
            var lSaveManagerFactory = PandoraSaveManagerFactory.GetSaveMangerFactory();
            var lSaveManager = lSaveManagerFactory.GetNewPandoraSaveManager(SavePlace.SQLiteDisk);
            if (File.Exists(FullTestFilePath))
                File.Delete(FullTestFilePath);
            lSaveManager.Initialize(DebugPath, FTestFileName, FUsername, FEmail);
            Assert.IsTrue(File.Exists(FullTestFilePath));
            lSaveManager.Dispose();
            var lSaveManager2 = lSaveManagerFactory.GetNewPandoraSaveManager(SavePlace.SQLiteDisk);
            lSaveManager2.Initialize(DebugPath, FTestFileName, FUsername, FEmail);
            Assert.AreEqual(lSaveManager2.Version, 103);
            lSaveManager2.Dispose();
        }

        [TestMethod]
        public void TestWriteUserKeysIntoDB()
        {
            if (File.Exists(FullTestFilePath))
                File.Delete(FullTestFilePath);
            var lSaveManagerFactory = PandoraSaveManagerFactory.GetSaveMangerFactory();
            var lSaveManager = lSaveManagerFactory.GetNewPandoraSaveManager(SavePlace.SQLiteDisk);
            lSaveManager.Initialize(DebugPath, FTestFileName, FUsername, FEmail);

            //Random GUIDs to test
            var lUserPublic = "b687d248edb346f5a1c77bdd9747035d";
            var lUserPrivate = "8a01640af9bf437dafcc235967fab6fb";
            var lUser2Public = "14eed08178ba4eb986a700d84ea193df";
            var lUser2Private = "630debc3a3d54daf8efed5eeb6adc25d";

            lSaveManager.WriteKeyValue("EXCHANGE_PUBLIC", lUserPublic, 1);
            lSaveManager.WriteKeyValue("EXCHANGE_PRIVATE", lUserPrivate, 1);
            lSaveManager.WriteKeyValue("EXCHANGE_PUBLIC", lUser2Public, 2);
            lSaveManager.WriteKeyValue("EXCHANGE_PRIVATE", lUser2Private, 2);

            //Two times to check that actually got replaced
            lSaveManager.WriteKeyValue("EXCHANGE_PUBLIC", lUserPublic, 1);
            lSaveManager.WriteKeyValue("EXCHANGE_PRIVATE", lUserPrivate, 1);
            lSaveManager.WriteKeyValue("EXCHANGE_PUBLIC", lUser2Public, 2);
            lSaveManager.WriteKeyValue("EXCHANGE_PRIVATE", lUser2Private, 2);

            Assert.AreEqual(lSaveManager.ReadKeyValue("EXCHANGE_PUBLIC", 1), lUserPublic);
            Assert.AreEqual(lSaveManager.ReadKeyValue("EXCHANGE_PRIVATE", 1), lUserPrivate);
            Assert.AreEqual(lSaveManager.ReadKeyValue("EXCHANGE_PUBLIC", 2), lUser2Public);
            Assert.AreEqual(lSaveManager.ReadKeyValue("EXCHANGE_PRIVATE", 2), lUser2Private);
            Assert.IsNull(lSaveManager.ReadKeyValue("EXCHANGE_PRIVATE", 4));
            Assert.IsNull(lSaveManager.ReadKeyValue("EXCHANGE_PRIV", 4));
        }

        [TestMethod]
        public void TestExchangeKeyValueHelper()
        {
            if (File.Exists(FullTestFilePath))
                File.Delete(FullTestFilePath);
            var lSaveManagerFactory = PandoraSaveManagerFactory.GetSaveMangerFactory();
            var lSaveManager = lSaveManagerFactory.GetNewPandoraSaveManager(SavePlace.SQLiteDisk);
            lSaveManager.Initialize(DebugPath, FTestFileName, FUsername, FEmail);
            var lKeyValueHelper = new ExchangeKeyValueHelper<TestKeyValueObject>(lSaveManager);
            var lTestObjects = new TestKeyValueObject[]
            {
                new TestKeyValueObject
                {
                    ProfileID = 1,
                    PublicKey = "b687d248edb346f5a1c77bdd9747035d",
                    PrivateKey = "8a01640af9bf437dafcc235967fab6fb",
                },
                new TestKeyValueObject
                {
                    ProfileID = 2,
                    PublicKey = "14eed08178ba4eb986a700d84ea193df",
                    PrivateKey = "630debc3a3d54daf8efed5eeb6adc25d"
                }
            };
            foreach (var lTestObject in lTestObjects)
                lKeyValueHelper.SaveChanges(lTestObject);
            var lObject = lKeyValueHelper.LoadKeyValues(2);
            Assert.AreEqual(lObject.PublicKey, "14eed08178ba4eb986a700d84ea193df");
            Assert.AreEqual(lObject.PrivateKey, "630debc3a3d54daf8efed5eeb6adc25d");

            Assert.ThrowsException<Exception>(() => lKeyValueHelper.WriteKey("exchange_secret", "testestest123123123", 3));

            lKeyValueHelper.WriteKey("exchange_private", "testestest123123123", 3);
            var lObject2 = lKeyValueHelper.LoadKeyValues(3);
            Assert.AreEqual(lObject2.PrivateKey, "testestest123123123");
        }

        private class TestKeyValueObject : IExchangeKeyValueObject
        {
            public int ProfileID { get; set; }

            [ExchangeKeyName("EXCHANGE_PUBLIC")]
            public string PublicKey { get; set; }

            [ExchangeKeyName("EXCHANGE_PRIVATE")]
            public string PrivateKey { get; set; }
        }
    }
}