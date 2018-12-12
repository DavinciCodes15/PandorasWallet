namespace Pandora.Client.ClientLib
{
    public delegate void TransactionEvent(uint[] aIds, bool isConfirmationUpdate);

    public delegate void CurrencyStatusEvent(uint[] aIds);

    public interface IPandoraServer
    {
        /// <summary>
        /// Logs on to system
        /// </summary>
        /// <param name="aEmail"></param>
        /// <param name="aUserName"></param>
        /// <param name="aPassword"></param>
        /// <returns></returns>
        bool Logon(string aEmail, string aUserName, string aPassword);

        /// <summary>
        /// Name of user last connected or currently connected.  If no user was connected the result is null.
        /// </summary>
        string Username { get; }

        /// <summary>
        /// Email address of the connected user or last connected user.
        /// </summary>
        string Email { get; }

        /// <summary>
        /// If the object is connected to a pandora server with correct username and password this returns true
        /// Should the server fail to maintain connection this becomes false.
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Logs the current user off and turns the connected to false.
        /// </summary>
        /// <returns>Returns true only if the user is currently connected and is disconnected.</returns>
        bool Logoff();

        /// <summary>
        /// The fuction obtains a list of supported currencies.
        /// </summary>
        /// <returns>returns an array of all currencies on the server.</returns>
        CurrencyItem[] GetCurrencyList();

        /// <summary>
        /// Gets the a currency by ID if it exists if not throws an exception error.
        /// </summary>
        /// <param name="aCurrencId"></param>
        /// <returns></returns>
        CurrencyItem GetCurrency(uint aCurrencyId);

        /// <summary>
        /// Provide an array of one currency Id's to get the aditional status information
        /// about coins.
        /// </summary>
        /// <param name="aCurrencyIdArray"></param>
        /// <returns></returns>
        CurrencyStatusItem GetCurrencyStatus(uint aCurrencyId);

        /// <summary>
        /// use this object to add remove and find monitored accounts.  Currency accounts in this list will be monitored and if
        /// transactions occur on the account the OnTransactions Event will be fired to notify the client app of new transactions.
        /// </summary>
        ICurrencyAccountList MonitoredAccounts { get; }

        /// <summary>
        /// Call this method to get all transactions then adds it to MonitoredAccounts.
        /// </summary>
        /// <param name="aCurrencyId">Currency id of the address you want the transaction</param>
        /// <param name="aAddress">Address of all Transations</param>
        TransactionRecord[] GetTransactions(uint aCurrencyId, string aAddress);

        /// <summary>
        /// Gets Extedended information of the currency's transanaction beyond just inputs and outputs.  The infromation is provided in Json format
        /// for the client app to display.
        /// </summary>
        /// <param name="aCurrencyId">The Id of the currency the tx belongs to.</param>
        /// <param name="aTxId">Transaction Id of the the transaction.</param>
        /// <returns>Returns a Json Script with the infromation about the transaction</returns>
        //string GetExtendedTransactionInfo(uint aCurrencyId, string aTxId);

        /// <summary>
        /// Creates a send transaction data string to be signed on the client side.  If the inputs or outputs are incorrect an exception orccers.
        /// </summary>
        /// <param name="aCurrencyId">Currency id of the address you want the transaction</param>
        /// <param name="aSendTx"></param>
        /// <returns></returns>
        ulong GetTransactionFee(uint aCurrencyId, CurrencyTransaction aCurrencyTransaction);

        /// <summary>
        /// Creates a send transaction data string to be signed on the client side.  If the inputs or outputs are incorrect an exception orccers.
        /// </summary>
        /// <param name="aCurrencyId">Currency id of the address you want the transaction</param>
        /// <param name="aSendTx"></param>
        /// <returns></returns>
        string CreateTransaction(uint aCurrencyID, CurrencyTransaction aSendTx);

        /// <summary>
        /// Sends the transaction if no errors then a Transaction event will fire
        /// if the transaction occured.
        /// </summary>
        /// <param name="aSignedTxData"></param>
        long SendTransaction(ulong aCurrencyId, string aSignedTxData);

        /// <summary>
        /// Returns the icon file with all sizes of the currency.
        /// </summary>
        /// <param name="aCurrencyId">The currency id of the Icon you wish to recieve</param>
        /// <returns></returns>
        byte[] GetCurrencyIcon(uint aCurrencyId);

        /// <summary>
        /// Download the currency interface code for creating addresses and keys
        /// </summary>
        /// <param name="aCurrencyId"></param>
        /// <param name="aLibVersion"></param>
        /// <returns></returns>
        //byte[] GetCurrencyLib(uint aCurrencyId, string aLibVersion);

        /// <summary>
        /// The path that this code will store cache data because the code
        /// assums the end user will request fixed data the back will not access the sever
        /// every time and look up data found in its local database.
        /// NOTE: this folder can be shared with diffrent instances of this object.
        /// Default is %appdata%\PandorasWallet
        /// or ~/.PandorasWallet
        /// </summary>
        string DataPath { get; }

        /// <summary>
        /// Returns a userstatus object that contais data related with the user on the current connection id
        /// </summary>
        UserStatus GetUserStatus();
    }
}