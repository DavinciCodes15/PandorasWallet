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