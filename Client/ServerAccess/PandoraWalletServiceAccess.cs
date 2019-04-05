﻿using Pandora.Client.ClientLib;
using Pandora.Client.Universal;
using Pandora.Client.Universal.Threading;
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
        private delegate string GetCurrencyListDelegate(uint aStartId);
        private delegate string GetCurrencyStatusListDelegate(ulong aCurrencyId, long aStartId);
        private delegate void AddMonitoredAccountDelegate(ulong aCurrencyId, string aAddress);
        private delegate string GetMonitoredAcccountsDelegate(ulong aCurrencyId, ulong aStartCurrencyAccountId);
        private delegate bool RemoveMonitoredAcccountsDelegate(ulong aCurrencyAccountId);
        private delegate string GetTransactionRecordsDelegate(uint aCurrencyId, ulong aStartTxRecordId);
        private delegate string CreateTransactionDelegate(CurrencyTransaction aCurrencyTransaction);
        private delegate long SendTransactionDelegate(ulong aCurrencyId, string aSignedTxData);
        private delegate bool IsTransactionSentDelegate(long aSendTxHandle);
        private delegate string GetTransactionIdDelegate(long aSendTxHandle);
        private delegate string GetCurrencyIconDelegate(uint aCurrencyId);
        private delegate ulong GetBlockHeightDelegate(uint aCurrencyId);
        private delegate bool CheckAddressDelegate(uint aCurrencyId, string aAddress);
        private delegate string GetCurrencyDelegate(uint aCurrencyId);


        private string FConnectionId;
        private PandoraWalletWebService FServer;
        private PandoraWalletWebService FPandoraWalletWebService;
        private SynchronizationContext FSyncContext;
        private string ConnectionId { get { lock (this) { return FConnectionId; } } }

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
            FConnectionId = aConnectionId;
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
            Binding lBinding = new BasicHttpBinding();
            if (EncryptedConnection)
                lBinding = new BasicHttpsBinding();

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
            catch(Exception ex)
            {
                if (ex.Message.Contains("Connection Expired."))
                    InternalDisconnect();
                throw;
            }
        }

        private object ReadServerResult(Pandora.Client.ServerAccess.PandoraWalletService1_1.PandoraResult aServerResult)
        {
            if (!string.IsNullOrEmpty(aServerResult.ErrorMsg)) // server sside error occured.
                throw new PandoraServerException("Server Error: " + aServerResult.ErrorMsg);
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

        // This method is overrdden so that we can try reconnecting when ever the ThreadXXX method is called
        // so we can do what ever w can to recover from bad connections or if the one server 
        // goes down for maintainane 
        protected override bool InvokeMethodMessage(DelegateMessage aMethodMessage)
        {
            aMethodMessage.OnAfterEvent += MethodMessage_OnAfterEvent;
            
            return base.InvokeMethodMessage(aMethodMessage);
        }

        private void MethodMessage_OnAfterEvent(object sender, EventArgs e)
        {
            DelegateMessage lMethodMessage = sender as DelegateMessage;
           
            if (lMethodMessage.ExceptionObject != null ) // do not try reconnect if the logon method is called
            {
                /// if we cant connet to the server because its down 
                if (lMethodMessage.ExceptionObject is System.ServiceModel.EndpointNotFoundException)
                {
                    Log.Write(LogLevel.Error, lMethodMessage.ExceptionObject.Message);
                    lMethodMessage.ExceptionObject = new PandoraServiceError("Internet connection error occured while connecting to the remote server.");
                }
                var lName = lMethodMessage.ToString();

                // an exception occured
                // Now test if the error is because of a communication issue or 
                // if the error is because the server sent the error to be thrown
                if (!(lMethodMessage.ExceptionObject is PandoraServerException) && lName != "ThreadLogon")
                    // if there is a comunication faild we need to reconnect
                    try
                    {
                        FServer = CreatePandoraWalletServer();
                    }
                    catch
                    {
                        // if we fail to reconnect to the server no problem
                    }
            }
        }

        private bool ThreadLogon(string aEmail, string aUserName, string aPassword)
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
                PandoraWalletService1_1.PandoraResult lServerResult = FServer.Logon(aEmail, aUserName, aPassword);
                if (!string.IsNullOrEmpty(lServerResult.ErrorMsg))
                    throw new PandoraServerException("Server Error: " + lServerResult.ErrorMsg);
                FConnectionId = (string)lServerResult.result;
                Username = aUserName;
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

        public DateTime LastConnected { get; private set; }

        private bool ThreadLogon2(string aEmail, string aUserName, string aPassword, string aVersion)
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
                var lServerResult = FServer.Logon2(aEmail, aUserName, aPassword, aVersion);
                LastConnected = lServerResult.LastConnected;
                if (!string.IsNullOrEmpty(lServerResult.ErrorMsg))
                    throw new PandoraServerException("Server Error: " + lServerResult.ErrorMsg);
                FConnectionId = (string)lServerResult.result;
                Username = aUserName;
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
        
        private string ThreadGetCurrencyList(uint aStartId)
        {
            return (string)ReadServerResult(FServer.GetCurrencyList(FConnectionId, aStartId));
        }

        private string ThreadGetCurrencyStatusList(ulong aCurrencyId, long aStartId)
        {
            return (string)ReadServerResult(FServer.GetCurrencyStatusList(FConnectionId, (long)aCurrencyId, aStartId));
        }
        private void ThreadAddMonitoredAccount(ulong aCurrencyId, string aAddress)
        {
            ReadServerResult(FServer.AddMonitoredAccount(ConnectionId, (long)aCurrencyId, aAddress));
        }

        private string ThreadGetMonitoredAcccounts(ulong aCurrencyId, ulong aStartCurrencyAccountId)
        {
            return (string)ReadServerResult(FServer.GetMonitoredAcccounts(ConnectionId, (long)aCurrencyId, (long)aStartCurrencyAccountId));
        }

        private bool ThreadRemoveMonitoredAcccounts(ulong aCurrencyAccountId)
        {
            return (bool)ReadServerResult(FServer.RemoveMonitoredAccount(ConnectionId, (long)aCurrencyAccountId));
        }

        private string ThreadGetTransactionRecords(uint aCurrencyId, ulong aStartTxRecordId)
        {
            return (string)ReadServerResult(FServer.GetTransactionRecords(ConnectionId, (long)aCurrencyId, (long)aStartTxRecordId));
        }

        private string ThreadCreateTransaction(CurrencyTransaction aCurrencyTransaction)
        {
            string lTxData = aCurrencyTransaction.ToString();
            long lHandle = (long)ReadServerResult(FServer.StartGetTransactionToSign(ConnectionId, lTxData));
            do
            {
                System.Threading.Thread.Sleep(1000);
                lTxData = (string)ReadServerResult(FServer.EndGetTransactionToSign(ConnectionId, lHandle));
            } while (string.IsNullOrEmpty(lTxData));
            return lTxData;
        }

        private long ThreadSendTransaction(ulong aCurrencyId, string aSignedTxData)
        {
            return (long)ReadServerResult(FServer.SendTransaction(ConnectionId, (long)aCurrencyId, aSignedTxData));
        }

        private bool ThreadIsTransactionSent(long aSendTxHandle)
        {
            return (bool)ReadServerResult(FServer.IsTransactionSent(ConnectionId, aSendTxHandle));
        }

        private string ThreadGetTransactionId(long aSendTxHandle)
        {
            return (string)ReadServerResult(FServer.GetTransactionId(ConnectionId, aSendTxHandle));
        }

        private string ThreadGetCurrencyIcon(uint aCurrencyId)
        {
            return (string)ReadServerResult(FServer.GetCurrencyIcon(ConnectionId, aCurrencyId));
        }

        private ulong ThreadGetBlockHeight(uint aCurrencyId)
        {
            return Convert.ToUInt64(ReadServerResult(FServer.GetBlockHeight(ConnectionId, aCurrencyId)));
        }

        private string ThreadGetCurrency(uint aCurrencyId)
        {
            var lResult = FServer.GetCurrency(FConnectionId, aCurrencyId);
            return (string)ReadServerResult(lResult);
        }

        public bool ThreadCheckAddress(uint aCurrencyId, string aAddress)
        {
            return (bool)ReadServerResult(FServer.CheckAddressValidity(ConnectionId, aCurrencyId, aAddress));
        }

        private string ThreadGetLastCurrencyStatus(uint aCurrencyId)
        {
            var lResult = FServer.GetLastCurrencyStatus(FConnectionId, aCurrencyId);
            return (string)ReadServerResult(lResult);
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
        public bool Logon2(string aEmail, string aUserName, string aPassword, string aVersion)
        {
            return (bool)this.Invoke(new Logon2Delegate(ThreadLogon2), aEmail, aUserName, aPassword, aVersion);
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

        public string GetUserStatus()
        {
            return (string)ReadServerResult(FServer.GetUserStatus(ConnectionId));
        }

        /// <summary>
        /// Returns a Json Array of longs
        /// </summary>
        /// <param name="aStartId"></param>
        /// <returns></returns>
        public string GetCurrencyList(uint aStartId)
        {
            CheckConnected();
            return (string)this.Invoke(new GetCurrencyListDelegate(ThreadGetCurrencyList), aStartId);
        }

        /// <summary>
        /// Gets the status of the currency
        /// </summary>
        public string GetCurrencyStatusList(ulong aCurrencyId, long aStartId)
        {
            CheckConnected();
            return (string)this.Invoke(new GetCurrencyStatusListDelegate(ThreadGetCurrencyStatusList), aCurrencyId, aStartId);
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
            this.Invoke(new AddMonitoredAccountDelegate(ThreadAddMonitoredAccount), aCurrencyId, aAddress);
        }

        /// <summary>
        /// Returns a list of address the severs is monitoring for transactions that may occur.
        /// </summary>
        /// <param name="aCurrencyId">The currency id of the Monitored address</param>
        /// <returns>returns an array of CurrencyAccount</returns>
        public string GetMonitoredAcccounts(ulong aCurrencyId, ulong aStartCurrencyAccountId)
        {
            CheckConnected();
            return (string)this.Invoke(new GetMonitoredAcccountsDelegate(ThreadGetMonitoredAcccounts), aCurrencyId, aStartCurrencyAccountId);
        }


        public bool RemoveMonitoredAcccounts(ulong aCurrencyAccountId)
        {
            CheckConnected();
            return (bool)this.Invoke(new RemoveMonitoredAcccountsDelegate(ThreadRemoveMonitoredAcccounts), aCurrencyAccountId);
        }

        public string GetTransactionRecords(uint aCurrencyId, ulong aStartTxRecordId)
        {
            CheckConnected();
            return (string)this.Invoke(new GetTransactionRecordsDelegate(ThreadGetTransactionRecords), aCurrencyId, aStartTxRecordId);
        }

        /// <summary>
        /// Creates the transaction data to be signed.
        /// </summary>
        /// <param name="aTransactionData">This is a CurrencyTransaction object.</param>
        /// <returns>returns the data that needs to be signed</returns>
        public string CreateTransaction(CurrencyTransaction aCurrencyTransaction)
        {
            CheckConnected();
            return (string)this.Invoke(new CreateTransactionDelegate(ThreadCreateTransaction), aCurrencyTransaction);
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
            return (long)this.Invoke(new SendTransactionDelegate(ThreadSendTransaction), aCurrencyId, aSignedTxData);
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
            return (bool)this.Invoke(new IsTransactionSentDelegate(ThreadIsTransactionSent), aSendTxHandle);
        }

        public string GetTransactionId(long aSendTxHandle)
        {
            CheckConnected();
            return (string)this.Invoke(new GetTransactionIdDelegate(ThreadGetTransactionId), aSendTxHandle);
        }

        /// <summary>
        /// Returns the icon file with all sizes of the currency.
        /// </summary>
        /// <param name="aCurrencyId">The currency id of the Icon you wish to recieve</param>
        /// <returns>Byte array binary data of the ICO file.</returns>
        public string GetCurrencyIcon(uint aCurrencyId)
        {
            CheckConnected();
            return (string)this.Invoke(new GetCurrencyIconDelegate(ThreadGetCurrencyIcon), aCurrencyId);
        }

        public ulong GetBlockHeight(uint aCurrencyId)
        {
            CheckConnected();
            return (ulong)this.Invoke(new GetBlockHeightDelegate(ThreadGetBlockHeight), aCurrencyId);
        }

        public bool CheckAddress(uint aCurrencyId, string aAddress)
        {
            CheckConnected();
            return (bool)this.Invoke(new CheckAddressDelegate(ThreadCheckAddress), aCurrencyId, aAddress);
        }

        public string GetCurrency(uint aCurrencyId)
        {
            CheckConnected();
            return (string)this.Invoke(new GetCurrencyDelegate(ThreadGetCurrency), aCurrencyId);
        }

        /// <summary>
        /// Returns a JSON text of the CurrencyStatusItem
        /// </summary>
        /// <param name="aCurrencyId"></param>
        /// <returns></returns>
        public string GetLastCurrencyStatus(uint aCurrencyId)
        {
            CheckConnected();
            return (string)this.Invoke(new GetCurrencyDelegate(ThreadGetLastCurrencyStatus), aCurrencyId);
        }

    }
}