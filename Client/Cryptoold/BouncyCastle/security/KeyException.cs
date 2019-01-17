using System;

namespace Pandora.Client.Crypto.Currencies.BouncyCastle.Security
{
	internal class KeyException : GeneralSecurityException
	{
		public KeyException() : base() { }
		public KeyException(string message) : base(message) { }
		public KeyException(string message, Exception exception) : base(message, exception) { }
	}
}
