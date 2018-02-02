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
        FieldParameter<int> ExposureTime { get; }
        FieldParameter<double> Gain { get; }
        FieldParameter<int> LaserDuration { get; }
        FieldParameter<int> CaptureInterval { get; }
        FieldParameter<string> OutputDirectory { get; }
    }

    public class ParameterServices : IParameterServices
    {
        public ParameterServices()
        { 
        }

        public FieldParameter<int> ExposureTime { get; } = new FieldParameter<int>(){Label="Exposure Time (us)", Value = 44, Minimum = 44, Maximum = 100000};
        public FieldParameter<double> Gain { get; } = new FieldParameter<double>() {Label = "Gain", Value = 0, Minimum = 0, Maximum = 20};
        public FieldParameter<int> LaserDuration { get; } = new FieldParameter<int>(){Label = "Laser duration (us)", Value = 20, Minimum = 0, Maximum = 100000};
        public FieldParameter<int> CaptureInterval { get; } = new FieldParameter<int>(){Label = "Capture Interval (ms)", Value = 1000, Minimum = 100, Maximum = 100000};
        public FieldParameter<string> OutputDirectory { get; } = new FieldParameter<string>(){Label = "Output directory", Value = @"C:\"};
    }

}
