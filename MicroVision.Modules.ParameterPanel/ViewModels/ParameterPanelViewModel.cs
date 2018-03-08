using System;
using MicroVision.Core.Events;
using Prism.Commands;
using Prism.Mvvm;
using MicroVision.Core.Models;
using MicroVision.Services;
using MicroVision.Services.Models;
using Prism.Events;

namespace MicroVision.Modules.ParameterPanel.ViewModels
{
    public class ParameterPanelViewModel : BindableBase
    {
        private readonly ISerialService _serialService;
        private readonly IEventAggregator _eventAggregator;

        #region properties

        #region parameter properties

        public IParameterServices Params { get; }

        #endregion

        #region status properties

        public ConnectionStatus ComConnectionStatus { get; }

        #endregion

        #endregion


        #region Commands

        private DelegateCommand<bool?> _powerConfigurationCommand;

        public DelegateCommand<bool?> PowerConfigurationCommand =>
            _powerConfigurationCommand ??
            (_powerConfigurationCommand =
                new DelegateCommand<bool?>(ExecutePowerConfigurationCommand, CanPowerConfigurationExecution))
            .ObservesProperty(() => ComConnectionStatus.IsConnected)
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
                    ComConnectionStatus.IsConnected));

        void ExecutePowerConfigurationCommand(bool? b)
        {
            _serialService.ControlPower(Params.MasterPowerCheck.Value, Params.FanPowerCheck.Value,
                Params.MotorPowerCheck.Value, Params.LaserPowerCheck.Value);
        }

        void ExecuteComConnectToggleCommand()
        {
            if (ComConnectionStatus.IsConnected) // already connected 
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

            _serialService.ControlFocus(step);
        }

        void ExecuteComUpdateListCommand()
        {
            //_eventAggregator.GetEvent<ComListUpdateRequestedEvent>().Publish();
            Params.ComSelection.Value = _serialService.UpdateComList();
        }

        private bool CanExecuteComConnectToggleCommand()
        {
            return Params.ComSelection.Selected != null;
        }

        private bool CanComOperationExecution(string s = null)
        {
            return ComConnectionStatus.IsConnected;
        }

        private bool CanPowerConfigurationExecution(bool? b)
        {
            return b != null && (CanComOperationExecution() && b.Value);
        }

        #endregion

        public ParameterPanelViewModel(ISerialService serialService, IParameterServices param,
            IStatusServices statusService,
            IEventAggregator eventAggregator)
        {
            _serialService = serialService;
            _eventAggregator = eventAggregator;
            Params = param;

            // ask for list update for initial value
            // _eventAggregator.GetEvent<ComListUpdateRequestedEvent>().Publish();

            Params.ManualPowerCheck.PropertyChanged += ManualPowerCheck_PropertyChanged;

            ComConnectionStatus = statusService.ComConnectionStatus;

            // Determine if the server is already connected
            if (_serialService.IsConnected())
                _eventAggregator.GetEvent<ComConnectedEvent>().Publish();
        }


        private void ManualPowerCheck_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Params.MasterPowerCheck.Enabled = Params.FanPowerCheck.Enabled = Params.LaserPowerCheck.Enabled = Params.MotorPowerCheck.Enabled = !Params.ManualPowerCheck.Value;
        }
    }
}