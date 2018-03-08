using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroVision.Core.Exceptions
{
    public class CameraRpcServerConnectionException : Exception
    {
        public CameraRpcServerConnectionException(string message) : base(message)
        {
        }

    }

    public class CameraRuntimeException : Exception
    {
        public CameraRuntimeException(string message) : base(message)
        {
        }
    }
}
