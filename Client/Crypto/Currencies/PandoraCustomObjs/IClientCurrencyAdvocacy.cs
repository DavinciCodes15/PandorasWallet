

namespace Pandora.Client.Crypto.Currencies
{
    public interface ICryptoCurrencyAdvocacy
    {
        /// <summary>
        /// Name of the crypto currency
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Currency Id
        /// </summary>
        long Id { get; }

        /// <summary>
        /// This root key is the seed to regenerate everything from any currency placing the same RootKey
        /// will regenerate the same addresses every time.
        /// </summary>
        string GetRootSeed();

        /// <summary>
        /// Gets an address at an index an address at the RootKey is the same every time.
        /// </summary>
        /// <param name="aIndex">Index of the address you want</param>
        /// <returns>returns the address</returns>
        string GetAddress(long aIndex);

        /// <summary>
        /// Get sthe private key of each address if the KeyType is PublickKey.
        /// If the KeyType is ExtPublicKey and all indexes the address is the root Extended private key.
        /// </summary>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        string GetPrivateKey(long aIndex);

        /// <summary>
        /// Returns ture if the coin is a testnet coin.
        /// </summary>
        bool TestNet { get; }

        /// <summary>
        /// Signs the transaction data recived and returns the signed data and insures you are signing
        /// the right data.
        /// </summary>
        /// <param name="aTxData"></param>
        /// <param name="aValidationInfo">Insure you are signing the right data.</param>
        /// <returns></returns>
        string SignTransaction(string aTxData, ICurrencyTransaction aValidationInfo);
        
        /// <summary>
        /// Used to check if an address is a valid address.
        /// </summary>
        /// <param name="aAddress">Povide an address to test</param>
        /// <returns>Returns true if the address is valid</returns>
        bool IsValidAddress(string aAddress);
    }
}
