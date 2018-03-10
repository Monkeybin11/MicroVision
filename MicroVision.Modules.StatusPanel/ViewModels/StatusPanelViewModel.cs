using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Media.Media3D;
using Microsoft.Practices.Unity;
using MicroVision.Core.Events;
using MicroVision.Services;
using MicroVision.Services.Models;
using Prism.Events;

namespace MicroVision.Modules.StatusPanel.ViewModels
{
    /// <summary>
    /// Status panel will actively pull status data from the services
    /// </summary>
    public class StatusPanelViewModel : BindableBase
    {
        private readonly IUnityContainer _container;
        private readonly IStatusServices _statusService;
        private readonly ILogService _logService;
        private readonly IEventAggregator _ea;
        private readonly ISerialService _serialService;
        private readonly ICameraService _cameraService;

        public IStatusServices Status => _statusService;

        public StatusPanelViewModel(IUnityContainer container,IStatusServices statusService, ILogService logService, IEventAggregator ea, ISerialService serialService, ICameraService cameraService)
        {
            _container = container;
            _statusService = statusService;
            _logService = logService;
            _ea = ea;
            _serialService = serialService;
            _cameraService = cameraService;
            _logService.ConfigureLogger("StatusPanel");

            _serialPowerStatusTimer = new Timer(_serialPowerStatusTimerInterval);
            _serialPowerStatusTimer.Elapsed += (sender, args) => SyncSerialPowerStatus();

            _serialCurrentStatusTimer = new Timer(_serialCurrentStatusTimerInterval);
            _serialCurrentStatusTimer.Elapsed += (sender, args) => SyncCurrentStatus();

            _syncCameraTemperatureTimer = new Timer(_syncCameraTemperatureTimerInterval);
            _syncCameraTemperatureTimer.Elapsed += (sender, args) => SyncCameraTemperature();

            _ea.GetEvent<ComConnectedEvent>().Subscribe(StartComStatusSynchronization);
            _ea.GetEvent<ComDisconnectedEvent>().Subscribe(StopComStatusSynchronization);

            _ea.GetEvent<VimbaConnectedEvent>().Subscribe(StartCameraStatusSynchronization);
            _ea.GetEvent<VimbaDisconnectedEvent>().Subscribe(StopCameraStatusSynchronization);
            _ea.GetEvent<ShutDownEvent>().Subscribe(Shutdown);
        }

        private void Shutdown()
        {
            StopCameraStatusSynchronization();
            StopComStatusSynchronization();
        }

        private void StopCameraStatusSynchronization()
        {

            _syncCameraTemperatureTimer.Stop();
        }

        private void StartCameraStatusSynchronization()
        {
            
            _syncCameraTemperatureTimer.Start();
        }

        private void StopComStatusSynchronization(bool obj = false)
        {
            _serialCurrentStatusTimer.Stop();
            _serialPowerStatusTimer.Stop();
        }

        private void StartComStatusSynchronization()
        {
            _serialCurrentStatusTimer.Start();
            _serialPowerStatusTimer.Start();
        }

        private double _serialPowerStatusTimerInterval = 1000;
        private Timer _serialPowerStatusTimer;
        public void SyncSerialPowerStatus()
        {
            bool master, fan, motor, laser;
            master = fan = motor = laser = false;
            try
            {
                SerialService.ParsePowerCode(_serialService.ReadPower(), out master, out fan, out motor, out laser);

            }
            catch (Exception e)
            {
                //ignored 
            }
            Status.FanPowerStatus.IsPowered = fan;
            Status.MasterPowerStatus.IsPowered = master;
            Status.MotorPowerStatus.IsPowered = motor;
            Status.LaserPowerStatus.IsPowered = laser;
        }

        private double _serialCurrentStatusTimerInterval = 500;
        private Timer _serialCurrentStatusTimer;
        public void SyncCurrentStatus()
        {
            try
            {
                Status.CurrentValueStatus.Value = _serialService.GetCurrent();
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        private double _syncCameraTemperatureTimerInterval = 1000;
        private Timer _syncCameraTemperatureTimer;
        public void SyncCameraTemperature()
        {
            try
            {
                Status.CameraTemperatureValueStatus.Value = _cameraService.GetTemperature();
            }
            catch (Exception e)
            {
                // ignored
            }
        }
    }
}
