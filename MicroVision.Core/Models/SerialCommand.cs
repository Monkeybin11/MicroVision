using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroVision.Core.Models
{
    public class SerialCommand
    {
        public enum SerialCommands
        {
            GetInfo,
            SoftwareReset,
            PowerStatus,
            IsConnected,

        }
    }
}
