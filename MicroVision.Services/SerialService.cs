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
            _eventAggregator.GetEvent<ShutDownEvent>().Subscribe(RestoreRpcStatus);
        }


        /// <summary>
        /// Rpc invocation wrapper
        /// </summary>
        /// <typeparam name="TReturnType"></typeparam>
        /// <param name="func"></param>
        /// <param name="runtimeExceptionPrompt"></param>
        /// <param name="connectionExceptionPrompt"></param>
        /// <param name="rethrow"></param>
        /// <param name="checkError"></param>
        /// <param name="notifyError">when the error is insiginficant it may not be notified</param>
        /// <param name="onError"></param>
        /// <returns></returns>
        private TReturnType TryInvoke<TReturnType>([NotNull] Func<TReturnType> func,
            string runtimeExceptionPrompt, string connectionExceptionPrompt = CameraControllerConnectionErrorPrompt,
            bool rethrow = false, bool checkError = true, bool notifyError = true, Action<string> onError = null)
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
                    // what will happen if the object does not contain error at all?
                    error = (Error) typeof(TReturnType).GetProperty("Error")?.GetValue(ret);
                    if (error != null) hasError = true;
                }
                catch (Exception e)
                {
                    error = new Error()
                    {
                        Level = Error.Types.Level.Error,
                        Message = e.Message,
                        Timestamp = Timestamp.FromDateTime(DateTime.Now)
                    };
                    hasError = true;
                }
            }

            if (ret == null || hasError)
            {
                onError?.Invoke(error?.Message);

                if (notifyError)
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

        public static void ParsePowerCode(int powerCode, out bool master, out bool fan, out bool motor, out bool laser)
        {
            master = Convert.ToBoolean(powerCode & (1 << 0));
            laser = Convert.ToBoolean(powerCode & (1 << 1));
            motor = Convert.ToBoolean(powerCode & (1 << 2));
            fan = Convert.ToBoolean(powerCode & (1 << 3));
        }

        #region Service methods

        /// <summary>
        /// Connect to the serial port
        /// </summary>
        /// <param name="s"> com port name</param>
        public void Connect(string s)
        {
            var ret = TryInvoke(() => _rpcService.CameraControllerClient.RequestConnectToPort(
                new ConnectionRequest() {ComPort = s, Connect = true}), $"Failed to connect to {s}");

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
        }

        /// <summary>
        /// Disconnect from the current port
        /// </summary>
        /// <returns>whether the port is disconnected successfully</returns>
        public void Disconnect()
        {
            var ret = TryInvoke(() => _rpcService.CameraControllerClient.RequestConnectToPort(
                new ConnectionRequest() {Connect = false}), "Failed to disconnect");
            if (ret?.Error != null || ret == null) return;
            _eventAggregator.GetEvent<ComDisconnectedEvent>().Publish(true);
            _eventAggregator.GetEvent<NotifyOperationEvent>().Publish("COM port successfully closed");
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

        public CameraControllerTrigger StreamTrigger()
        {
            var trigger = _rpcService.CameraControllerClient.StreamRequestArmTrigger();
            return new CameraControllerTrigger(trigger);
        }

        /// <summary>
        /// Assert if serial port is already connected. <remarks>This service method will throw to interrupt the routine of code.</remarks> 
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            var ret = TryInvoke(() => _rpcService.CameraControllerClient.IsConnected(new Empty()),
                "Failed to query COM connection status");

            if (ret == null || ret?.Error != null) throw new ComRuntimeException("Cannot get connection status");
            return ret.IsConnected;
        }

        /// <summary>
        /// Control the power configuration.
        /// </summary>
        /// <param name="master">Master power</param>
        /// <param name="fan">Fan power</param>
        /// <param name="motor">Motor power</param>
        /// <param name="laser">Laser power</param>
        public void ControlPower(bool master, bool fan, bool motor, bool laser)
        {
            int powerCode = Convert.ToInt32(master) << 0 | Convert.ToInt32(laser) << 1 | Convert.ToInt32(motor) << 2 |
                            Convert.ToInt32(fan) << 3;
            TryInvoke(() =>
                _rpcService.CameraControllerClient.RequestPowerStatus(
                    new PowerStatusRequest() {PowerCode = powerCode, Write = true}),
                "Failed to set the power configuration");
        }

        /// <summary>
        /// Read the power code. Throw ComRunTimeException if error occured.
        /// <remarks>TODO: make it slient </remarks>
        /// </summary>
        /// <returns>power code</returns>
        public int ReadPower()
        {
            var ret = TryInvoke(() =>
                _rpcService.CameraControllerClient.RequestPowerStatus(new PowerStatusRequest() {Write = false}), "Failed to read the power configuration");
            if (ret == null || ret?.Error != null) throw new ComRuntimeException("Cannot get power code");
            return ret.PowerCode;
        }

        /// <summary>
        /// Control the focus motor
        /// </summary>
        /// <param name="steps">Movement steps</param>
        /// <param name="slowdown">Slow down factor</param>
        /// <param name="autoPower">Always true</param>
        /// <param name="driverPower">Always true</param>
        public void ControlFocus(int steps, int slowdown = 1000, bool autoPower = true, bool driverPower = true)
        {
            if (steps == 0) return;
            var ret = TryInvoke(() => _rpcService.CameraControllerClient.RequestFocusStatus(new FocusStatusRequest()
            {
                Steps = steps,
                SlowdownFactor = slowdown,
                DriverPower = driverPower,
                AutoPower = autoPower
            }), "Failed to control the focus motor");
        }

        /// <summary>
        /// Read current
        /// </summary>
        /// <returns></returns>
        public double GetCurrent()
        {
            var ret = TryInvoke(
                () => _rpcService.CameraControllerClient.RequestCurrentStatus(new CurrentStatusRequest()),
                "Failed to read the current");
            return ret.Current;
        }

        #endregion
    }
}