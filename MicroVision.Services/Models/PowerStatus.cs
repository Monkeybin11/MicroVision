using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace MicroVision.Services.Models
{
    public class PowerStatus : Status
    {
        private Boolean _isPowered;
        public Boolean IsPowered
        {
            get { return _isPowered; }
            set { SetProperty(ref _isPowered, value); }
        }
    }

    public class MasterPowerStatus : PowerStatus
    {
        public MasterPowerStatus()
        {
            Label = "Master";
        }
    }

    public class FanPowerStatus : PowerStatus
    {
        public FanPowerStatus()
        {
            Label = "Fan";
        }
    }
    public class LaserPowerStatus : PowerStatus
    {
        public LaserPowerStatus()
        {
            Label = "Laser";
        }
    }
    public class MotorPowerStatus : PowerStatus
    {
        public MotorPowerStatus()
        {
            Label = "Motor";
        }
    }
}
