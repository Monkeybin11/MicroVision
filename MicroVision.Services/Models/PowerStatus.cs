using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace MicroVision.Services.Models
{
    public class PowerStatus : Status
    {
        private Boolean _isPowered;
        public Boolean IsPowered
        {
            get { return _isPowered; }
            set { SetProperty(ref _isPowered, value); }
        }

        public PowerStatus(string label) : base(label)
        {
        }
    }
}
