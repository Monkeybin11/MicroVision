using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroVision.Modules.ParameterPanel.Models;

namespace MicroVision.Services
{
    public interface IParameterService
    {
        ExposureTime ExposureTime { get; }
        Gain Gain { get; }
        OutputDirectory OutputDirectory { get; }
    }

    public class ParameterService : IParameterService
    {
        public ParameterService()
        { 
        }

        public ExposureTime ExposureTime { get; } = new ExposureTime();
        public Gain Gain { get; } = new Gain();
        public OutputDirectory OutputDirectory { get; } = new OutputDirectory();
    }
}
