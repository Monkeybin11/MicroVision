using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroVision.Core.Events;
using MicroVision.Services.Models;
using Prism.Events;
using Prism.Mvvm;

namespace MicroVision.Services
{
  
    public interface IStatusServices
    {
        ConnectionStatus VimbaConnectionStatus { get; }
        ConnectionStatus ComConnectionStatus { get; }
        PowerStatus MasterPowerStatus { get; }
        PowerStatus FanPowerStatus { get; }
        PowerStatus LaserPowerStatus { get; }
        PowerStatus MotorPowerStatus { get; }
        ValueStatus<double> CameraTemperatureValueStatus { get; }
        ValueStatus<double> CurrentValueStatus { get; }
    }

    public class StatusServices : BindableBase, IStatusServices
    {
        private readonly IEventAggregator _ea;
        private ConnectionStatus _vimbaConnectionStatus = new ConnectionStatus("Vimba");

        public ConnectionStatus VimbaConnectionStatus
        {
            get => _vimbaConnectionStatus;
            private set { SetProperty(ref _vimbaConnectionStatus, value); }
        }

        public ConnectionStatus ComConnectionStatus { get; } = new ConnectionStatus("COM");

        public PowerStatus MasterPowerStatus { get; } = new PowerStatus("Master");
        public PowerStatus FanPowerStatus { get; } = new PowerStatus("Fan");
        public PowerStatus LaserPowerStatus { get; } = new PowerStatus("Laser");
        public PowerStatus MotorPowerStatus { get; } = new PowerStatus("Motor");

        public ValueStatus<double> CameraTemperatureValueStatus { get; } = new ValueStatus<double>("Temperature (C)");
        public ValueStatus<double> CurrentValueStatus { get; } = new ValueStatus<double>("Current (A)");

        public StatusServices(IEventAggregator ea)
        {
            _ea = ea;
            _ea.GetEvent<ComConnectedEvent>().Subscribe(ComConnectedHandler);
            _ea.GetEvent<ComDisconnectedEvent>().Subscribe(ComDisconnectedHandler);

            _ea.GetEvent<VimbaConnectedEvent>().Subscribe(VimbaConnectionHandler);
            _ea.GetEvent<VimbaDisconnectedEvent>().Subscribe(VimbaDisconnectionHandler);
        }

        private void VimbaDisconnectionHandler()
        {
            VimbaConnectionStatus.SetConnected(false);
            VimbaConnectionStatus.ResetError();
        }

        private void VimbaConnectionHandler()
        {
            VimbaConnectionStatus.SetConnected(true);
            VimbaConnectionStatus.ResetError();
        }

        private void ComDisconnectedHandler(bool b)
        {
            ComConnectionStatus.SetConnected(false);
            ComConnectionStatus.ResetError();
        }

        private void ComConnectedHandler()
        {
            ComConnectionStatus.SetConnected(true);
            ComConnectionStatus.ResetError();
        }
    }

}