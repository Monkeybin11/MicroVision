using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroVision.Modules.Statusbar.Models;
using MicroVision.Modules.Statusbar.Views;
using Prism.Interactivity.InteractionRequest;

namespace MicroVision.Modules.Statusbar.Notifications
{
    public interface IStatusLogNotification
    {
        List<StatusEntry> Logs { get; }

    }

    public class StatusLogNotification : IStatusLogNotification, INotification
    {
        public List<StatusEntry> Logs { get; }
        public string Title { get; set; }
        public object Content { get; set; }

        public StatusLogNotification(List<StatusEntry> log)
        {
            Title = "Status Log";
            Content = null;
            Logs = log;
        }
    }
}
