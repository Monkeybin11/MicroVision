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

        private DelegateCommand _comConnectToggleCommand;

        public DelegateCommand ComConnectToggleCommand =>
            _comConnectToggleCommand ??
            (_comConnectToggleCommand = new DelegateCommand(ExecuteComConnectToggleCommand, CanExecuteComConnectToggleCommand)).ObservesProperty(() => Params.ComSelection.Selected);

        private bool CanExecuteComConnectToggleCommand()
        {
            return Params.ComSelection.Selected != null;
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

        private DelegateCommand _comUpdateListCommand;
        public DelegateCommand ComUpdateListCommand =>
            _comUpdateListCommand ?? (_comUpdateListCommand = new DelegateCommand(ExecuteComUpdateListCommand));

        void ExecuteComUpdateListCommand()
        {
            //_eventAggregator.GetEvent<ComListUpdateRequestedEvent>().Publish();
            Params.ComSelection.Value = _serialService.UpdateComList();
        }

        #endregion

        public ParameterPanelViewModel(ISerialService serialService, IParameterServices param, IStatusServices statusService,
            IEventAggregator eventAggregator)
        {
            _serialService = serialService;
            _eventAggregator = eventAggregator;
            Params = param;

            // ask for list update for initial value
            // _eventAggregator.GetEvent<ComListUpdateRequestedEvent>().Publish();

            Params.ManualPowerCheck.PropertyChanged += ManualPowerCheck_PropertyChanged;

            ComConnectionStatus = statusService.ComConnectionStatus;
        }


        private void ManualPowerCheck_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var senderObj = (CheckParameter) sender;
            Params.MasterPowerCheck.IsEnabled = Params.FanPowerCheck.IsEnabled =
                Params.LaserPowerCheck.IsEnabled =
                    Params.MotorPowerCheck.IsEnabled = senderObj.Value;
        }
    }
}