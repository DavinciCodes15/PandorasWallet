using System;

namespace Pandora.Client.Crypto.Currencies.BouncyCastle.Security
{
	internal class InvalidKeyException : KeyException
	{
		public InvalidKeyException() : base() { }
		public InvalidKeyException(string message) : base(message) { }
		public InvalidKeyException(string message, Exception exception) : base(message, exception) { }
	}
}
