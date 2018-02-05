using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Practices.Unity;
using MicroVision.Core.Events;
using MicroVision.Services;
using MicroVision.Services.Models;
using Prism.Events;

namespace MicroVision.Modules.StatusPanel.ViewModels
{
    public class StatusPanelViewModel : BindableBase
    {
        private readonly IUnityContainer _container;
        private readonly ILogService _logService;
        private readonly IEventAggregator _ea;


        public StatusPanelViewModel(IUnityContainer container,IStatusServices statusService, ILogService logService, IEventAggregator ea)
        {
            _container = container;
            _logService = logService;
            _ea = ea;
            _logService.ConfigureLogger("StatusPanel");

            ComConnectionStatus = statusService.ComConnectionStatus;
            VimbaConnectionStatus = statusService.VimbaConnectionStatus;

            MasterPowerStatus = statusService.MasterPowerStatus;
            FanPowerStatus = statusService.FanPowerStatus;
            MotorPowerStatus = statusService.MotorPowerStatus;
            LaserPowerStatus = statusService.LaserPowerStatus;

            CurrentValueStatus = statusService.CurrentValueStatus;
            CameraTemperatureValueStatus = statusService.CameraTemperatureValueStatus;

            _ea.GetEvent<ComConnectedEvent>().Subscribe(ComConnectedHandler);
        }

        private void ComConnectedHandler()
        {
            ComConnectionStatus.IsConnected = true;
            ComConnectionStatus.IsError = false;
        }

        public ConnectionStatus ComConnectionStatus { get; }
        public ConnectionStatus VimbaConnectionStatus { get; }
        public PowerStatus MasterPowerStatus { get; }
        public PowerStatus FanPowerStatus { get; }
        public PowerStatus MotorPowerStatus { get; }
        public PowerStatus LaserPowerStatus { get; }
        public ValueStatus<double> CurrentValueStatus { get; }
        public ValueStatus<double> CameraTemperatureValueStatus { get; }      
    }
}
