using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroVision.Core.Models
{
    public interface ISerialCommand
    {
        /// <summary>
        ///  Convert the command to a serialized string that will be sent via serial port
        /// </summary>
        /// <returns>target board understandable string command</returns>
        string BuildCommandString();
    }
    public class SerialCommand : ISerialCommand
    {
        public SerialCommand()
        {

        }

        public string BuildCommandString()
        {
            throw new NotImplementedException();
        }
    }
}
