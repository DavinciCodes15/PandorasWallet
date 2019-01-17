using Pandora.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.FakeServerData
{
    public class FakePandoraServer : IPandoraServer
    {
        public string Username { get; private set; }

        public string Email { get; private set; }

        public bool Connected { get; private set; }

        private CurrencyAccountList AccountList;
        private List<CurrencyItem> CurrList;
        private List<CurrencyStatusItem> CurrStatusList;

        private string bitcoinTx;
        private string litecoinTx;

        public ICurrencyAccountList MonitoredAccounts
        {
            get
            {
                return AccountList;
            }
        }

        private Dictionary<uint, List<string>> TxDic;

        public string DataPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event TransactionEvent OnTransactions;

        public FakePandoraServer()
        {
            AccountList = new CurrencyAccountList();
            AccountList.AddCurrencyAccount(0, "muZkczUieaExLSvmRS649ZTWZn5waA5puT");
            AccountList.AddCurrencyAccount(0, "n4Tm9i7Yp93XPhtoRyjEJFMEKeSLVmZtXM");
            AccountList.AddCurrencyAccount(1, "mvnuGtwCPWmemXjy71b5e9K2zmBJZU8YuL");
            AccountList.AddCurrencyAccount(1, "muZkczUieaExLSvmRS649ZTWZn5waA5puT");

            CurrList.Add(new CurrencyItem(0, "FakeBitcoin", "FBTC", true, 8, new string[2] { "1.2.3", "2.3.4" },6));
            CurrList.Add(new CurrencyItem(1, "FakeLitecoin", "FLTC", true, 8, new string[2] { "3.2.1", "4.3.2." }, 6));
            CurrList.Add(new CurrencyItem(2, "FakeMonero", "FMON", true, 8, new string[2] { "3.0", "2.7" }, 6));
            CurrList.Add(new CurrencyItem(3, "FakeDash", "FDAS", true, 8, new string[2] { "1.1", "1.0" }, 6));

            CurrStatusList.Add(new CurrencyStatusItem(0, new DateTime(2018, 1, 1), CurrencyStatus.Active, "Bitcoin extended info", 894509348590));
            CurrStatusList.Add(new CurrencyStatusItem(1, new DateTime(2018, 2, 2), CurrencyStatus.Active, "Litecoin extended info", 94509348590));
            CurrStatusList.Add(new CurrencyStatusItem(2, new DateTime(2018, 3, 3), CurrencyStatus.Disabled, "Monero extended info", 4509348590));
            CurrStatusList.Add(new CurrencyStatusItem(3, new DateTime(2018, 4, 4), CurrencyStatus.Maintenance, "Dash extended info", 509348590));

            TxDic.Add(0, new List<string> { "b657e22827039461a9493ede7bdf55b01579254c1630b0bfc9185ec564fc05ab", "851b733e0bb1d8130dd7261a30079b86dfedc42727e051738854161246c71202" });

            bitcoinTx = "{\"hex\":\"020000000195253ae6ad09a47c02cb0f9d6ffba70f2" +
                              "4a9b6c6eb6731eb726d24aa94c7a1430100000000ff" +
                              "ffffff020076b010000000001976a914fbb057bbc26" +
                              "7c9deddd75e8dfad6096ad7470c4a88acc01f2e0100" +
                              "0000001976a9149a1943aaa58e63d72a911efdc9910" +
                              "2fb826c508f88ac00000000\"," +
                 "\"inputs\":[{\"txid\":\"43a1c794aa246d72eb3167ebc6b6a9240fa7fb6f9d0fcb027ca409ade63a2595\"," +
                              "\"vout\":1,\"scriptPubKey\":\"76a9149a1943aaa58e63d72a911efdc99102fb826c508f88ac\"}]}";

            litecoinTx = "{\"hex\":\"0200000001fae203d880bcce5e54b92e0e5ec1aa2d6dc93d616b3cef3ffc9fbe657d671fe50100000000ffffffff0200199222000000001976a9149a1943aaa58e63d72a911efdc99102fb826c508f88acc01f2e01000000001976a914a78e056de5b9db213021033fd15612ed3f0d24f188ac00000000\"," +
                 "\"inputs\":[{\"txid\":\"e51f677d65be9ffc3fef3c6b613dc96d2daac15e0e2eb9545ecebc80d803e2fa\"," +
                              "\"vout\":1,\"scriptPubKey\":\"76a914a78e056de5b9db213021033fd15612ed3f0d24f188ac\"}]}";

        }

        public string CreateTransaction(uint aCurrencyId, ICurrencyTransaction aSendTx)
        {
            return bitcoinTx;
        }

        public ICurrencyItem GetCurrency(uint aCurrencyId)
        {
            return CurrList.Find((x) => x.Id == aCurrencyId);
        }

        public byte[] GetCurrencyIcon(uint aCurrencyId)
        {
            return null;
        }

        public byte[] GetCurrencyLib(uint aCurrencyId, string aLibVersion)
        {
            throw new NotImplementedException();
        }

        public ICurrencyItem[] GetCurrencyList()
        {
            return CurrList.ToArray();
        }

        public ICurrencyStatusItem[] GetCurrencyStatus(uint[] aCurrencyIdArray)
        {
            CurrencyStatusItem[] lItems = new CurrencyStatusItem[aCurrencyIdArray.Length];
            foreach(uint i in aCurrencyIdArray)
            {
                lItems[i] = CurrStatusList.Find(y => y.CurrencyId == i);
            }

            return lItems;
            
        }

        public string GetExtendedTransactionInfo(uint aCurrencyId, string aTxId)
        {
            List<string> AuxList;
            if (TxDic.TryGetValue(aCurrencyId, out AuxList))
            {
                if(AuxList.Contains(aTxId))
                    return bitcoinTx;
            }
            
        return "Some fake information failed to return.";
        }

        public ulong GetTransactionFee(uint aCurrencyId, ICurrencyTransaction aSendTx)
        {
            return 200000;
        }

        public ITransactionRecord[] GetTransactions(uint aCurrencyId, string aAddress, bool aMonitored = true)
        {
            throw new NotImplementedException();
        }

        public bool Logoff()
        {
            if (Connected)
            {
                Connected = false;
                return true;
            }
            else return false;
        }

        public LogonResult Logon(string aEmail, string aUserName, string aPasswordHash)
        {   
            if (aEmail == "test@test.com" && aUserName == "test" && aPasswordHash == "test")
            {
                Email = aEmail;
                Username = aUserName;
                Connected = true;
                return LogonResult.AccessGranted;
            }
            else return LogonResult.AccessDenied;
        }

        public void SendTransaction(string aSignedTxData)
        {
            return;
        }
    }
}
