using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Pandora.Client.Universal
{
    [Serializable]
    public class GenericException : Exception
    {
        public GenericException(string msg) : base(msg) { }
        public GenericException(SerializationInfo aSI, StreamingContext aContext) : base(aSI, aContext) { }
        public GenericException(string aMessage, Exception aInner) : base(aMessage, aInner) { }
    }

    [Serializable]
    public class AssignmentException : GenericException
    {
        public AssignmentException() : base("Invalid type assigned.") { }
        public AssignmentException(SerializationInfo aSI, StreamingContext aContext) : base(aSI, aContext) { }
        public AssignmentException(string aMessage, Exception aInner) : base(aMessage, aInner) { }
    }

    [Serializable]
    public class SystemLogMissingException : GenericException
    {
        public SystemLogMissingException() : base("Coding error SystemLong Missing in Pandora.Client.Universal.Log class.") { }
    }
}