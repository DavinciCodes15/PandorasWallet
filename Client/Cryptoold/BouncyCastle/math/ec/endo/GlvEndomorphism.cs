namespace Pandora.Client.Crypto.Currencies.BouncyCastle.Math.EC.Endo
{
	internal interface GlvEndomorphism
		: ECEndomorphism
	{
		BigInteger[] DecomposeScalar(BigInteger k);
	}
}
