using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using MicroVision.Services;

namespace MicroVision.Modules.StatusPanel.ViewModels
{
    public class StatusPanelViewModel : BindableBase
    {
        private readonly IStatusService _statusService;
        public IStatusService StatusService => _statusService;

        public StatusPanelViewModel(IStatusService statusService)
        {
            _statusService = statusService;
 
            Timer timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
            timer.Enabled = true;
            
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (StatusService.ComConnectionStatus.IsConnected)
            {
                StatusService.ComConnectionStatus.Disconnected();
                StatusService.VimbaConnectionStatus.ResetError();
                StatusService.VimbaConnectionStatus.Connected();
            }
            else
            {
                StatusService.ComConnectionStatus.Connected();
                StatusService.VimbaConnectionStatus.RaiseError("Test Error!");
            }

        }
    }
}
