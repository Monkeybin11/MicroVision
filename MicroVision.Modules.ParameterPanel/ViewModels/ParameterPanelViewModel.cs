﻿using System;
using System.Threading.Tasks;
using System.Timers;
using MicroVision.Core.Events;
using Prism.Commands;
using Prism.Mvvm;
using MicroVision.Core.Models;
using MicroVision.Services;
using MicroVision.Services.Models;
using Prism.Events;
using Services;

namespace MicroVision.Modules.ParameterPanel.ViewModels
{
    public class ParameterPanelViewModel : BindableBase
    {
        public ParameterPanelViewModel(
            ISerialService serialService,
            ICameraService cameraService,
            IParameterServices param,
            IStatusServices statusService,
            ICaptureService captureService,
            IEventAggregator eventAggregator)
        {
            _serialService = serialService;
            _cameraService = cameraService;
            _captureService = captureService;
            Status = statusService;
            _eventAggregator = eventAggregator;
            Params = param;

            // ask for list update for initial value
            // _eventAggregator.GetEvent<ComListUpdateRequestedEvent>().Publish();

            Params.ManualPowerCheck.PropertyChanged += ManualPowerCheck_PropertyChanged;


            // restore remote configuration
            SyncRemoteSerialConfiguration();

            CameraInitialization();
        }

        private void CameraInitialization()
        {
            _cameraService.VimbaInstanceControl(ConnectionCommands.Connect);
        }

        private readonly ISerialService _serialService;
        private readonly ICameraService _cameraService;
        private readonly ICaptureService _captureService;

        private readonly IEventAggregator _eventAggregator;

        #region properties

        #region parameter properties

        public IParameterServices Params { get; }

        #endregion

        #region status properties

        public IStatusServices Status { get; }

        #endregion

        #endregion

        public void SyncRemoteSerialConfiguration()
        {
            try
            {
                if (_serialService.IsConnected()) Status.ComConnectionStatus.SetConnected(true);

                var powerCode = _serialService.ReadPower();
                bool master, fan, laser, motor;
                SerialService.ParsePowerCode(powerCode, out master, out fan, out motor, out laser);
                Params.MasterPowerCheck.Value = master;
                Params.FanPowerCheck.Value = fan;
                Params.MotorPowerCheck.Value = motor;
                Params.LaserPowerCheck.Value = laser;
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        #region Commands
        private DelegateCommand _captureCommand;
        public DelegateCommand CaptureCommand =>
            _captureCommand ?? (_captureCommand = new DelegateCommand(ExecuteCaptureCommand));

        void ExecuteCaptureCommand()
        {
            if (_captureService.Capturing)
            {
                _captureService.Stop();
                _eventAggregator.GetEvent<StopCaptureEvent>().Publish();
            }
            else
            {
                _captureService.Capture(1000, -1);
                _eventAggregator.GetEvent<StartCaptureEvent>().Publish();
            }
        }

        private DelegateCommand _disconnectAllCommand;
        public DelegateCommand DisconnectAllCommand =>
            _disconnectAllCommand ?? (_disconnectAllCommand = new DelegateCommand(ExecuteDisconnectAllCommand));

        void ExecuteDisconnectAllCommand()
        {
            if (Status.VimbaConnectionStatus.IsConnected)
            {
                CameraConnectToggleCommand.Execute();
            }

            if (Status.ComConnectionStatus.IsConnected)
            {
                ComConnectToggleCommand.Execute();
            }
        }

        private DelegateCommand _connectAllCommand;
        public DelegateCommand ConnectAllCommand =>
            _connectAllCommand ?? (_connectAllCommand = new DelegateCommand(ExecuteConnectAllCommand));

        void ExecuteConnectAllCommand()
        {
            if (!Status.VimbaConnectionStatus.IsConnected)
            {
                CameraConnectToggleCommand.Execute();
            }

            if (!Status.ComConnectionStatus.IsConnected)
            {
                ComConnectToggleCommand.Execute();
            }
        }
        private DelegateCommand<bool?> _powerConfigurationCommand;

        public DelegateCommand<bool?> PowerConfigurationCommand =>
            _powerConfigurationCommand ??
            (_powerConfigurationCommand =
                new DelegateCommand<bool?>(ExecutePowerConfigurationCommand, CanPowerConfigurationExecution))
            .ObservesProperty(() => Status.ComConnectionStatus.IsConnected)
            .ObservesProperty(() => Params.ManualPowerCheck.Value);

        private DelegateCommand _comConnectToggleCommand;

        public DelegateCommand ComConnectToggleCommand =>
            _comConnectToggleCommand ??
            (_comConnectToggleCommand =
                new DelegateCommand(ExecuteComConnectToggleCommand, CanExecuteComConnectToggleCommand))
            .ObservesProperty(() => Params.ComSelection.Selected);

        private DelegateCommand _comUpdateListCommand;

        public DelegateCommand ComUpdateListCommand =>
            _comUpdateListCommand ?? (_comUpdateListCommand = new DelegateCommand(ExecuteComUpdateListCommand));

        private DelegateCommand<string> _focusCommand;

        public DelegateCommand<string> FocusCommand =>
            _focusCommand ?? (_focusCommand =
                new DelegateCommand<string>(ExecuteFocusCommand, CanComOperationExecution).ObservesProperty(() =>
                    Status.ComConnectionStatus.IsConnected));


        private DelegateCommand _cameraUpdateListCommand;

        public DelegateCommand CameraUpdateListCommand =>
            _cameraUpdateListCommand ??
            (_cameraUpdateListCommand = new DelegateCommand(ExecuteCameraUpdateListCommand));

        private DelegateCommand _cameraConnectToggleCommand;

        public DelegateCommand CameraConnectToggleCommand =>
            _cameraConnectToggleCommand ?? (_cameraConnectToggleCommand =
                new DelegateCommand(ExecuteCameraConnectToggleCommand, CanCameraConnectToggleExecuteMethod)
                    .ObservesProperty(() => Params.VimbaSelection.Selected));

        void ExecuteCameraConnectToggleCommand()
        {
            if (Status.VimbaConnectionStatus.IsConnected)
            {
                Task.Run(() => _cameraService.Disconnect());
            }
            else
            {
                if (Params.VimbaSelection.Selected != null)
                    Task.Run(() => _cameraService.Connect(Params.VimbaSelection.Selected));
            }
        }

        void ExecuteCameraUpdateListCommand()
        {
            Task.Run(() => Params.VimbaSelection.Value = _cameraService.CameraUpdateList());
        }

        void ExecutePowerConfigurationCommand(bool? b)
        {
            Task.Run(() => _serialService.ControlPower(Params.MasterPowerCheck.Value, Params.FanPowerCheck.Value,
                Params.MotorPowerCheck.Value, Params.LaserPowerCheck.Value));
        }

        void ExecuteComConnectToggleCommand()
        {
            Task.Run(() =>
            {
                if (Status.ComConnectionStatus.IsConnected) // already connected 
                {
                    //_eventAggregator.GetEvent<ComDisconnectionRequestedEvent>().Publish();
                    _serialService.Disconnect();
                }
                else
                {
                    var selectedSerialPort = Params.ComSelection.Selected;
                    //_eventAggregator.GetEvent<ComConnectionRequestedEvent>().Publish(selectedSerialPort);
                    _serialService.Connect(selectedSerialPort);
                }
            });
        }

        void ExecuteFocusCommand(string s)
        {
            // TODO: power status check
            int step;
            if (!Int32.TryParse(s, out step))
            {
                _eventAggregator.GetEvent<ExceptionEvent>()
                    .Publish(new ArgumentException("Cannot parse the movement steps"));
                return;
            }

            // put the blocking action in a separated thread
            Task.Run(() => _serialService.ControlFocus(step));
        }

        void ExecuteComUpdateListCommand()
        {
            //_eventAggregator.GetEvent<ComListUpdateRequestedEvent>().Publish();
            Task.Run(() => Params.ComSelection.Value = _serialService.UpdateComList());
        }

        private bool CanCameraConnectToggleExecuteMethod()
        {
            return Params.VimbaSelection.Selected != null;
        }

        private bool CanExecuteComConnectToggleCommand()
        {
            return Params.ComSelection.Selected != null;
        }

        private bool CanComOperationExecution(string s = null)
        {
            return Status.ComConnectionStatus.IsConnected;
        }

        private bool CanPowerConfigurationExecution(bool? b)
        {
            return b != null && (CanComOperationExecution() && b.Value);
        }

        #endregion


        private void ManualPowerCheck_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Params.MasterPowerCheck.Enabled = Params.FanPowerCheck.Enabled = Params.LaserPowerCheck.Enabled =
                Params.MotorPowerCheck.Enabled = !Params.ManualPowerCheck.Value;
        }
    }
}