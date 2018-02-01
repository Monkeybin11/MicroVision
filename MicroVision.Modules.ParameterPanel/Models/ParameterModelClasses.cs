using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroVision.Modules.ParameterPanel.Models
{
    public class BaseParameter<T> : BindableBase
    {
        public string Label { get; set; }
        private T _value;
        public T Value
        {
            get => _value;
            set { SetProperty(ref _value, value); }
            
        }
    }

    public class ExposureTimeParameter : BaseParameter<int>
    {
        public ExposureTimeParameter()
        {
            Label = "Exposure time:";
            Value = 20;
        }
    }

}
