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
            SoftwareReset,
            RequestPowerStatus,
            IsConnected,
            RequestCurrentStatus,
            RequestFocusStatus,
            RequestLaserStatus,
            RequestArmTrigger,
            RequestSoftwareReset,
        }
        public RpcSerialCommand Command { get; set; }

        public string Argument { get; set; }
    }
}
