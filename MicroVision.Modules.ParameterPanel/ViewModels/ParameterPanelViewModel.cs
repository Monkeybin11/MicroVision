using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Collections.ObjectModel;
using System.Windows;
using MicroVision.Core.Events;
using MicroVision.Core.Models;
using MicroVision.Services;
using MicroVision.Services.Models;
using Prism.Events;

namespace MicroVision.Modules.ParameterPanel.ViewModels
{
    public class ParameterPanelViewModel : BindableBase
    {
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

        private DelegateCommand _comConnectToggleCommand;

        public DelegateCommand ComConnectToggleCommand =>
            _comConnectToggleCommand ??
            (_comConnectToggleCommand = new DelegateCommand(ExecuteComConnectToggleCommand, CanExecuteComConnectToggleCommand)).ObservesProperty(() => Params.DeviceSelections.ComSelection.Selected);

        private bool CanExecuteComConnectToggleCommand()
        {
            return Params.DeviceSelections.ComSelection.Selected != null;
        }

        void ExecuteComConnectToggleCommand()
        {
            if (ComConnectionStatus.IsConnected) // already connected 
            {
                _eventAggregator.GetEvent<ComDisconnectionRequestedEvent>().Publish();
            }
            else
            {
                var selectedSerialPort = Params.DeviceSelections.ComSelection.Selected;
                _eventAggregator.GetEvent<ComConnectionRequestedEvent>().Publish(selectedSerialPort);
            }
        }

        private DelegateCommand _comUpdateListCommand;
        public DelegateCommand ComUpdateListCommand =>
            _comUpdateListCommand ?? (_comUpdateListCommand = new DelegateCommand(ExecuteComUpdateListCommand));

        void ExecuteComUpdateListCommand()
        {
            _eventAggregator.GetEvent<ComListUpdateRequestedEvent>().Publish();
        }

        #endregion

        public ParameterPanelViewModel(IParameterServices param, IStatusServices statusService,
            IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            Params = param;

            // ask for list update for initial value
            // _eventAggregator.GetEvent<ComListUpdateRequestedEvent>().Publish();

            Params.PowerConfigurations.ManualPowerCheck.PropertyChanged += ManualPowerCheck_PropertyChanged;

            ComConnectionStatus = statusService.ComConnectionStatus;
        }


        private void ManualPowerCheck_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var senderObj = (CheckParameter) sender;
            var powerConfigurations = Params.PowerConfigurations;
            powerConfigurations.MasterPowerCheck.IsEnabled = powerConfigurations.FanPowerCheck.IsEnabled =
                powerConfigurations.LaserPowerCheck.IsEnabled =
                    powerConfigurations.MotorPowerCheck.IsEnabled = senderObj.Value;
        }
    }
}