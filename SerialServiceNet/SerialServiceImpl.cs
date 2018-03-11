using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Logging;
using RateLimiter;
using RJCP.IO.Ports;
using Services;
using static Services.Error.Types;
using static Services.Error.Types.Level;

namespace SerialServiceNet
{
    public partial class SerialSericeImpl : CameraController.CameraControllerBase
    {
        private SerialPortStream _serialPort;


        private const string FirmwareVersion = "0.1";
        private const string HardwareVersion = "0.1";
        private const string ServiceVersion = "0.1";
        private ResponseDispatcher _resp = new ResponseDispatcher();
        private int _cancellationTimeout = 1000;
        private Task _dataListener;
        private bool captureMonopoly = false;

#if DEBUG
        private SerialConversationLogger _conversationLogger;
        private FileStream _conversationFileStream;
#endif
        public SerialSericeImpl()
        {
            _serialPort = new SerialPortStream();

#if DEBUG
            _conversationFileStream = new FileStream("conversation_log.txt", FileMode.Create);
            _conversationLogger = new SerialConversationLogger(_conversationFileStream);
#endif
        }

        private void ReadStreamListener()
        {
            _serialPort.Flush();
            while (IsSerialPortAvailable())
            {
                try
                {
                    var line = _serialPort.ReadLine();
                    _resp.FeedMessage(line);
#if DEBUG
                    _conversationLogger.ReceivedBySerial(line);
#endif
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// Get version info
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<Services.VersionInfo> GetInfo(Empty request, ServerCallContext context)
        {
            var version = new Services.VersionInfo()
            {
                FirmwareVersion = FirmwareVersion,
                HardwareVersion = HardwareVersion,
                ServiceVersion = ServiceVersion
            };
            return Task.FromResult(version);
        }

        /// <summary>
        /// Check if the com port has been connected
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<ConnectionResponse> IsConnected(Empty request, ServerCallContext context)
        {
            var connectionResponse = new ConnectionResponse();
            try
            {
                connectionResponse.IsConnected = _serialPort.IsOpen;
            }
            catch (Exception e)
            {
                connectionResponse.Error = ServiceHelper.BuildError(e, Level.Error);
            }

            return Task.FromResult<ConnectionResponse>(connectionResponse);
        }


        public override Task<ArmTriggerResponse> RequestArmTrigger(ArmTriggerRequest request, ServerCallContext context)
        {
            captureMonopoly = true;
            var response = new ArmTriggerResponse();
            var laserConfiguration = request.LaserConfiguration;
            response.Error = InvokeCommand("P", new[] {laserConfiguration.Intensity.ToString()}, true);
            if (response.Error != null) goto exit;

            response.Error = InvokeCommand("D", new[] {laserConfiguration.DurationUs.ToString()}, true);
            if (response.Error != null) goto exit;

            response.Error = InvokeCommand("M", new[] {request.MaxTriggerTimeUs.ToString()}, true);
            if (response.Error != null) goto exit;

            response.TriggerAutoDisarmed = false;
            if (request.ArmTrigger)
            {
                string outString = "";
                response.Error = InvokeCommandWithResponse("B", null, ref outString, calledByArmTrigger: true);
                if (response.Error != null) goto exit;

                response.TriggerAutoDisarmed = outString == "1";
            }

            exit:
            captureMonopoly = false;
            return Task.FromResult(response);
        }

        /// <summary>
        /// Get the available Com ports
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<ComList> RequestComList(ComListRequest request, ServerCallContext context)
        {
            var comList = new ComList();
            try
            {
                var comPorts = SerialPortStream.GetPortNames();
                comList.ComPort.AddRange(comPorts);
            }
            catch (Exception e)
            {
                comList.Error = ServiceHelper.BuildError(e, Fatal);
            }

            return Task.FromResult<ComList>(comList);
        }

        /// <summary>
        ///  Request to connect or disconnect. When Connect is false, the other arguments are ignored. When intend to connect, the serial port name must be provided.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<ConnectionResponse> RequestConnectToPort(ConnectionRequest request,
            ServerCallContext context)
        {
            var connectionResponse = new ConnectionResponse();
            if (!request.Connect)
            {
                // Request to disconnect
                if (!_serialPort.IsOpen)
                {
                    // The serial port is not open
                    connectionResponse.Error = ServiceHelper.BuildError("The serial port is not opened", Warning);
                    return Task.FromResult(connectionResponse);
                }

                // The serial port is opened and ready to be closed
                try
                {
                    _serialPort.Close();
                    _dataListener.Wait(_cancellationTimeout);
                    connectionResponse.IsConnected = false;
                }
                catch (Exception e)
                {
                    connectionResponse.Error = ServiceHelper.BuildError(e, Level.Error);
                    return Task.FromResult(connectionResponse);
                }
            }
            else
            {
                // request to connect
                if (String.IsNullOrEmpty(request.ComPort))
                {
                    // Argument is invalid
                    connectionResponse.Error = ServiceHelper.BuildError("ComPort is invalid", Level.Error);
                    return Task.FromResult(connectionResponse);
                }

                try
                {
                    if (_serialPort.IsOpen)
                    {
                        connectionResponse.Error = ServiceHelper.BuildError("Port already opened", Warning);
                        return Task.FromResult(connectionResponse);
                    }

                    _serialPort.PortName = request.ComPort;
                    _serialPort.BaudRate = 115200;
                    _serialPort.Open();
                    _dataListener = Task.Factory.StartNew(ReadStreamListener);
                    connectionResponse.IsConnected = true;
                }
                catch (Exception e)
                {
                    connectionResponse.Error = ServiceHelper.BuildError(e, Level.Error);
                    return Task.FromResult(connectionResponse);
                }
            }

            return Task.FromResult(connectionResponse);
        }

        /// <summary>
        /// Get current reading
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<CurrentStatusResponse> RequestCurrentStatus(CurrentStatusRequest request,
            ServerCallContext context)
        {
            var currentResponse = new CurrentStatusResponse();
            string result = "";
            currentResponse.Error = InvokeCommandWithResponse("i", null, ref result);
            if (currentResponse.Error != null)
            {
                return Task.FromResult(currentResponse);
            }

            try
            {
                currentResponse.Current = Double.Parse(result);
            }
            catch (Exception e)
            {
                currentResponse.Error = ServiceHelper.BuildError(e, Level.Error);
                return Task.FromResult(currentResponse);
            }

            return Task.FromResult(currentResponse);
        }

        /// <summary>
        /// Access and control the focus motor
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<FocusStatusResponse> RequestFocusStatus(FocusStatusRequest request,
            ServerCallContext context)
        {
            var focusStatusResponse = new FocusStatusResponse();

            bool autoPower = request.AutoPower;
            if (!autoPower)
            {
                focusStatusResponse.Error = InvokeCommand(request.DriverPower ? "V" : "v", null);
                if (focusStatusResponse.Error != null) return Task.FromResult(focusStatusResponse);
            }

            // set the slow down factor
            if (request.SlowdownFactor != 0)
                focusStatusResponse.Error = InvokeCommand("Y", new[] {request.SlowdownFactor.ToString()});
            if (focusStatusResponse.Error != null) return Task.FromResult(focusStatusResponse);

            // get the slow down factor
            string output = "";
            focusStatusResponse.Error = InvokeCommandWithResponse("y", null, ref output);
            if (focusStatusResponse.Error != null) return Task.FromResult(focusStatusResponse);
            try
            {
                focusStatusResponse.SlowdownFactor = Int32.Parse(output);
            }
            catch (Exception e)
            {
                focusStatusResponse.Error = ServiceHelper.BuildError(e, Level.Error);
                return Task.FromResult(focusStatusResponse);
            }

            // no need to perform step;
            if (request.Steps == 0) return Task.FromResult(focusStatusResponse);

            if (autoPower) focusStatusResponse.Error = InvokeCommand("V", null);
            if (focusStatusResponse.Error != null) return Task.FromResult(focusStatusResponse);

            Task.Delay(50).Wait();
            string stepResponse = "";
            // This timeout should be extremely long or even disabled
            focusStatusResponse.Error =
                InvokeCommandWithResponse("S", new[] {request.Steps.ToString()}, ref stepResponse, -1);
            if (autoPower) focusStatusResponse.Error = InvokeCommand("v", null);
            return Task.FromResult(focusStatusResponse);
        }

        /// <summary>
        /// Reset the controller
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<SoftwareResetStatus> RequestSoftwareReset(Empty request, ServerCallContext context)
        {
            return Task.FromResult<SoftwareResetStatus>(new SoftwareResetStatus()
            {
                Error = InvokeCommand("SWRESET", null)
            });
        }

        /// <summary>
        /// Direct access to the laser control. Currently disabled for safety concern
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<LaserStatusResponse> RequestLaserStatus(LaserStatusRequest request,
            ServerCallContext context)
        {
            return Task.FromResult(new LaserStatusResponse()
            {
                Error = ServiceHelper.BuildError(
                    new NotImplementedException(
                        "For safety reason the laser is not accessible by the interface, please use the arm trigger feature to enable laser"),
                    Level.Error)
            });
        }

        /// <summary>
        /// Control the power configuration.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<PowerStatusResponse> RequestPowerStatus(PowerStatusRequest request,
            ServerCallContext context)
        {
            var powerStatusResponse = new PowerStatusResponse();

            if (request.Write)
            {
                // change the power configuration (Write mode)
                if (request.PowerCode < 0 || request.PowerCode > 15)
                {
                    powerStatusResponse.Error = ServiceHelper.BuildError("Power code is invalid", Level.Error);
                    return Task.FromResult<PowerStatusResponse>(powerStatusResponse);
                }

                powerStatusResponse.Error = InvokeCommand("R", new string[] {request.PowerCode.ToString()});
                powerStatusResponse.PowerCode = request.PowerCode;
                return Task.FromResult(powerStatusResponse);
            }
            else
            {
                // Read mode
                string outputValue = "";
                powerStatusResponse.Error = InvokeCommandWithResponse("r", null, ref outputValue, 1000);
                if (powerStatusResponse.Error != null)
                {
                    return Task.FromResult(powerStatusResponse);
                }

                try
                {
                    powerStatusResponse.PowerCode = Int32.Parse(outputValue);
                }
                catch (Exception e)
                {
                    powerStatusResponse.Error = ServiceHelper.BuildError(e, Level.Error);
                    return Task.FromResult(powerStatusResponse);
                }
            }

            return Task.FromResult(powerStatusResponse);
        }

        public override async Task StreamRequestArmTrigger(IAsyncStreamReader<ArmTriggerRequest> requestStream,
            IServerStreamWriter<ArmTriggerResponse> responseStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var current = requestStream.Current;
                await responseStream.WriteAsync(await RequestArmTrigger(current, context));
            }
        }
    }
}