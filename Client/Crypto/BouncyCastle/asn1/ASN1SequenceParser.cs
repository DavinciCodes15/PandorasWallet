namespace Pandora.Client.Crypto.Currencies.BouncyCastle.Asn1
{
	internal interface Asn1SequenceParser
		: IAsn1Convertible
	{
		IAsn1Convertible ReadObject();
	}
}
