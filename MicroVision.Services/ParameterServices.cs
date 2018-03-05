using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroVision.Services.Models;

namespace MicroVision.Services
{
    public interface IParameterServices
    {
        FieldParameter<string> CameraControllerUri { get; }
        FieldParameter<string> CameraUri { get; }
        FieldParameter<string> ProcessorUri { get; }
        FieldParameter<int> ExposureTime { get; }
        FieldParameter<double> Gain { get; }
        FieldParameter<int> LaserDuration { get; }
        FieldParameter<int> CaptureInterval { get; }
        FieldParameter<string> OutputDirectory { get; }
        SelectionParameter<string> ComSelection { get; }
        SelectionParameter<string> VimbaSelection { get; }
        CheckParameter ManualPowerCheck { get; }
        CheckParameter MasterPowerCheck { get; }
        CheckParameter FanPowerCheck { get; }
        CheckParameter LaserPowerCheck { get; }
        CheckParameter MotorPowerCheck { get; }
    }

    public class ParameterServices : IParameterServices
    {
        public ParameterServices()
        {
            
            // set the manual power override logic
            ManualPowerCheck.PropertyChanged += ManualPowerCheck_PropertyChanged;
        }

        private void ManualPowerCheck_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var senderObj = (CheckParameter) sender;
        }

        public FieldParameter<string> CameraControllerUri { get; } = new FieldParameter<string>() {Label = "Camera Controller Server Uri", IsEnabled = true, Value = ""};
        public FieldParameter<string> CameraUri { get; } = new FieldParameter<string>() {Label = "Camera Server Uri", IsEnabled = true, Value = ""};
        public FieldParameter<string> ProcessorUri { get; } = new FieldParameter<string>() {Label = "Image Processing Server Uri", IsEnabled = true, Value = ""};
        public FieldParameter<int> ExposureTime { get; } = new FieldParameter<int>(){Label="Exposure Time (us)", Value = 44, Minimum = 44, Maximum = 100000};
        public FieldParameter<double> Gain { get; } = new FieldParameter<double>() {Label = "Gain", Value = 0, Minimum = 0, Maximum = 20};
        public FieldParameter<int> LaserDuration { get; } = new FieldParameter<int>(){Label = "Laser duration (us)", Value = 20, Minimum = 0, Maximum = 100000};
        public FieldParameter<int> CaptureInterval { get; } = new FieldParameter<int>(){Label = "Capture Interval (ms)", Value = 1000, Minimum = 100, Maximum = 100000};
        public FieldParameter<string> OutputDirectory { get; } = new FieldParameter<string>(){Label = "Output directory", Value = @"C:\"};

        public SelectionParameter<string> ComSelection { get; } =new SelectionParameter<string>("COM");
        public SelectionParameter<string> VimbaSelection { get; } = new SelectionParameter<string>("Camera");

        public CheckParameter ManualPowerCheck { get; } = new CheckParameter("Manual");
        public CheckParameter MasterPowerCheck { get; } = new CheckParameter("Master", false);
        public CheckParameter FanPowerCheck { get; } = new CheckParameter("Fan", false);
        public CheckParameter LaserPowerCheck { get; } = new CheckParameter("Laser", false);
        public CheckParameter MotorPowerCheck { get; } = new CheckParameter("Motor", false);
    }

}
