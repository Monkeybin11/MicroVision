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
        ExposureTime ExposureTime { get; }
        Gain Gain { get; }
        LaserDuration LaserDuration { get; }
        CaptureInterval CaptureInterval { get; }
        OutputDirectory OutputDirectory { get; }
    }

    public class ParameterServices : IParameterServices
    {
        public ParameterServices()
        { 
        }

        public ExposureTime ExposureTime { get; } = new ExposureTime();
        public Gain Gain { get; } = new Gain();
        public LaserDuration LaserDuration { get; } = new LaserDuration();
        public CaptureInterval CaptureInterval { get; } = new CaptureInterval();
        public OutputDirectory OutputDirectory { get; } = new OutputDirectory();
    }
}
