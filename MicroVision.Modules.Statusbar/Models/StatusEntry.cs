using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroVision.Modules.Statusbar.Models
{
    public class StatusEntry
    {
        public StatusEntry(DateTime timestamp, string message)
        {
            Timestamp = timestamp;
            Message = message;
        }

        public DateTime Timestamp { get; }
        public string Message { get; }
    }
}
