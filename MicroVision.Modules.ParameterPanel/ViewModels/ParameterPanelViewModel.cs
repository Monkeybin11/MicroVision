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
using MicroVision.Services;
using MicroVision.Services.Models;

namespace MicroVision.Modules.ParameterPanel.ViewModels
{
    public class ParameterPanelViewModel : BindableBase
    {
        private DelegateCommand _testCommand;

        public ExposureTime ExposureTime { get; }
        public LaserDuration LaserDuration { get; }
        public CaptureInterval CaptureInterval { get; }
        public Gain Gain { get; }
        public OutputDirectory OutputDirectory { get; }

        public DelegateCommand TestCommand =>
            _testCommand ?? (_testCommand = new DelegateCommand(ExecuteTestCommand));

        void ExecuteTestCommand()
        {
            ExposureTime.Value -= 10;
        }
        public ParameterPanelViewModel(IParameterServices param)
        {
            ExposureTime = param.ExposureTime;
            LaserDuration = param.LaserDuration;
            CaptureInterval = param.CaptureInterval;
            Gain = param.Gain;
            OutputDirectory = param.OutputDirectory;
        }


    }
}
