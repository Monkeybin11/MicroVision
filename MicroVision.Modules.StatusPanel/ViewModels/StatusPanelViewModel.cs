using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Practices.Unity;
using MicroVision.Services;
using MicroVision.Services.Models;

namespace MicroVision.Modules.StatusPanel.ViewModels
{
    public class StatusPanelViewModel : BindableBase
    {
        private readonly IUnityContainer _container;
        private readonly ILogService _logService;


        public StatusPanelViewModel(IUnityContainer container,IStatusServices statusService, ILogService logService)
        {
            _container = container;
            _logService = logService;
            _logService.ConfigureLogger("StatusPanel");

            ComConnectionStatus = statusService.ComConnectionStatus;
            VimbaConnectionStatus = statusService.VimbaConnectionStatus;

            MasterPowerStatus = statusService.MasterPowerStatus;
            FanPowerStatus = statusService.FanPowerStatus;
            MotorPowerStatus = statusService.MotorPowerStatus;
            LaserPowerStatus = statusService.LaserPowerStatus;

            CurrentValueStatus = statusService.CurrentValueStatus;
            CameraTemperatureValueStatus = statusService.CameraTemperatureValueStatus;

            Timer timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
            timer.Enabled = true;
            
        }

        public ConnectionStatus ComConnectionStatus { get; }
        public ConnectionStatus VimbaConnectionStatus { get; }
        public PowerStatus MasterPowerStatus { get; }
        public PowerStatus FanPowerStatus { get; }
        public PowerStatus MotorPowerStatus { get; }
        public PowerStatus LaserPowerStatus { get; }
        public ValueStatus<double> CurrentValueStatus { get; }
        public ValueStatus<double> CameraTemperatureValueStatus { get; }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (ComConnectionStatus.IsConnected)
            {
                ComConnectionStatus.IsConnected = false;
                VimbaConnectionStatus.ResetError();
                VimbaConnectionStatus.IsConnected = true;
            }
            else
            {
                ComConnectionStatus.IsConnected = true;
                VimbaConnectionStatus.RaiseError("Test Error!");
            }

            var status = (IStatusServices)_container.Resolve<StatusServices>();
            _logService.Logger.Info(status.ComConnectionStatus.IsConnected);


        }
    }
}
