using Pandora.Client.ClientLib;
using Pandora.Client.Universal;
using Pandora.Client.Universal.Threading;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using Pandora.Client.ServerAccess.PandoraWalletService1_2;

namespace Pandora.Client.ServerAccess
{
    internal class PandoraWalletWebService : PandoraWalletService1_2SoapClient
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

        public bool Disconnected { get; set; }
    }

    public delegate void OnDiconnectEventHandler();

    public class PandoraServiceError : Exception
    {
        public PandoraServiceError(string aMessage) : base(aMessage)
        {
        }
    }

    public delegate void ServerEvent(object sender, string aResult);

    public class PandoraWalletServiceAccess : MethodJetThread
    {
        public event EventHandler OnDiconnect;

        private delegate bool LogonDelegate(string aEmail, string aUserName, string aPassword);

        private delegate bool Logon2Delegate(string aEmail, string aUserName, string aPassword, string aVersion);

        private delegate string GetCurrencyListDelegate(long aStartId);

        private delegate string GetCurrencyStatusListDelegate(long aCurrencyId, long aStartId);

        private delegate long AddMonitoredAccountDelegate(long aCurrencyId, string aAddress);

        private delegate string GetMonitoredAcccountsDelegate(long aCurrencyId, long aStartCurrencyAccountId);

        private delegate bool RemoveMonitoredAcccountsDelegate(long aCurrencyAccountId);

        private delegate string GetTransactionRecordsDelegate(long aCurrencyId, long aStartTxRecordId, bool aIncludeScript);

        private delegate string CreateTransactionDelegate(CurrencyTransaction aCurrencyTransaction);

        private delegate long SendTransactionDelegate(long aCurrencyId, string aSignedTxData);

        private delegate bool IsTransactionSentDelegate(long aSendTxHandle);

        private delegate string GetTransactionIdDelegate(long aSendTxHandle);

        private delegate string GetCurrencyIconDelegate(long aCurrencyId);

        private delegate long GetBlockHeightDelegate(long aCurrencyId);

        private delegate bool CheckAddressDelegate(long aCurrencyId, string aAddress);

        private delegate string GetCurrencyDelegate(long aCurrencyId);

        private delegate string GetCurrencyTokenDelegate(long aCurrencyId, string aTokenContract);

        private delegate long GetCurrencyTxFeeDelegate(long aCurrencyId);

        private delegate bool MarkOldUserDelegate(string aEmail, string aUserName);

        private PandoraWalletWebService FServer;
        private PandoraWalletWebService FPandoraWalletWebService;
        private SynchronizationContext FSyncContext;
        public string ConnectionId { get; private set; }

        public PandoraWalletServiceAccess(string aRemoteserver, int aPort, bool aEncryptedConnection)
        {
            FSyncContext = SynchronizationContext.Current;
            RemoteServer = aRemoteserver;
            Port = aPort;
            EncryptedConnection = aEncryptedConnection;
            Run();
            this.OnErrorEvent += PandoraWalletServiceAccess_OnErrorEvent;
        }

        public PandoraWalletServiceAccess(string aRemoteserver, int aPort, bool aEncryptedConnection, string aConnectionId)
        {
            FSyncContext = SynchronizationContext.Current;
            RemoteServer = aRemoteserver;
            Port = aPort;
            EncryptedConnection = aEncryptedConnection;
            ConnectionId = aConnectionId;
            FServer = CreatePandoraWalletServer();
            Run();
            this.OnErrorEvent += PandoraWalletServiceAccess_OnErrorEvent;
        }

        ~PandoraWalletServiceAccess()
        {
            Dispose();
        }

        public override void Dispose()
        {
            base.Dispose();
            InternalDisconnect();
        }

        private void PandoraWalletServiceAccess_OnErrorEvent(object sender, Exception e, ref bool aIsHandled)
        {
            aIsHandled = true;
        }

        private string GetConnectionURL(string aRemoteServer)
        {
            string lURL;
            string lServer = "localhost";
            if (!string.IsNullOrEmpty(RemoteServer))
            {
                lServer = aRemoteServer.ToLower();
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

        private PandoraWalletWebService CreatePandoraWalletServer()
        {
            PandoraWalletWebService lResult;

            // Create connection binding
            var lBinding = new BasicHttpBinding();
            // Set maximum size of packages (maximum of 2147483647 = 2GB)
            lBinding.MaxReceivedMessageSize = 5242880; // => 5MB limit
            lBinding.MaxBufferPoolSize = 5242880;
            lBinding.MaxBufferSize = 5242880;
            if (EncryptedConnection)
            {
                lBinding.Security.Mode = BasicHttpSecurityMode.Transport;
                lBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                lBinding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
                lBinding.Security.Transport.Realm = string.Empty;
                lBinding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.Certificate;
                lBinding.Security.Message.AlgorithmSuite = System.ServiceModel.Security.SecurityAlgorithmSuite.Default;
            }
            lBinding.Name = "PWService_SOAP_Binding";
            EndpointAddress lAddress = new EndpointAddress(GetConnectionURL(RemoteServer));
            lResult = new PandoraWalletWebService(lBinding, lAddress);
            lResult.GetServerId();
            if (FPandoraWalletWebService == null)
                FPandoraWalletWebService = new PandoraWalletWebService(lBinding, lAddress);
            return lResult;
        }

        //
        // By default this object remains connected even if there is an error
        // If the users connection got expired we need to
        // disconnect from the server and make this connection
        //
        public override object Invoke(Delegate method, params object[] args)
        {
            try
            {
                return base.Invoke(method, args);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Connection Expired."))
                    InternalDisconnect();
                throw;
            }
        }

        private object ReadServerResult(PandoraResult aServerResult)
        {
            if (!string.IsNullOrEmpty(aServerResult.ErrorMsg)) // server sside error occured.
            {
                if (aServerResult.ErrorType == typeof(SubscriptionOverException).Name)
                    throw new SubscriptionOverException(aServerResult.ErrorMsg);
                else
                    throw new PandoraServerException("Server Error: " + aServerResult.ErrorMsg);
            }
            return aServerResult.result;
        }

        private void CheckConnected()
        {
            if (!Connected)
                throw new PandoraServiceError("Not connected.");
        }

        private void InternalDisconnect()
        {
            lock (this)
            {
                if (ConnectionId != null)
                {
                    ConnectionId = null;
                    FServer = null;
                    if (OnDiconnect != null)
                    {
                        OnDiconnect(this, null);
                    }
                }
            }
        }

        // This method is overrdden so that we can try reconnecting when ever the ThreadXXX method is called
        // so we can do what ever w can to recover from bad connections or if the one server
        // goes down for maintainane
        protected override bool InvokeMethodMessage(DelegateMessage aMethodMessage)
        {
            aMethodMessage.OnAfterEvent += MethodMessage_OnAfterEvent;
            if (FServer != null && FServer.Disconnected)
            {
                int lCounter = 0;
                PandoraWalletWebService lServer = null;
                do
                {
                    Log.Write(LogLevel.Debug, "InvokeMethodMessage: try reconnect to server {0}", lCounter);
                    try
                    {
                        lServer = CreatePandoraWalletServer();
                        FServer = lServer;
                    }
                    catch
                    {
                        lCounter++;
                    }
                } while (lServer == null && lCounter < 2);
            }
            return base.InvokeMethodMessage(aMethodMessage);
        }

        private void MethodMessage_OnAfterEvent(object sender, EventArgs e)
        {
            try
            {
                DelegateMessage lMethodMessage = sender as DelegateMessage;

                if (lMethodMessage.ExceptionObject != null) // do not try reconnect if the logon method is called
                {
                    /// if we cant connet to the server because its down
                    if (lMethodMessage.ExceptionObject is System.ServiceModel.EndpointNotFoundException)
                    {
                        Log.Write(LogLevel.Error, lMethodMessage.ExceptionObject.Message);
                        lMethodMessage.ExceptionObject = new PandoraServiceError("Internet connection error occured while connecting to the remote server.");
                        if (FServer != null)
                            FServer.Disconnected = true;
                    }
                    var lName = lMethodMessage.ToString();

                    // an exception occured
                    // Now test if the error is because of a communication issue or
                    // if the error is because the server sent the error to be thrown
                    if (!(lMethodMessage.ExceptionObject is PandoraServerException) && lName != "ThreadLogon")
                    {      // if there is a comunication faild we need to reconnect
                        int lCounter = 0;
                        PandoraWalletWebService lServer = null;
                        do
                        {
                            Log.Write(LogLevel.Debug, "MethodMessage_OnAfterEvent: try reconnect to server {0}", lCounter);
                            try
                            {
                                lServer = CreatePandoraWalletServer();
                                FServer = lServer;
                            }
                            catch
                            {
                                System.Threading.Thread.Sleep(1000);
                                lCounter++;
                            }
                        } while (lServer == null && lCounter < 2);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(LogLevel.Error, "Connection to Pandora Server failed with: " + ex.Message);
                throw;
            }
        }

        public DateTime LastConnected { get; private set; }

        private bool ThreadLogon(string aEmail, string aUserName, string aPassword, string aVersion)
        {
            try
            {
                // only this thread an no other app has access to the
                // Only one method will be executed at a time
                //
                //  This Method will create an test the conneciton and find
                //  Other servers to connect to.
                FServer = CreatePandoraWalletServer();
                // Assuming this connection is now solid call the logon
                var lServerResult = FServer.Logon(aEmail, aUserName, aPassword, aVersion);
                LastConnected = lServerResult.LastConnected;
                if (!string.IsNullOrEmpty(lServerResult.ErrorMsg))
                    throw new PandoraServerException("Server Error: " + lServerResult.ErrorMsg);
                ConnectionId = (string) lServerResult.result;
                UserName = aUserName;
                Email = aEmail;
                return Connected;
            }
            catch (EndpointNotFoundException ex)
            {
                Log.Write(LogLevel.Error, "Connection to Pandora Server failed with: " + ex.Message);
                Connected = false;
                throw new PandoraServerException("Server not available. Please ensure you have an active internet conection.");
            }
            catch (Exception ex)
            {
                Log.Write(LogLevel.Error, "Connection to Pandora Server failed with: " + ex.Message);
                Connected = false;
                //Note: if the web server is using intergrated security it will fail with this result.
                if (ex.Message.Contains("The request failed with HTTP status 401: Unauthorized"))
                    throw new PandoraServerException("Access denied.  Invalid user name or password.");
                throw;
            }
        }

        private string ThreadGetCurrencyList(long aStartId)
        {
            return (string) ReadServerResult(FServer.GetCurrencyList(ConnectionId, aStartId));
        }

        private string ThreadGetCurrencyStatusList(long aCurrencyId, long aStartId)
        {
            return (string) ReadServerResult(FServer.GetCurrencyStatusList(ConnectionId, aCurrencyId, aStartId));
        }

        private long ThreadAddMonitoredAccount(long aCurrencyId, string aAddress)
        {
            return (long) ReadServerResult(FServer.AddMonitoredAccount(ConnectionId, aCurrencyId, aAddress));
        }

        private string ThreadGetMonitoredAcccounts(long aCurrencyId, long aStartCurrencyAccountId)
        {
            return (string) ReadServerResult(FServer.GetMonitoredAcccounts(ConnectionId, aCurrencyId, aStartCurrencyAccountId));
        }

        private bool ThreadRemoveMonitoredAcccounts(long aCurrencyAccountId)
        {
            return (bool) ReadServerResult(FServer.RemoveMonitoredAccount(ConnectionId, aCurrencyAccountId));
        }

        private string ThreadGetTransactionRecords(long aCurrencyId, long aStartTxRecordId, bool aIncludeScript)
        {
            return (string) ReadServerResult(FServer.GetTransactionRecords(ConnectionId, aCurrencyId, aStartTxRecordId, aIncludeScript));
        }

        private string ThreadCreateTransaction(CurrencyTransaction aCurrencyTransaction)
        {
            string lTxData = aCurrencyTransaction.ToString();
            long lHandle = (long) ReadServerResult(FServer.StartGetTransactionToSign(ConnectionId, lTxData));
            do
            {
                System.Threading.Thread.Sleep(1000);
                lTxData = (string) ReadServerResult(FServer.EndGetTransactionToSign(ConnectionId, lHandle));
            } while (string.IsNullOrEmpty(lTxData));
            return lTxData;
        }

        private long ThreadSendTransaction(long aCurrencyId, string aSignedTxData)
        {
            return (long) ReadServerResult(FServer.SendTransaction(ConnectionId, aCurrencyId, aSignedTxData));
        }

        private bool ThreadIsTransactionSent(long aSendTxHandle)
        {
            return (bool) ReadServerResult(FServer.IsTransactionSent(ConnectionId, aSendTxHandle));
        }

        private string ThreadGetTransactionId(long aSendTxHandle)
        {
            return (string) ReadServerResult(FServer.GetTransactionId(ConnectionId, aSendTxHandle));
        }

        private string ThreadGetCurrencyIcon(long aCurrencyId)
        {
            return (string) ReadServerResult(FServer.GetCurrencyIcon(ConnectionId, aCurrencyId));
        }

        private long ThreadGetCurrencyTxFee(long aCurrencyId)
        {
            return Convert.ToInt64(ReadServerResult(FServer.GetCurrencyTxFee(ConnectionId, aCurrencyId)));
        }

        private long ThreadGetBlockHeight(long aCurrencyId)
        {
            return Convert.ToInt64(ReadServerResult(FServer.GetBlockHeight(ConnectionId, aCurrencyId)));
        }

        private string ThreadGetCurrency(long aCurrencyId)
        {
            var lResult = FServer.GetCurrency(ConnectionId, aCurrencyId);
            return (string) ReadServerResult(lResult);
        }

        private string ThreadGetCurrencyToken(long aCurrencyID, string aTokenAddress)
        {
            var lResult = FServer.GetCurrencyToken(ConnectionId, aCurrencyID, aTokenAddress);
            return (string) ReadServerResult(lResult);
        }

        public bool ThreadCheckAddress(long aCurrencyId, string aAddress)
        {
            return (bool) ReadServerResult(FServer.CheckAddressValidity(ConnectionId, aCurrencyId, aAddress));
        }

        private string ThreadGetLastCurrencyStatus(long aCurrencyId)
        {
            var lResult = FServer.GetLastCurrencyStatus(ConnectionId, aCurrencyId);
            return (string) ReadServerResult(lResult);
        }

        /// <summary>
        /// Name of user last connected or currently connected.  If no user was connected the result is null.
        /// </summary>
        public string UserName { get; private set; }

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
                return ConnectionId != null;
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

        public string RemoteServer { get; private set; }
        public bool EncryptedConnection { get; private set; }
        public int Port { get; private set; }

        /// <summary>
        /// Use this method to authenticate the user ONLY this means the user account is
        /// in the database and the passwords match nothing more.  Check AccountStatus
        /// method to review the status of the account.
        /// </summary>
        /// <param name="aEmail">Users email this is not case sensitive</param>
        /// <param name="aUserName">User name that is not case sensitive</param>
        /// <param name="aPassword">Case sensitive unhashed password.</param>
        /// <returns>Returns true if the user is authenticated.</returns>
        //public bool Logon(string aEmail, string aUserName, string aPassword)
        //{
        //    return (bool)this.Invoke(new LogonDelegate(ThreadLogon), aEmail, aUserName, aPassword);
        //}

        /// <summary>
        /// Use this method to authenticate the user ONLY this means the user account is
        /// in the database and the passwords match nothing more.  Check AccountStatus
        /// method to review the status of the account.
        /// </summary>
        /// <param name="aEmail">Users email this is not case sensitive</param>
        /// <param name="aUserName">User name that is not case sensitive</param>
        /// <param name="aPassword">Case sensitive unhashed password.</param>
        /// <returns>Returns true if the user is authenticated.</returns>
        public bool Logon(string aEmail, string aUserName, string aPassword, string aVersion)
        {
            return (bool) this.Invoke(new Logon2Delegate(ThreadLogon), aEmail, aUserName, aPassword, aVersion);
        }

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
                        lResult = (bool) ReadServerResult(FServer.Logoff(ConnectionId));
                    }
                    finally
                    {
                        Connected = false;
                    }
                }
            }

            return lResult;
        }

        public string GetUserStatus()
        {
            return (string) ReadServerResult(FServer.GetUserStatus(ConnectionId));
        }

        /// <summary>
        /// Returns a Json Array of longs
        /// </summary>
        /// <param name="aStartId"></param>
        /// <returns>returns a JSON array of currency ids</returns>
        public string GetCurrencyList(long aStartId)
        {
            CheckConnected();
            return (string) this.Invoke(new GetCurrencyListDelegate(ThreadGetCurrencyList), aStartId);
        }

        /// <summary>
        /// Gets the status of the currency
        /// </summary>
        public string GetCurrencyStatusList(long aCurrencyId, long aStartId)
        {
            CheckConnected();
            return (string) this.Invoke(new GetCurrencyStatusListDelegate(ThreadGetCurrencyStatusList), aCurrencyId, aStartId);
        }

        /// <summary>
        /// The server keeps a record of all addresses created and monitored.
        /// So at any time all monitored accounts will can be accessed.
        /// </summary>
        /// <param name="aCurrencyId"></param>
        /// <param name="aAddress"></param>
        public long AddMonitoredAccount(long aCurrencyId, string aAddress)
        {
            CheckConnected();
            return (long) this.Invoke(new AddMonitoredAccountDelegate(ThreadAddMonitoredAccount), aCurrencyId, aAddress);
        }

        /// <summary>
        /// Returns a list of address the severs is monitoring for transactions that may occur.
        /// </summary>
        /// <param name="aCurrencyId">The currency id of the Monitored address</param>
        /// <returns>returns an array of CurrencyAccount</returns>
        public string GetMonitoredAcccounts(long aCurrencyId, long aStartCurrencyAccountId)
        {
            CheckConnected();
            return (string) this.Invoke(new GetMonitoredAcccountsDelegate(ThreadGetMonitoredAcccounts), aCurrencyId, aStartCurrencyAccountId);
        }

        public bool RemoveMonitoredAcccounts(long aCurrencyAccountId)
        {
            CheckConnected();
            return (bool) this.Invoke(new RemoveMonitoredAcccountsDelegate(ThreadRemoveMonitoredAcccounts), aCurrencyAccountId);
        }

        public string GetTransactionRecords(long aCurrencyId, long aStartTxRecordId, bool aIncludeScript = false)
        {
            CheckConnected();
            return (string) this.Invoke(new GetTransactionRecordsDelegate(ThreadGetTransactionRecords), aCurrencyId, aStartTxRecordId, aIncludeScript);
        }

        /// <summary>
        /// Creates the transaction data to be signed.
        /// </summary>
        /// <param name="aTransactionData">This is a CurrencyTransaction object.</param>
        /// <returns>returns the data that needs to be signed</returns>
        public string CreateTransaction(CurrencyTransaction aCurrencyTransaction)
        {
            CheckConnected();
            return (string) this.Invoke(new CreateTransactionDelegate(ThreadCreateTransaction), aCurrencyTransaction);
        }

        /// <summary>
        /// Stores into the database the signed transaction for another service to boradcast.
        /// The handle returned by this call can be used to check if the transaction was broadcasted.
        /// </summary>
        /// <param name="aSignedTxData">The data for the server to broadcast</param>
        /// <returns>Returnes a SendTxHandle that can be used by IsTransactionSent function.</returns>
        public long SendTransaction(long aCurrencyId, string aSignedTxData)
        {
            CheckConnected();
            return (long) this.Invoke(new SendTransactionDelegate(ThreadSendTransaction), aCurrencyId, aSignedTxData);
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
            return (bool) this.Invoke(new IsTransactionSentDelegate(ThreadIsTransactionSent), aSendTxHandle);
        }

        public string GetTransactionId(long aSendTxHandle)
        {
            CheckConnected();
            return (string) this.Invoke(new GetTransactionIdDelegate(ThreadGetTransactionId), aSendTxHandle);
        }

        /// <summary>
        /// Returns the icon file with all sizes of the currency.
        /// </summary>
        /// <param name="aCurrencyId">The currency id of the Icon you wish to recieve</param>
        /// <returns>Byte array binary data of the ICO file.</returns>
        public string GetCurrencyIcon(long aCurrencyId)
        {
            CheckConnected();
            return (string) this.Invoke(new GetCurrencyIconDelegate(ThreadGetCurrencyIcon), aCurrencyId);
        }

        public long GetCurrencyTxFee(long aCurrencyId)
        {
            CheckConnected();
            return (long) this.Invoke(new GetCurrencyTxFeeDelegate(ThreadGetCurrencyTxFee), aCurrencyId);
        }

        public long GetBlockHeight(long aCurrencyId)
        {
            CheckConnected();
            return (long) this.Invoke(new GetBlockHeightDelegate(ThreadGetBlockHeight), aCurrencyId);
        }

        public bool CheckAddress(long aCurrencyId, string aAddress)
        {
            CheckConnected();
            return (bool) this.Invoke(new CheckAddressDelegate(ThreadCheckAddress), aCurrencyId, aAddress);
        }

        public string GetCurrency(long aCurrencyId)
        {
            CheckConnected();
            return (string) this.Invoke(new GetCurrencyDelegate(ThreadGetCurrency), aCurrencyId);
        }

        public string GetCurrencyToken(long aCurrencyId, string aTokenAddress)
        {
            CheckConnected();
            return (string) this.Invoke(new GetCurrencyTokenDelegate(ThreadGetCurrencyToken), aCurrencyId, aTokenAddress);
        }

        /// <summary>
        /// Returns a JSON text of the CurrencyStatusItem
        /// </summary>
        /// <param name="aCurrencyId"></param>
        /// <returns></returns>
        public string GetLastCurrencyStatus(long aCurrencyId)
        {
            CheckConnected();
            return (string) this.Invoke(new GetCurrencyDelegate(ThreadGetLastCurrencyStatus), aCurrencyId);
        }
    }
}