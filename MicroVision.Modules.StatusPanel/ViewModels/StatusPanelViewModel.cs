using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Practices.Unity;
using MicroVision.Services;
using MicroVision.Services.Models;

namespace MicroVision.Modules.StatusPanel.ViewModels
{
    public class StatusPanelViewModel : BindableBase
    {
        private readonly IUnityContainer _container;
        private readonly ILogService _logService;


        public StatusPanelViewModel(IUnityContainer container,IStatusService statusService, ILogService logService)
        {
            _container = container;
            _logService = logService;
            _logService.ConfigureLogger("StatusPanel");

            ComConnectionStatus = statusService.ComConnectionStatus;
            VimbaConnectionStatus = statusService.VimbaConnectionStatus;
            Timer timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
            timer.Enabled = true;
            
        }

        public ComConnectionStatus ComConnectionStatus { get; }
        public VimbaConnectionStatus VimbaConnectionStatus { get; }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (ComConnectionStatus.IsConnected)
            {
                ComConnectionStatus.Disconnected();
                VimbaConnectionStatus.ResetError();
                VimbaConnectionStatus.Connected();
            }
            else
            {
                ComConnectionStatus.Connected();
                VimbaConnectionStatus.RaiseError("Test Error!");
            }

            var status = (IStatusService)_container.Resolve<StatusServices>();
            _logService.Logger.Info(status.ComConnectionStatus.IsConnected);


        }
    }
}
