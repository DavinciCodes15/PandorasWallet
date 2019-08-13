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

namespace Pandora.Client.SystemBackup
{
    public static class BackupExceptions
    {
        [Serializable]
        public class BadRecoveryPhrase : Exception
        {
            public BadRecoveryPhrase()
            {
            }

            public BadRecoveryPhrase(string message) : base(message)
            {
            }

            public BadRecoveryPhrase(string message, Exception innerException) : base(message, innerException)
            {
            }
        }

        [Serializable]
        public class MisspelledWord : Exception
        {
            public MisspelledWord()
            {
            }

            public MisspelledWord(string message) : base(message)
            {
            }

            public MisspelledWord(string message, Exception innerException) : base(message, innerException)
            {
            }
        }

        [Serializable]
        public class BadRecoveryFile : Exception
        {
            public BadRecoveryFile()
            {
            }

            public BadRecoveryFile(string message) : base(message)
            {
            }

            public BadRecoveryFile(string message, Exception innerException) : base(message, innerException)
            {
            }
        }
    }
}