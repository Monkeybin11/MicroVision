using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroVision.Core.Exceptions
{
    public class RpcServerConnectionException : Exception
    {
        public string ServiceName { get; }

        public RpcServerConnectionException(string serviceName, string message) : base(message)
        {
            ServiceName = serviceName;
        }
    }

    public class CameraControllerRpcServerConnectionException : RpcServerConnectionException
    {
        public CameraControllerRpcServerConnectionException(string message) : base("Camera Controller", message)
        {
        }
    }
}
