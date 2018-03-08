using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Practices.Unity;
using MicroVision.Core.Events;
using MicroVision.Services;
using MicroVision.Services.Models;
using Prism.Events;

namespace MicroVision.Modules.StatusPanel.ViewModels
{
    public class StatusPanelViewModel : BindableBase
    {
        private readonly IUnityContainer _container;
        private readonly IStatusServices _statusService;
        private readonly ILogService _logService;
        private readonly IEventAggregator _ea;

        public IStatusServices Status
        {
            get { return _statusService; }
        }
        public StatusPanelViewModel(IUnityContainer container,IStatusServices statusService, ILogService logService, IEventAggregator ea)
        {
            _container = container;
            _statusService = statusService;
            _logService = logService;
            _ea = ea;
            _logService.ConfigureLogger("StatusPanel");



        }    
    }
}
