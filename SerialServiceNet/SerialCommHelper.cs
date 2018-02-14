using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services;

namespace SerialServiceNet
{
    public partial class SerialSericeImpl : CameraController.CameraControllerBase
    {
        private Object _invokeLock = new Object();
        private Error InvokeCommand(string command, string[] param)
        {
            string tmp = "";
            return InvokeCommandWithResponse(command, param, ref tmp, 0);
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
        private Error InvokeCommandWithResponse(string command, string[] param, ref string response, int timeout = 1000)
        {
            if (! IsSerialPortAvailable())
            {
                return BuildError("COM Port not available", Error.Types.Level.Error);
            }

            var stringToSend = param != null ? command + param.Aggregate("", (acc, element) => acc + " " + element) + "\n" : command + "\n";
            try
            {
                lock (_invokeLock)
                {
                    _serialPort.Write(stringToSend);
                    if (timeout != 0)
                    {
                        return GetImmediateResponse(ref response, timeout);
                    }
                }
            }
            catch (Exception e)
            {
                return BuildError(e, Error.Types.Level.Error);
            }

            return null;
        }
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
