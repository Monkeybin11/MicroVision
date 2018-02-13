﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Logging;
using RJCP.IO.Ports;
using Services;
using static Services.Error.Types;
using static Services.Error.Types.Level;

namespace SerialServiceNet
{
    public class SerialSericeImpl : CameraController.CameraControllerBase
    {
        private SerialPortStream _serialPort;
        private ConsoleLogger _logger;


        private const string FirmwareVersion = "0.1";
        private const string HardwareVersion = "0.1";
        private const string ServiceVersion = "0.1";

        public SerialSericeImpl()
        {
            _serialPort = new SerialPortStream();
            _logger = new ConsoleLogger();
        }
        /// <summary>
        /// internal log function
        /// </summary>
        /// <typeparam name="T">Exception or string</typeparam>
        /// <param name="msg"></param>
        /// <param name="level"></param>
        private void _log<T>(T msg, Level level)
        {
            if (typeof(T) == typeof(Exception))
            {
                var e = msg as Exception;
                switch (level)
                {
                    case Level.Error:
                    case Level.Fatal:
                        _logger.Error(e, e?.Message);
                        break;
                    case Level.Info:
                        _logger.Info(e?.Message);
                        break;
                    case Warning:
                        _logger.Warning(e, e?.Message);
                        break;

                }
            }
            else
            {
                var message = msg as string;
                switch (level)
                {
                    case Level.Error:
                    case Level.Fatal:
                        _logger.Error(message);
                        break;
                    case Level.Info:
                        _logger.Info(message);
                        break;
                    case Warning:
                        _logger.Warning(message);
                        break;

                }
            }
        }

        /// <summary>
        /// Internal use for build the error structure.
        /// </summary>
        /// <param name="e"> Exception thrown</param>
        /// <param name="level"> error level</param>
        /// <returns>Error object</returns>
        private Error BuildError(Exception e, Level level)
        {
            _log(e, level);
            return new Error() {Level = level, Message = e.Message, Timestamp = Timestamp.FromDateTime(DateTime.UtcNow) };
        }

        private Error BuildError(string message, Level level)
        {
            _log(message, level);
            return new Error() {Level = level, Message = message, Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)};
        }

        public override Task<Services.VersionInfo> GetInfo(Empty request, ServerCallContext context)
        {
            var version = new Services.VersionInfo(){FirmwareVersion = FirmwareVersion, HardwareVersion = HardwareVersion, ServiceVersion = ServiceVersion};
            return Task.FromResult(version);
        }

        public override Task<ConnectionResponse> IsConnected(Empty request, ServerCallContext context)
        {
            var connectionResponse = new ConnectionResponse();
            try
            {
                connectionResponse.IsConnected = _serialPort.IsOpen;
            }
            catch (Exception e)
            {
                connectionResponse.Error = BuildError(e, Level.Error);
            }
            return Task.FromResult<ConnectionResponse>(connectionResponse);
        }

        public override Task<ArmTriggerResponse> RequestArmTrigger(ArmTriggerRequest request, ServerCallContext context)
        {
            return base.RequestArmTrigger(request, context);
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
                comList.Error = BuildError(e, Fatal);
            }
            return Task.FromResult<ComList>(comList);
        }

        /// <summary>
        ///  Request to connect or disconnect. When Connect is false, the other arguments are ignored. When intend to connect, the serial port name must be provided.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<ConnectionResponse> RequestConnectToPort(ConnectionRequest request, ServerCallContext context)
        {
            var connectionResponse = new ConnectionResponse();
            if (!request.Connect)
            {
                // Request to disconnect
                if (!_serialPort.IsOpen)
                {
                    // The serial port is not open
                    connectionResponse.Error = BuildError("The serial port is not opened", Warning);
                }
                else
                {
                    // The serial port is opened and ready to be closed
                    try
                    {
                        _serialPort.Close();
                        connectionResponse.IsConnected = false;
                    }
                    catch (Exception e)
                    {
                        connectionResponse.Error = BuildError(e, Level.Error);
                    }
                }
            }
            else
            {
                // request to connect
                if (String.IsNullOrEmpty(request.ComPort))
                {
                    // Argument is invalid
                    connectionResponse.Error = BuildError("ComPort is invalid", Level.Error);
                }
                else
                {
                    try
                    {
                        _serialPort.PortName = request.ComPort;
                        _serialPort.BaudRate = 115200;
                        _serialPort.Open();
                        connectionResponse.IsConnected = true;
                    }
                    catch (Exception e)
                    {
                        connectionResponse.Error = BuildError(e, Level.Error);
                    }
                }
            }
            
            return Task.FromResult(connectionResponse);
        }

        public override Task<CurrentStatusResponse> RequestCurrentStatus(CurrentStatusRequest request, ServerCallContext context)
        {
            return base.RequestCurrentStatus(request, context);
        }

        public override Task<FocusStatusResponse> RequestFocusStatus(FocusStatusRequest request, ServerCallContext context)
        {
            return base.RequestFocusStatus(request, context);
        }

        public override Task<HardwareResetStatus> RequestHardwareReset(Empty request, ServerCallContext context)
        {
            return base.RequestHardwareReset(request, context);
        }

        public override Task<LaserStatusResponse> RequestLaserStatus(LaserStatusRequest request, ServerCallContext context)
        {
            return base.RequestLaserStatus(request, context);
        }

        public override Task<PowerStatusResponse> RequestPowerPowerStatus(PowerStatusRequest request, ServerCallContext context)
        {
            return base.RequestPowerPowerStatus(request, context);
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}