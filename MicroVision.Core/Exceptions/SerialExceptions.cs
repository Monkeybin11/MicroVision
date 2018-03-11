using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroVision.Core.Exceptions
{
    public class ComRuntimeException : Exception
    {
        public ComRuntimeException(string message) : base(message)
        {
        }
    }

    public class ComListException : ComRuntimeException
    {
        public ComListException(string message) : base(message)
        {
        }
    }
}
