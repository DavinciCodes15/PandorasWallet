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
using System;

namespace Pandora.Client.PandorasWallet.Wallet
{
    public static class ClientExceptions
    {
        public class InvalidOperationException : Exception
        {
            public InvalidOperationException(string message)
                : base(message)
            {
            }
        }

        public class EncryptedKeysException : Exception
        {
            public EncryptedKeysException(string message)
                : base(message)
            {
            }
        }

        public class ExchangeException : Exception
        {
            public ExchangeException(string message) : base(message)
            {
            }
        }

        public class ConnectionTimeoutException : Exception
        {
            public ConnectionTimeoutException(string message)
                : base(message)
            {
            }
        }

        public class WalletCorruptionException : Exception
        {
            public WalletCorruptionException(string message) : base(message)
            {
            }
        }

        public class WrongPasswordException : Exception
        {
            public WrongPasswordException(string message)
                : base(message)
            {
            }
        }

        public class InvalidAddressException : Exception
        {
            public InvalidAddressException(string message)
                : base(message)
            {
            }
        }

        public class SettingsFailureException : Exception
        {
            public SettingsFailureException(string message)
                : base(message)
            {
            }
        }

        public class InvalidSeedException : Exception
        {
            public InvalidSeedException(string message)
                : base(message)
            {
            }
        }

        public class AddressSynchException : Exception
        {
            public AddressSynchException(string message)
                : base(message)
            {
            }
        }

        public class UserNotActiveException : Exception
        {
            public UserNotActiveException(string message, DateTime aDate)
                : base(message)
            {
                Data.Add("message", message);
                Data.Add("statustime", aDate);
            }
        }

        public class InsufficientFundsException : Exception
        {
            public InsufficientFundsException(string message)
                : base(message)
            {
            }
        }

        public class MisspelledWord : Exception
        {
            public MisspelledWord(string message)
                : base(message)
            {
            }
        }
    }
}