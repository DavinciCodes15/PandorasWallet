namespace Pandora.Client.Crypto.Currencies.BouncyCastle.Math.EC.Endo
{
	internal interface ECEndomorphism
	{
		ECPointMap PointMap
		{
			get;
		}

		bool HasEfficientPointMap
		{
			get;
		}
	}
}
