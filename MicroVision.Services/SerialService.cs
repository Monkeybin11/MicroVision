using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;

namespace MicroVision.Services
{
    public class SerialService
    {
        private readonly IParameterServices _parameterServices;
        private readonly IEventAggregator _eventAggregator;

        public SerialService(IParameterServices parameterServices, IEventAggregator eventAggregator)
        {
            _parameterServices = parameterServices;
            _eventAggregator = eventAggregator;
            
        }
    }
}
