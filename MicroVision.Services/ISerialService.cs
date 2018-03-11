using System;
using System.Collections.Generic;
using MicroVision.Core.Models;

namespace MicroVision.Services
{
    public interface ISerialService
    {
        /// <summary>
        /// Connect to the serial port
        /// </summary>
        /// <param name="s"> com port name</param>
        /// <returns>whether the port is connected successfully</returns>
        void Connect(string s);

        /// <summary>
        /// Disconnect from the current port
        /// </summary>
        /// <returns>whether the port is disconnected successfully</returns>
        void Disconnect();

        /// <summary>
        /// get the com list
        /// </summary>
        /// <returns>null for failure</returns>
        List<string> UpdateComList();

        /// <summary>
        /// Assert if serial port is already connected
        /// </summary>
        /// <returns></returns>
        bool IsConnected();

        /// <summary>
        /// Control the power configuration.
        /// </summary>
        /// <param name="master">Master power</param>
        /// <param name="fan">Fan power</param>
        /// <param name="motor">Motor power</param>
        /// <param name="laser">Laser power</param>
        void ControlPower(bool master, bool fan, bool motor, bool laser);

        /// <summary>
        /// Read the power code. Throw ComRunTimeException if error occured.
        /// </summary>
        /// <returns>power code</returns>
        int ReadPower();

        /// <summary>
        /// Control the focus motor
        /// </summary>
        /// <param name="steps">Movement steps</param>
        /// <param name="slowdown">Slow down factor</param>
        /// <param name="autoPower">Always true</param>
        /// <param name="driverPower">Always true</param>
        void ControlFocus(int steps, int slowdown = 1000, bool autoPower = true, bool driverPower = true);

        double GetCurrent();
        CameraControllerTrigger StreamTrigger();
    }
}