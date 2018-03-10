using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RateLimiter;
using Services;
using static Services.ServiceHelper;

namespace SerialServiceNet
{
    public partial class SerialSericeImpl : CameraController.CameraControllerBase
    {
        private Object _invokeLock = new Object();

        private Error InvokeCommand(string command, string[] param, bool calledByArmTrigger = false)
        {
            string tmp = "";
            return InvokeCommandWithResponse(command, param, ref tmp, 0, calledByArmTrigger);
        }

        /// <summary>
        /// Base method to send command. 
        /// TODO: there should be an async version of this function
        /// </summary>
        /// <param name="command">command</param>
        /// <param name="param">parameter list</param>
        /// <param name="response">immediate response after the command is sent</param>
        /// <param name="timeout">wait for milliseconds before LF arrives. negative is infinite and 0 for no response</param>
        /// <returns></returns>
        private Error InvokeCommandWithResponse(string command, string[] param, ref string response, int timeout = 1000, bool calledByArmTrigger = false)
        {
            if (captureMonopoly && !calledByArmTrigger)
            {
                return BuildError("Hardware accessing is disabled during capture", Error.Types.Level.Warning);
            }
            if (!IsSerialPortAvailable())
            {
                return BuildError("COM Port not available", Error.Types.Level.Error);
            }

            var stringToSend = param != null
                ? command + param.Aggregate("", (acc, element) => acc + " " + element) + "\n"
                : command + "\n";
            try
            {
                Task<string> responseTask = null;
                lock (_invokeLock)
                {
                    if (timeout != 0)
                    {
                        // deprecated return GetImmediateResponse(ref response, timeout);
                        try
                        {
                            responseTask = _resp.WaitForResultAsync(command, timeout);
                        }
                        catch (Exception e)
                        {
                            return BuildError(e, Error.Types.Level.Error);
                        }
                    }

                    _serialPort.Write(stringToSend);
                }

                if (responseTask != null)
                {
                    responseTask.Wait();
                    response = responseTask.Result;
                }
            }
            catch (Exception e)
            {
                return BuildError(e, Error.Types.Level.Error);
            }

            return null;
        }

        /// <summary>
        /// Deprecated sync code
        /// </summary>
        /// <param name="response"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private Error GetImmediateResponse(ref string response, int timeout)
        {
            try
            {
                _serialPort.ReadTimeout = timeout;
                response = _serialPort.ReadLine();
            }
            catch (Exception e)
            {
                return BuildError(e, Error.Types.Level.Error);
            }

            return null;
        }

        private bool IsSerialPortAvailable()
        {
            return (!_serialPort.IsDisposed && _serialPort.IsOpen);
        }
    }
}