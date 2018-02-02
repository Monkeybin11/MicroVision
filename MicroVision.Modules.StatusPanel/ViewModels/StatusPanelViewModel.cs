using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MicroVision.Modules.StatusPanel.ViewModels
{
    public class StatusPanelViewModel : BindableBase
    {
        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        private Boolean _comConnected;
        public Boolean ComConnected
        {
            get { return _comConnected; }
            set { SetProperty(ref _comConnected, value); }
        }

        public StatusPanelViewModel()
        {
            Message = "View A from your Prism Module";
            Timer timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
            timer.Enabled = true;
            
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ComConnected = !ComConnected;
        }
    }
}
