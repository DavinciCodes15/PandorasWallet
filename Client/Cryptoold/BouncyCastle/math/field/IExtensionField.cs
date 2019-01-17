﻿namespace Pandora.Client.Crypto.Currencies.BouncyCastle.Math.Field
{
	internal interface IExtensionField
		: IFiniteField
	{
		IFiniteField Subfield
		{
			get;
		}

		int Degree
		{
			get;
		}
	}
}
