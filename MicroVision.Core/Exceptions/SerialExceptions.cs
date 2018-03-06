using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroVision.Core.Exceptions
{
    public class ComListException : Exception
    {
        public ComListException(string message) : base(message)
        {
        }
    }
}
