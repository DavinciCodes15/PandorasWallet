
namespace Pandora.Client.Crypto.Currencies.BIP32
{
	public interface IHDKey
	{
		IHDKey Derive(KeyPath keyPath);
		PubKey GetPublicKey();
		bool CanDeriveHardenedPath();
	}
}
