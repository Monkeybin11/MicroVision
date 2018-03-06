using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using MicroVision.Core.Events;
using MicroVision.Core.Models;
using Prism.Events;
using RJCP.IO.Ports;
using static Services.CameraController;
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
        private CameraControllerClient _rpcClient;
        private Channel _rpcChannel;

        public SerialService(IParameterServices parameterServices, ILogService log, IEventAggregator eventAggregator)
        {
            _sp = new SerialPortStream();
            _sp.ErrorReceived += SerialPortErrorOccured;
            _sp.DataReceived += SerialPortDataReceived;
            _sp.NewLine = "\n";

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
                try
                {
                    _log.Logger.Info(_sp.ReadLine());
                }
                catch (ThreadInterruptedException)
                {
                    // time to stop the thread
                    return;
                }
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

        /// <summary>
        /// Connect to the serial port
        /// </summary>
        private void Connect()
        {
            if (_sp.IsOpen || _sp.IsDisposed)
            {
                return;
            }

            ConfigureWithParameterService();

            try
            {
                _sp.Open();
                _eventAggregator.GetEvent<ComConnectedEvent>().Publish();
                _serialDataParsingThread = new Thread(SerialDataParsing);
                _serialDataParsingThread.Start();
            }
            catch (Exception e)
            {
                _log.Logger.Error("Failed to open the serial port", e);
                _eventAggregator.GetEvent<ComErrorOccuredEvent>().Publish(e.Message);
            }
        }

        private void Disconnect()
        {
            try
            {
                _serialDataParsingThread.Interrupt();
                _serialDataParsingThread.Join(500);
                _sp.Close();
                _eventAggregator.GetEvent<ComDisconnectedEvent>().Publish(true);    //intentional disconnection
            }
            catch (Exception e)
            {
                _log.Logger.Error("Failed to close the serial port", e);
            }
        }

        /// <summary>
        /// Get the configuration from the paramter service and update the serial port object
        /// </summary>
        private void ConfigureWithParameterService()
        {
            _sp.PortName = _parameterServices.DeviceSelections.ComSelection.Selected;
        }

        private void DispatchCommand(ISerialCommand serialCommand)
        {
            SendCommand(serialCommand.BuildCommandString());
        }

        private void UpdateComList()
        {
            _parameterServices.DeviceSelections.ComSelection.Value = new List<string>(SerialPort.GetPortNames());
            _parameterServices.DeviceSelections.ComSelection.Selected = _parameterServices.DeviceSelections.ComSelection.Value[0];
        }

        private void SendCommand(string command) { } //TODO
    }
}
