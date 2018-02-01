using MicroVision.Modules.ParameterPanel.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Collections.ObjectModel;

namespace MicroVision.Modules.ParameterPanel.ViewModels
{
    public class ParameterPanelViewModel : BindableBase
    {
        //private ExposureTimeParameter exposureTimeParameter = new ExposureTimeParameter();
        //public ExposureTimeParameter ExposureTime
        //{
        //    get => exposureTimeParameter;
        //    set
        //    {
        //        SetProperty(ref exposureTimeParameter, value);
        //    }
        //}
        public ExposureTimeParameter ExposureTime { get; } = new ExposureTimeParameter();
        public DelegateCommand Command { get; }
        public ParameterPanelViewModel()
        {
            Command = new DelegateCommand(Executed);
        }

        private void Executed()
        {
            ExposureTime.Value++;
        }
    }
}
