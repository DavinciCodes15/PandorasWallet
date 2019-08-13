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
using System.Runtime.Serialization;

namespace Pandora.Client.ClientLib
{
    [Serializable]
    public class PandoraServerException: Exception
    {
        public PandoraServerException(string msg) : base(msg) { }
        public PandoraServerException(SerializationInfo aSI, StreamingContext aContext) : base(aSI, aContext) { }
        public PandoraServerException(string aMessage, Exception aInner) : base(aMessage, aInner) { }
    }

    [Serializable]
    public class CryptoCurrencyServiceMissingException : PandoraServerException
    {
        public CryptoCurrencyServiceMissingException(string msg) : base(msg) { }
        public CryptoCurrencyServiceMissingException(SerializationInfo aSI, StreamingContext aContext) : base(aSI, aContext) { }
        public CryptoCurrencyServiceMissingException(string aMessage, Exception aInner) : base(aMessage, aInner) { }
    }

    [Serializable]
    public class PandoraServerDataException : PandoraServerException
    {
        public PandoraServerDataException(string msg) : base(msg) { }
        public PandoraServerDataException(SerializationInfo aSI, StreamingContext aContext) : base(aSI, aContext) { }
        public PandoraServerDataException(string aMessage, Exception aInner) : base(aMessage, aInner) { }
    }

    [Serializable]
    public class InvalidUserName : Exception
    {
        public InvalidUserName(string aMessage)
            : base(aMessage)
        {

        }
    }


}