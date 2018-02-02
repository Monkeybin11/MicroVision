using System;
using System.Collections.Generic;
using Prism.Mvvm;

namespace MicroVision.Services.Models
{

    /// <summary>
    /// Base parameter class
    /// </summary>
    /// <typeparam name="T"> value type</typeparam>
    public abstract class Parameter<T> : BindableBase
    {
        private bool _isEnabled;
        public string Label { get; protected set; }
        public abstract T Value { get; set; }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }
    }

    #region Field Parameters

    public class FieldParameter<T> : Parameter<T> where T : IComparable
    {
        private T _value;
        public override T Value
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

    public class ExposureTime : FieldParameter<int>
    {
        public ExposureTime()
        {
            Label = "Exposure time (us):";
            Value = 44;
            Minimum = 44;
            Maximum = 1000000;
        }
    }

    public class Gain : FieldParameter<double>
    {
        public Gain()
        {
            Label = "Gain:";
            Value = 0;
            Minimum = 0;
            Maximum = 20;
        }
    }

    public class LaserDuration : FieldParameter<int>
    {
        public LaserDuration()
        {
            Label = "Laser Duration (us):";
            Value = 20;
            Minimum = 0;
            Maximum = 10000;
        }
    }

    public class CaptureInterval : FieldParameter<int>
    {
        public CaptureInterval()
        {
            Label = "Capture Interval (ms):";
            Value = 500;
            Minimum = 0;
            Maximum = 100000;
        }
    }
    public class OutputDirectory : FieldParameter<string>
    {
        public OutputDirectory()
        {
            Label = "Output Directory:";
            Value = @"C:\";
        }
    }

    #endregion

    #region Combobox selection
    public class SelectionParameter<T> : Parameter<List<T>>
    {
        private List<T> _value;
        private T _selected;

        public override List<T> Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public T Selected
        {
            get { return _selected; }
            set { SetProperty(ref _selected, value); }
        }
    }

    public class ComSelectionParameter : SelectionParameter<string>
    {
        public ComSelectionParameter()
        {
            Label = "COM";
        }
    }

    public class VimbaSelectionParameter : SelectionParameter<string>
    {
        public VimbaSelectionParameter()
        {
            Label = "Camera";
        }
    }
    #endregion

    #region Check selection

    public class CheckParameter : Parameter<bool>
    {
        private bool _value;

        public override bool Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }
    }

    #endregion
}
