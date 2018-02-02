using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroVision.Services.Models;
using Prism.Mvvm;

namespace MicroVision.Services
{
  
    public interface IStatusServices
    {
        VimbaConnectionStatus VimbaConnectionStatus { get; }
        ComConnectionStatus ComConnectionStatus { get; }
        MasterPowerStatus MasterPowerStatus { get; }
        FanPowerStatus FanPowerStatus { get; }
        LaserPowerStatus LaserPowerStatus { get; }
        MotorPowerStatus MotorPowerStatus { get; }
        CameraTemperatureValueStatus CameraTemperatureValueStatus { get; }
        CurrentValueStatus CurrentValueStatus { get; }
    }

    public class StatusServices : IStatusServices
    {
        public VimbaConnectionStatus VimbaConnectionStatus { get; } = new VimbaConnectionStatus();
        public ComConnectionStatus ComConnectionStatus { get; } = new ComConnectionStatus();

        public MasterPowerStatus MasterPowerStatus { get; } = new MasterPowerStatus();
        public FanPowerStatus FanPowerStatus { get; } = new FanPowerStatus();
        public LaserPowerStatus LaserPowerStatus { get; } = new LaserPowerStatus();
        public MotorPowerStatus MotorPowerStatus { get; } = new MotorPowerStatus();

        public CameraTemperatureValueStatus CameraTemperatureValueStatus { get; } = new CameraTemperatureValueStatus();
        public CurrentValueStatus CurrentValueStatus { get; } = new CurrentValueStatus();

    }
}