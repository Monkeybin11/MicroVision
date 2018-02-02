using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace MicroVision.Services.Models
{
    public class Status :BindableBase
    {
        public string Label { get; protected set; }

        public Status(string label)
        {
            Label = label;
        }
    }
}
