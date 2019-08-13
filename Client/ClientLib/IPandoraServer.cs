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


namespace Pandora.Client.ClientLib
{
    public delegate void TransactionEvent(long[] aIds, bool isConfirmationUpdate);

    public delegate void CurrencyStatusEvent(long[] aIds);

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
        CurrencyItem GetCurrency(long aCurrencyId);

        /// <summary>
        /// Provide an array of one currency Id's to get the aditional status information
        /// about coins.
        /// </summary>
        /// <param name="aCurrencyIdArray"></param>
        /// <returns></returns>
        CurrencyStatusItem GetCurrencyStatus(long aCurrencyId);

        /// <summary>
        /// use this object to add remove and find monitored accounts.  Currency accounts in this list will be monitored and if
        /// transactions occur on the account the OnTransactions Event will be fired to notify the client app of new transactions.
        /// </summary>
        //CurrencyAccountList MonitoredAccounts { get; }

        /// <summary>
        /// Call this method to get all transactions then adds it to MonitoredAccounts.
        /// </summary>
        /// <param name="aCurrencyId">Currency id of the address you want the transaction</param>
        /// <param name="aAddress">Address of all Transations</param>
        TransactionRecord[] GetTransactions(long aCurrencyId, string aAddress);

        /// <summary>
        /// Creates a send transaction data string to be signed on the client side.  If the inputs or outputs are incorrect an exception orccers.
        /// </summary>
        /// <param name="aCurrencyId">Currency id of the address you want the transaction</param>
        /// <param name="aSendTx"></param>
        /// <returns></returns>
        ulong GetTransactionFee(long aCurrencyId, CurrencyTransaction aCurrencyTransaction);

        /// <summary>
        /// Creates a send transaction data string to be signed on the client side.  If the inputs or outputs are incorrect an exception orccers.
        /// </summary>
        /// <param name="aCurrencyId">Currency id of the address you want the transaction</param>
        /// <param name="aSendTx"></param>
        /// <returns></returns>
        string CreateTransaction(long aCurrencyID, CurrencyTransaction aSendTx);

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
        byte[] GetCurrencyIcon(long aCurrencyId);

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