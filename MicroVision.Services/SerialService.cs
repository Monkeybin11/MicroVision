using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroVision.Core.Events;
using Prism.Events;

namespace MicroVision.Services
{
    public interface ISerialService
    {
    }

    public class SerialService : ISerialService
    {
        private readonly IParameterServices _parameterServices;
        private readonly IEventAggregator _eventAggregator;

        public SerialService(IParameterServices parameterServices, IEventAggregator eventAggregator)
        {
            _parameterServices = parameterServices;
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<ComListUpdateRequestedEvent>().Subscribe(UpdateComList);
        }

        private void UpdateComList()
        {
            _parameterServices.ComSelection.Value = new List<string>(SerialPort.GetPortNames());
        }
    }
}
