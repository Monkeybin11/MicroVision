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
using MicroVision.Services;
using MicroVision.Services.Models;
using Prism.Events;

namespace MicroVision.Modules.ParameterPanel.ViewModels
{
    public class ParameterPanelViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private DelegateCommand _testCommand;

        public FieldParameter<int> ExposureTime { get; }
        public FieldParameter<int> LaserDuration { get; }
        public FieldParameter<int> CaptureInterval { get; }
        public FieldParameter<double> Gain { get; }
        public FieldParameter<string> OutputDirectory { get; }
        public CheckParameter MotorPowerCheck { get; }
        public CheckParameter LaserPowerCheck { get; }
        public CheckParameter FanPowerCheck { get; }
        public CheckParameter MasterPowerCheck { get; }
        public CheckParameter ManualPowerCheck { get; }
        public SelectionParameter<string> VimbaSelection { get; set; }
        public SelectionParameter<string> ComSelection { get; set; }

        public DelegateCommand TestCommand =>
            _testCommand ?? (_testCommand = new DelegateCommand(ExecuteTestCommand));

        void ExecuteTestCommand()
        {
            ExposureTime.Value -= 10;
        }
        public ParameterPanelViewModel(IParameterServices param, IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            ExposureTime = param.ExposureTime;
            LaserDuration = param.LaserDuration;
            CaptureInterval = param.CaptureInterval;
            Gain = param.Gain;
            OutputDirectory = param.OutputDirectory;

            ManualPowerCheck = param.ManualPowerCheck;
            MasterPowerCheck = param.MasterPowerCheck;
            FanPowerCheck = param.FanPowerCheck;
            LaserPowerCheck = param.LaserPowerCheck;
            MotorPowerCheck = param.MotorPowerCheck;

            ComSelection = param.ComSelection;
            VimbaSelection = param.VimbaSelection;

            // ask for list update for initial value
            _eventAggregator.GetEvent<ComListUpdateRequestedEvent>().Publish();

            ManualPowerCheck.PropertyChanged += ManualPowerCheck_PropertyChanged;
        }

        private void ManualPowerCheck_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var senderObj = (CheckParameter) ManualPowerCheck;
            MasterPowerCheck.IsEnabled = FanPowerCheck.IsEnabled = LaserPowerCheck.IsEnabled = MotorPowerCheck.IsEnabled = senderObj.Value;
        }
    }
}
