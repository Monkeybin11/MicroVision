using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using MicroVision.Core.Events;
using MicroVision.Core.Exceptions;
using MicroVision.Core.Models;
using MicroVision.Services.GrpcReference;
using Prism.Events;
using RJCP.IO.Ports;
using Services;
using static Services.CameraController;
using SerialErrorReceivedEventArgs = System.IO.Ports.SerialErrorReceivedEventArgs;

namespace MicroVision.Services
{
    public interface ISerialService
    {
    }

    public class SerialService : ISerialService
    {
        private const string CameraControllerConnectionErrorPrompt = "Failed to connect to camera controller server. Please check if the server is running and check if the server uri is correct in app.config. Please restart the software";
        private readonly IParameterServices _parameterServices;
        private readonly ILogService _log;
        private readonly IEventAggregator _eventAggregator;
        private readonly IRpcService _rpcService;


        public SerialService(IParameterServices parameterServices, ILogService log, IEventAggregator eventAggregator,
            IRpcService rpcService)
        {
            _parameterServices = parameterServices;
            _log = log;

            _eventAggregator = eventAggregator;
            _rpcService = rpcService;
            _eventAggregator.GetEvent<ComListUpdateRequestedEvent>().Subscribe(UpdateComList);
            _eventAggregator.GetEvent<ComCommandDispatchedEvent>().Subscribe(DispatchCommand);
            _eventAggregator.GetEvent<ComConnectionRequestedEvent>().Subscribe(Connect);
            _eventAggregator.GetEvent<ComDisconnectionRequestedEvent>().Subscribe(Disconnect);
        }

        /// <summary>
        /// Connect to the serial port
        /// </summary>
        private void Connect(string s)
        {
            ConnectionResponse ret = null;
            try
            {
                ret = _rpcService.CameraControllerClient.RequestConnectToPort(new ConnectionRequest() {ComPort = s, Connect = true});
            }
            catch (Exception e)
            {
                _log.Logger.Error(e);
                _eventAggregator.GetEvent<ExceptionEvent>().Publish(new CameraControllerRpcServerConnectionException(CameraControllerConnectionErrorPrompt));
            }

            if (ret == null || ret.Error != null)
            {
                _eventAggregator.GetEvent<ExceptionEvent>()
                    .Publish(new ComListException($"Failed to connect to {s}: {ret?.Error.Message}"));
                return;
            }

            if (!ret.IsConnected)
            {
                _eventAggregator.GetEvent<ExceptionEvent>()
                    .Publish(new ComListException("COM port is not connected"));
            }
            else
            {
                _eventAggregator.GetEvent<ComConnectedEvent>().Publish();
                _eventAggregator.GetEvent<NotifyOperationEvents>().Publish($"{s} successfully connected");
            }
        }

        private void Disconnect()
        {
            ConnectionResponse ret = null;
            try
            {
                ret = _rpcService.CameraControllerClient.RequestConnectToPort(new ConnectionRequest() { Connect = false });
            }
            catch (Exception e)
            {
                _log.Logger.Error(e);
                _eventAggregator.GetEvent<ExceptionEvent>().Publish(new CameraControllerRpcServerConnectionException(CameraControllerConnectionErrorPrompt));
            }

            if (ret == null || ret.Error != null)
            {
                _eventAggregator.GetEvent<ExceptionEvent>()
                    .Publish(new ComListException($"Failed to disconnect {ret?.Error.Message}"));
                return;
            }

            if (!ret.IsConnected)
            {
                _eventAggregator.GetEvent<ExceptionEvent>()
                    .Publish(new ComListException("COM port is not connected"));
            }
            else
            {
                _eventAggregator.GetEvent<ComDisconnectedEvent>().Publish(true);
                _eventAggregator.GetEvent<NotifyOperationEvents>().Publish("COM port successfully closed");
            }
        }

        /// <summary>
        /// Get the configuration from the paramter service and update the serial port object
        /// </summary>
        private void ConfigureWithParameterService()
        {
        }

        private void DispatchCommand(ISerialCommand serialCommand)
        {
        }

        /// <summary>
        /// Triggered by the event aggregator. Will call rpc to get the available COM ports
        /// </summary>
        private void UpdateComList()
        {
            _parameterServices.DeviceSelections.ComSelection.Value.Clear();
            ComList comList = null;
            try
            {
                comList = _rpcService.CameraControllerClient.RequestComList(new ComListRequest());
            }
            catch (Exception e)
            {
                _log.Logger.Error(e);
                _eventAggregator.GetEvent<ExceptionEvent>().Publish(new CameraControllerRpcServerConnectionException(CameraControllerConnectionErrorPrompt));
                return;
            }

            if (comList == null || comList.Error != null)
            {
                _eventAggregator.GetEvent<ExceptionEvent>()
                    .Publish(new ComListException("Failed to update the serial port list"));
                return;
            }
            _parameterServices.DeviceSelections.ComSelection.Value = new List<string>(comList.ComPort);
        }
    }
}