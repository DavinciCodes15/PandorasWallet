using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Client.Universal.Threading
{
    public class MethodExecutedException : GenericException
    {
        public MethodExecutedException(string message) : base(message) { }
    }
    public class DevErrorException : GenericException
    {
        public DevErrorException(string message) : base(message) { }
    }
}
