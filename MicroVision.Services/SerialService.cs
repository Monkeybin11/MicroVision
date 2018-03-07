using System;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using MicroVision.Core.Events;
using MicroVision.Core.Exceptions;
using MicroVision.Core.Models;
using MicroVision.Services.GrpcReference;
using Prism.Events;
using Services;

namespace MicroVision.Services
{
    public interface ISerialService
    {
        /// <summary>
        /// Connect to the serial port
        /// </summary>
        /// <param name="s"> com port name</param>
        /// <returns>whether the port is connected successfully</returns>
        bool Connect(string s);

        /// <summary>
        /// Disconnect from the current port
        /// </summary>
        /// <returns>whether the port is disconnected successfully</returns>
        bool Disconnect();

        void DispatchCommand(SerialCommand serialCommand);

        /// <summary>
        /// get the com list
        /// </summary>
        /// <returns>null for failure</returns>
        List<string> UpdateComList();
    }

    public class SerialService : ISerialService
    {
        private const string CameraControllerConnectionErrorPrompt = "Failed to connect to camera controller server. Please check if the server is running and check if the server uri is correct in app.config. Please restart the software";
        private readonly ILogService _log;
        private readonly IEventAggregator _eventAggregator;
        private readonly IRpcService _rpcService;
        
        

        public SerialService(ILogService log, IEventAggregator eventAggregator,
            IRpcService rpcService)
        {
            _log = log;

            _eventAggregator = eventAggregator;
            _rpcService = rpcService;
            //_eventAggregator.GetEvent<ComListUpdateRequestedEvent>().Subscribe(UpdateComList);
            //_eventAggregator.GetEvent<ComCommandDispatchedEvent>().Subscribe(DispatchCommand);
            //_eventAggregator.GetEvent<ComConnectionRequestedEvent>().Subscribe(Connect);
            //_eventAggregator.GetEvent<ComDisconnectionRequestedEvent>().Subscribe(Disconnect);
            _eventAggregator.GetEvent<ShutDownEvent>().Subscribe(RestoreRpcStatus);
        }

        private void RestoreRpcStatus()
        {
            try
            {
                _rpcService.CameraControllerClient.RequestSoftwareReset(new Empty());
            }
            catch (Exception e)
            {
                // ignored
            }
            
            Disconnect();
        }

        #region Service methods


        /// <summary>
        /// Connect to the serial port
        /// </summary>
        /// <param name="s"> com port name</param>
        /// <returns>whether the port is connected successfully</returns>
        public bool Connect(string s)
        {
            ConnectionResponse ret = null;
            try
            {
                ret = _rpcService.CameraControllerClient.RequestConnectToPort(new ConnectionRequest() { ComPort = s, Connect = true });
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
                return false;
            }

            if (!ret.IsConnected)
            {
                _eventAggregator.GetEvent<ExceptionEvent>()
                    .Publish(new ComListException("COM port is not connected"));
            }
            else
            {
                _eventAggregator.GetEvent<ComConnectedEvent>().Publish();
                _eventAggregator.GetEvent<NotifyOperationEvent>().Publish($"{s} successfully connected");
            }

            return true;
        }

        /// <summary>
        /// Disconnect from the current port
        /// </summary>
        /// <returns>whether the port is disconnected successfully</returns>
        public bool Disconnect()
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
                return false;
            }

            _eventAggregator.GetEvent<ComDisconnectedEvent>().Publish(true);
            _eventAggregator.GetEvent<NotifyOperationEvent>().Publish("COM port successfully closed");
            return true;
        }


        public void DispatchCommand(SerialCommand serialCommand)
        {
        }

        /// <summary>
        /// get the com list
        /// </summary>
        /// <returns>null for failure</returns>
        public List<string> UpdateComList()
        {
            ComList comList = null;
            try
            {
                comList = _rpcService.CameraControllerClient.RequestComList(new ComListRequest());
            }
            catch (Exception e)
            {
                _log.Logger.Error(e);
                _eventAggregator.GetEvent<ExceptionEvent>().Publish(new CameraControllerRpcServerConnectionException(CameraControllerConnectionErrorPrompt));
                return null;
            }

            if (comList == null || comList.Error != null)
            {
                _eventAggregator.GetEvent<ExceptionEvent>()
                    .Publish(new ComListException("Failed to update the serial port list"));
                return null;
            }
            return new List<string>(comList.ComPort);
        }
        #endregion

    }
}