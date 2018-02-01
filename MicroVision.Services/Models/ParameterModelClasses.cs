using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroVision.Modules.ParameterPanel.Models
{
    public class BaseParameter<T> : BindableBase where T : IComparable
    {
        public string Label { get; set; }
        private T _value;
        public T Value
        {
            get => _value;
            set
            {
                if (typeof(T).IsValueType)
                {
                    var _v = value.CompareTo(Maximum) > 0 ? Maximum : value;
                    _v = value.CompareTo(Minimum) < 0 ? Minimum : value;
                    SetProperty(ref _value, _v);
                }
                else
                {
                    SetProperty(ref _value, value);
                }

            }
            
        }
        public T Minimum { get; set; }
        public T Maximum { get; set; }

    }

    public class ExposureTime : BaseParameter<int>
    {
        public ExposureTime()
        {
            Label = "Exposure time:";
            Value = 44;
            Minimum = 44;
            Maximum = 1000000;
        }
    }

    public class Gain : BaseParameter<double>
    {
        public Gain()
        {
            Label = "Gain:";
            Value = 0;
            Minimum = 0;
            Maximum = 20;
        }
    }

    public class OutputDirectory : BaseParameter<string>
    {
        public OutputDirectory()
        {
            Label = "Output Directory:";
            Value = @"C:\";
        }
    }
}
