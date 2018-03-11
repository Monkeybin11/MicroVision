using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroVision.Core.Models
{
    public class SerialCommand
    {
        public enum RpcSerialCommand
        {
            GetInfo,
            RequestPowerStatus,
            IsConnected,
            RequestCurrentStatus,
            RequestFocusStatus,
            RequestLaserStatus,
            RequestArmTrigger,
            RequestSoftwareReset,
        }
        public RpcSerialCommand Command { get; set; }

        public object Argument { get; set; }
    }
}
