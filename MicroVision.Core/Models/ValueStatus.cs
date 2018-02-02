using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroVision.Services.Models
{
    public class ValueStatus<T> : Status
    {
        private T _value;
        public T Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }

        public ValueStatus(string label) : base(label)
        {
        }
    }
}
