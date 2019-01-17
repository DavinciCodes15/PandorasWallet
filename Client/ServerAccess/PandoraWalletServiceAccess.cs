using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;

namespace Pandora.Client.ServerAccess
{
    internal class PandoraWalletWebService : PandoraWalletService1_1.PandoraWalletService1_1SoapClient
    {
        public PandoraWalletWebService()
        {
        }

        public PandoraWalletWebService(string endpointConfigurationName) :
                base(endpointConfigurationName)
        {
        }

        public PandoraWalletWebService(string endpointConfigurationName, string remoteAddress) :
                base(endpointConfigurationName, remoteAddress)
        {
        }

        public PandoraWalletWebService(string endpointConfigurationName, EndpointAddress remoteAddress) :
                base(endpointConfigurationName, remoteAddress)
        {
        }

        public PandoraWalletWebService(Binding binding, EndpointAddress remoteAddress) :
                base(binding, remoteAddress)
        {
        }
    }

    public delegate void OnDiconnectEventHandler();

    public delegate void ServerEvent(object sender, string aResult);

    public class PandoraWalletServiceAccess : IDisposable
    {
        public event EventHandler OnDiconnect;

        private string FConnectionId;
        private PandoraWalletWebService FServer;
        private PandoraWalletWebService FPandoraWalletWebService;
        private SynchronizationContext FSyncContext;
        private string ConnectionId { get { lock (this) { return FConnectionId; } } }

        ~PandoraWalletServiceAccess()
        {
            Dispose();
        }

        public PandoraWalletServiceAccess(string aRemoteserver, int aPort, bool aEncryptedConnection)
        {
            FSyncContext = SynchronizationContext.Current;
            RemoteServer = aRemoteserver;
            Port = aPort;
            EncryptedConnection = aEncryptedConnection;
        }

        private string GetConnectionURL()
        {
            string lURL;
            string lServer = "localhost";
            if (!string.IsNullOrEmpty(RemoteServer))
            {
                lServer = RemoteServer.ToLower();
            }

            if (EncryptedConnection)
            {
                lURL = "https:";
            }
            else
            {
                lURL = "http:";
            }

            if (lServer == "localhost")
            {
                lURL = lURL + string.Format("//{0}:{1}/{2}", lServer, Port, Defaults.PandoraURLPathLocal);
            }
            else
            {
                lURL = lURL + string.Format("//{0}:{1}/{2}", lServer, Port, Defaults.PandoraURLPathServer);
            }

            return lURL;
        }

        private PandoraWalletWebService CreatePandoraWalletServer(string aUserName, string aPassword)
        {
            PandoraWalletWebService lResult;
            try
            {
                Binding lBinding = new BasicHttpBinding();
                if (EncryptedConnection)
                {
                    lBinding = new BasicHttpsBinding();
                }

                EndpointAddress lAddress = new EndpointAddress(GetConnectionURL());
                lResult = new PandoraWalletWebService(lBinding, lAddress);
                lResult.GetServerId();
                if (FPandoraWalletWebService == null)
                {
                    FPandoraWalletWebService = new PandoraWalletWebService(lBinding, lAddress);
                }
            }
            catch
            {
                lResult = null;
                throw;
            }
            return lResult;
        }

        /// <summary>
        /// Use this method to authenticate the user ONLY this means the user account is
        /// in the database and the passwords match nothing more.  Check AccountStatus
        /// method to review the status of the account.
        /// </summary>
        /// <param name="aEmail">Users email this is not case sensitive</param>
        /// <param name="aUserName">User name that is not case sensitive</param>
        /// <param name="aPassword">Case sensitive unhashed password.</param>
        /// <returns>Returns true if the user is authenticated.</returns>
        public bool Logon(string aEmail, string aUserName, string aPassword)
        {
            try
            {
                lock (this)
                {
                    FServer = CreatePandoraWalletServer(aUserName, aPassword);
                    PandoraWalletService1_1.PandoraResult lServerResult = FServer.Logon(aEmail, aUserName, aPassword);
                    if (!string.IsNullOrEmpty(lServerResult.ErrorMsg))
                    {
                        throw new Pandora.Client.ClientLib.PandoraServerException("Server Error: " + lServerResult.ErrorMsg);
                    }

                    FConnectionId = (string)lServerResult.result;
                    Username = aUserName;
                    Email = aEmail;
                    return Connected;
                }
            }
            catch (Exception ex)
            {
                Pandora.Client.Universal.Log.Write(Pandora.Client.Universal.LogLevel.Error, "Connection to Pandora Server failed with: " + ex.Message);
                Connected = false;
                //Note: if the web server is using intergrated security it will fail with this result.
                if (ex.Message.Contains("The request failed with HTTP status 401: Unauthorized"))
                {
                    throw new Pandora.Client.ClientLib.PandoraServerException("Access denied.  Invalid user name or password.");
                }

                throw;
            }
        }

        /// <summary>
        /// Name of user last connected or currently connected.  If no user was connected the result is null.
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Email address of the connected user or last connected user.
        /// </summary>
        public string Email { get; private set; }

        /// <summary>
        /// If the object is connected to a pandora server with correct username and password this returns true
        /// Should the server fail to maintain connection this becomes false.
        /// </summary>
        public bool Connected
        {
            get
            {
                lock (this)
                {
                    return FConnectionId != null;
                }
            }

            private set
            {
                if (value)
                {
                    throw new Exception("Internal errror Connect can not be set to true.");
                }
                else
                {
                    InternalDisconnect();
                }
            }
        }

        private void InternalDisconnect()
        {
            lock (this)
            {
                if (FConnectionId != null)
                {
                    FConnectionId = null;
                    FServer = null;
                    if (OnDiconnect != null)
                    {
                        OnDiconnect(this, null);
                    }
                }
            }
        }

        public string RemoteServer { get; private set; }
        public bool EncryptedConnection { get; private set; }
        public int Port { get; private set; }

        /// <summary>
        /// Logs the current user off and turns the connected to false.
        /// </summary>
        /// <returns>Returns true only if the user is currently connected and is disconnected.</returns>
        public bool Logoff()
        {
            bool lResult = false;
            lock (this)
            {
                if (Connected)
                {
                    try
                    {
                        lResult = (bool)ReadServerResult(FServer.Logoff(ConnectionId));
                    }
                    finally
                    {
                        Connected = false;
                    }
                }
            }

            return lResult;
        }

        private object ReadServerResult(Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult aServerResult)
        {
            if (!string.IsNullOrEmpty(aServerResult.ErrorMsg))
            {
                throw new Pandora.Client.ClientLib.PandoraServerException("Server Error: " + aServerResult.ErrorMsg);
            }

            return aServerResult.result;
        }

        public string GetUserStatus()
        {
            return (string)ReadServerResult(FServer.GetUserStatus(ConnectionId));
        }

        private void CheckConnected()
        {
            if (!Connected)
            {
                throw new Exception("Not connected.");
            }
        }

        public string GetCurrencyList(uint aStartId)
        {
            CheckConnected();
            return (string)ReadServerResult(FServer.GetCurrencyList(FConnectionId, aStartId));
        }

        /// <summary>
        /// Gets the status of the currency
        /// </summary>
        public string GetCurrencyStatusList(ulong aCurrencyId, ulong aStartId)
        {
            CheckConnected();
            return (string)ReadServerResult(FServer.GetCurrencyStatusList(FConnectionId, (long)aCurrencyId, (long)aStartId));
        }

        /// <summary>
        /// The server keeps a record of all addresses created and monitored.
        /// So at any time all monitored accounts will can be accessed.
        /// </summary>
        /// <param name="aCurrencyId"></param>
        /// <param name="aAddress"></param>
        public void AddMonitoredAccount(ulong aCurrencyId, string aAddress)
        {
            CheckConnected();
            FServer.AddMonitoredAccount(ConnectionId, (long)aCurrencyId, aAddress);
        }

        /// <summary>
        /// Returns a list of address the severs is monitoring for transactions that may occur.
        /// </summary>
        /// <param name="aCurrencyId">The currency id of the Monitored address</param>
        /// <returns>returns an array of CurrencyAccount</returns>
        public string GetMonitoredAcccounts(ulong aCurrencyId, ulong aStartCurrencyAccountId)
        {
            CheckConnected();
            return (string)ReadServerResult(FServer.GetMonitoredAcccounts(ConnectionId, (long)aCurrencyId, (long)aStartCurrencyAccountId));
        }

        public bool RemoveMonitoredAcccounts(ulong aCurrencyAccountId)
        {
            CheckConnected();
            return (bool)ReadServerResult(FServer.RemoveMonitoredAccount(ConnectionId, (long)aCurrencyAccountId));
        }

        public string GetTransactionRecords(ulong aCurrencyId, ulong aStartTxRecordId)
        {
            CheckConnected();
            return (string)ReadServerResult(FServer.GetTransactionRecords(ConnectionId, (long)aCurrencyId, (long)aStartTxRecordId));
        }

        /// <summary>
        /// Creates the transaction data to be signed.
        /// </summary>
        /// <param name="aTransactionData">This is a CurrencyTransaction object.</param>
        /// <returns>returns the data that needs to be signed</returns>
        public string CreateTransaction(Client.ClientLib.CurrencyTransaction aCurrencyTransaction)
        {
            CheckConnected();
            string lTxData = aCurrencyTransaction.ToString();
            long lHandle = (long)ReadServerResult(FServer.StartGetTransactionToSign(ConnectionId, lTxData));
            do
            {
                System.Threading.Thread.Sleep(1000);
                lTxData = (string)ReadServerResult(FServer.EndGetTransactionToSign(ConnectionId, lHandle));
            } while (string.IsNullOrEmpty(lTxData));
            return lTxData;
        }

        /// <summary>
        /// Stores into the database the signed transaction for another service to boradcast.
        /// The handle returned by this call can be used to check if the transaction was broadcasted.
        /// </summary>
        /// <param name="aSignedTxData">The data for the server to broadcast</param>
        /// <returns>Returnes a SendTxHandle that can be used by IsTransactionSent function.</returns>
        public long SendTransaction(ulong aCurrencyId, string aSignedTxData)
        {
            CheckConnected();
            return (long)ReadServerResult(FServer.SendTransaction(ConnectionId, (long)aCurrencyId, aSignedTxData));
        }

        /// <summary>
        /// Call this method to see if your transaction was broadcasted to the netowrk.
        /// This method does not time out if checked many times. If the Handle is not
        /// valid an exception is thrown.
        /// </summary>
        /// <param name="aSendTxHandle">Valid handle to a transaction sent via SendTransaction</param>
        /// <returns></returns>
        public bool IsTransactionSent(long aSendTxHandle)
        {
            CheckConnected();
            return (bool)ReadServerResult(FServer.IsTransactionSent(ConnectionId, aSendTxHandle));
        }

        public string GetTransactionId(long aSendTxHandle)
        {
            CheckConnected();
            return (string)ReadServerResult(FServer.GetTransactionId(ConnectionId, aSendTxHandle));
        }

        /// <summary>
        /// Returns the icon file with all sizes of the currency.
        /// </summary>
        /// <param name="aCurrencyId">The currency id of the Icon you wish to recieve</param>
        /// <returns>Byte array binary data of the ICO file.</returns>
        public string GetCurrencyIcon(uint aCurrencyId)
        {
            CheckConnected();
            return (string)ReadServerResult(FServer.GetCurrencyIcon(ConnectionId, aCurrencyId));
        }

        public long GetBlockHeight(uint aCurrencyId)
        {
            CheckConnected();
            return (long)ReadServerResult(FServer.GetBlockHeight(ConnectionId, aCurrencyId));
        }

        public bool CheckAddress(uint aCurrencyId, string aAddress)
        {
            CheckConnected();
            return (bool)ReadServerResult(FServer.CheckAddressValidity(ConnectionId, aCurrencyId, aAddress));
        }

        public string GetCurrency(uint aCurrencyId)
        {
            CheckConnected();
            return (string)ReadServerResult(FServer.GetCurrency(FConnectionId, aCurrencyId));
        }

        public void Dispose()
        {
        }
    }
}