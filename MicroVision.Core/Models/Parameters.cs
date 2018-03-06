using System;
using System.Collections.Generic;
using Prism.Mvvm;

namespace MicroVision.Core.Models
{

    /// <summary>
    /// Base parameter class
    /// </summary>
    /// <typeparam name="T"> value type</typeparam>
    [Serializable]
    public abstract class Parameter<T> : BindableBase
    {
        private bool _isEnabled;
        public string Label { get; set; }
        public abstract T Value { get; set; }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public Parameter(string label, bool isEnabled = true)
        {
            Label = label;
            IsEnabled = isEnabled;
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

        public FieldParameter(string label, T value, bool isEnabled, T minimum, T maximum) : base(label, isEnabled)
        {
            this.Value = value;
            Minimum = minimum;
            Maximum = maximum;
        }

        public FieldParameter() : base("Field", true)
        {
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

        public SelectionParameter() : base("Label", true)
        {
            _value = new List<T>();
        }
        public SelectionParameter(string label, bool isEnabled = true) : base(label, isEnabled)
        {
            _value = new List<T>();
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
        public CheckParameter(): base("Label", true) { }
        public CheckParameter(string label) : base(label) {}
        public CheckParameter(string label, bool isEnabled):base(label, isEnabled) { }
    }

    #endregion
}
