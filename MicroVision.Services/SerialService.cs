using System;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using MicroVision.Core.Events;
using MicroVision.Core.Exceptions;
using MicroVision.Core.Models;
using MicroVision.Services.Annotations;
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
        private const string CameraControllerConnectionErrorPrompt =
            "Failed to connect to camera controller server. Please check if the server is running and check if the server uri is correct in app.config. Please restart the software";

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

        private TReturnType TryInvoke<TReturnType>([NotNull]Func<TReturnType> func,
            string runtimeExceptionPrompt, string connectionExceptionPrompt = CameraControllerConnectionErrorPrompt,
            bool rethrow = false, bool checkError = true, Action<string> onError = null) 
        {
            var ret = default(TReturnType);
            try
            {
                ret = func.Invoke();
            }
            catch (Exception e)
            {
                _log.Logger.Error(e);
                _eventAggregator.GetEvent<ExceptionEvent>()
                    .Publish(new CameraControllerRpcServerConnectionException(connectionExceptionPrompt));
                if (rethrow) throw;
                return ret;
            }

            bool hasError = false;
            Error error = null;

            if (checkError)
            {
                try
                {
                    error = (Error) typeof(TReturnType).GetProperty("Error").GetValue(ret);
                    if (error != null) hasError = true;
                }
                catch (Exception e)
                {
                    error = new Error() {Level = Error.Types.Level.Error, Message = e.Message, Timestamp = Timestamp.FromDateTime(DateTime.Now)};
                    hasError = true;
                }
            }

            if (ret == null || hasError)
            {
                _eventAggregator.GetEvent<ExceptionEvent>()
                    .Publish(new Exception($"{runtimeExceptionPrompt} {error?.Message}"));
            }

            return ret;
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
            var ret = TryInvoke(() => _rpcService.CameraControllerClient.RequestConnectToPort(
                new ConnectionRequest() { ComPort = s, Connect = true }), $"Failed to connect to {s}");

            if (ret == null || ret.Error != null) return false;

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
            var ret = TryInvoke(() => _rpcService.CameraControllerClient.RequestConnectToPort(
                new ConnectionRequest() { Connect = false }), "Failed to disconnect");
            if (ret.Error != null || ret == null) return false;
            _eventAggregator.GetEvent<ComDisconnectedEvent>().Publish(true);
            _eventAggregator.GetEvent<NotifyOperationEvent>().Publish("COM port successfully closed");
            return true;
        }


        public void DispatchCommand(SerialCommand serialCommand)
        {
            switch (serialCommand.Command)
            {
                case SerialCommand.RpcSerialCommand.GetInfo:
                    try
                    {
                        var ret = _rpcService.CameraControllerClient.GetInfo(new Empty());
                    }
                    catch (Exception e)
                    {
                        _log.Logger.Error(CameraControllerConnectionErrorPrompt);
                        _eventAggregator.GetEvent<ExceptionEvent>().Publish(
                            new CameraControllerRpcServerConnectionException(
                                $"Failed to invoke {serialCommand.Command.ToString()}: {e.Message}"));
                    }

                    break;
                case SerialCommand.RpcSerialCommand.IsConnected:
                    break;

                case SerialCommand.RpcSerialCommand.SoftwareReset:
                    break;
                case SerialCommand.RpcSerialCommand.RequestPowerStatus:
                    break;
                case SerialCommand.RpcSerialCommand.RequestCurrentStatus:
                    break;
                case SerialCommand.RpcSerialCommand.RequestFocusStatus:
                    break;
                case SerialCommand.RpcSerialCommand.RequestLaserStatus:
                    break;
                case SerialCommand.RpcSerialCommand.RequestArmTrigger:
                    break;
                case SerialCommand.RpcSerialCommand.RequestSoftwareReset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// get the com list
        /// </summary>
        /// <returns>null for failure</returns>
        public List<string> UpdateComList()
        {
            var comList = TryInvoke(() => _rpcService.CameraControllerClient.RequestComList(new ComListRequest()),
                "Failed to update the serial port list");
            return comList?.ComPort == null ? new List<string>() : new List<string>(comList.ComPort);
        }

        #endregion
    }
}