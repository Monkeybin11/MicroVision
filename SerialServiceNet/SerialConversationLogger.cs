using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialServiceNet
{
    public class SerialConversationLogger
    {
        private readonly Stream _logStream;

        public SerialConversationLogger(Stream logStream)
        {
            _logStream = logStream;
        }

        public void WrittenToSerial(string command)
        {
            var sw = new StreamWriter(_logStream);
            sw.Write(encode(command, '>'));
            sw.Flush();
        }

        public void ReceivedBySerial(string command)
        {
            var sw = new StreamWriter(_logStream);
            sw.Write(encode(command, '<'));
            sw.Flush();
        }

        private string encode(string s, char dirSymbol)
        {
            var noNewLine = s.Replace("\n", "[NewLine]\n");
            return $"{DateTime.Now.Ticks} {dirSymbol} {noNewLine}";
        }
    }
}
