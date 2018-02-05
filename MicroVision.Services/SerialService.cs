using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MicroVision.Core.Events;
using MicroVision.Core.Models;
using Prism.Events;
using RJCP.IO.Ports;
using SerialErrorReceivedEventArgs = System.IO.Ports.SerialErrorReceivedEventArgs;

namespace MicroVision.Services
{
    public interface ISerialService
    {
    }
    public class SerialService : ISerialService
    {
        private readonly IParameterServices _parameterServices;
        private readonly ILogService _log;
        private readonly IEventAggregator _eventAggregator;
        private SerialPortStream _sp;
        private Thread _serialDataParsingThread;

        public SerialService(IParameterServices parameterServices, ILogService log, IEventAggregator eventAggregator)
        {
            _sp = new SerialPortStream();
            _sp.ErrorReceived += SerialPortErrorOccured;
            _sp.DataReceived += SerialPortDataReceived;
            _sp.NewLine = "\n";
            _serialDataParsingThread = new Thread(SerialDataParsing);

            _parameterServices = parameterServices;
            _log = log;

            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<ComListUpdateRequestedEvent>().Subscribe(UpdateComList);
            _eventAggregator.GetEvent<ComCommandDispatchedEvent>().Subscribe(DispatchCommand);
            _eventAggregator.GetEvent<ComConnectionRequestedEvent>().Subscribe(Connect);
            _eventAggregator.GetEvent<ComDisconnectionRequestedEvent>().Subscribe(Disconnect);

        }

        private void SerialDataParsing()
        {
            while (true)
            {
                _log.Logger.Info(_sp.ReadLine());
            }        
        }

        private void SerialPortDataReceived(object sender, RJCP.IO.Ports.SerialDataReceivedEventArgs e)
        {
            var sp = (SerialPortStream) sender;
        }

        private void SerialPortErrorOccured(object sender, RJCP.IO.Ports.SerialErrorReceivedEventArgs serialErrorReceivedEventArgs)
        {
            _eventAggregator.GetEvent<ComErrorOccuredEvent>().Publish(serialErrorReceivedEventArgs.EventType.ToString());
        }

        private void Disconnect()
        {
            try
            {
                _serialDataParsingThread.Abort();
                _sp.Close();
                _eventAggregator.GetEvent<ComDisconnectedEvent>().Publish(true);    //intentional disconnection
            }
            catch (Exception e)
            {
                _log.Logger.Error("Failed to close the serial port", e);
            }
        }

        /// <summary>
        /// Connect to the serial port
        /// </summary>
        private void Connect()
        {
            ConfigureWithParameterService();
            try
            {
                _sp.Open();
                _eventAggregator.GetEvent<ComConnectedEvent>().Publish();
                _serialDataParsingThread.Start();
            }
            catch (Exception e)
            {
                _log.Logger.Error("Failed to open the serial port", e);
                _eventAggregator.GetEvent<ComErrorOccuredEvent>().Publish(e.Message);
            }
        }

        /// <summary>
        /// Get the configuration from the paramter service and update the serial port object
        /// </summary>
        private void ConfigureWithParameterService()
        {
            _sp.PortName = _parameterServices.ComSelection.Selected;
        }

        private void DispatchCommand(ISerialCommand serialCommand)
        {
            SendCommand(serialCommand.BuildCommandString());
        }

        private void UpdateComList()
        {
            _parameterServices.ComSelection.Value = new List<string>(SerialPort.GetPortNames());
            _parameterServices.ComSelection.Selected = _parameterServices.ComSelection.Value[0];
        }

        private void SendCommand(string command) { } //TODO
    }
}
