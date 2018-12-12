namespace Pandora.Client.Crypto.Currencies
{
	/// <summary>
	/// Represent any type which represent an underlying ScriptPubKey
	/// </summary>
    public interface IDestination
	{
		Script ScriptPubKey
		{
			get;
		}
	}

    public interface ICryptoCurrencyString
    {
        Network Network
        {
            get;
        }
    }

}
