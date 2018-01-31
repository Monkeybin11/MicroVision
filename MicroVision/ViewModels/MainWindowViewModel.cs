using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using MicroVision.Services;
using Prism.Mvvm;

namespace MicroVision.ViewModels
{
    
    class MainWindowViewModel: BindableBase
    {
        private readonly ILogService _logService;

        public MainWindowViewModel(ILogService logservice)
        {
            _logService = logservice;
            _logService.ConfigureLogger(this.GetType().Name);
        }

        public string Title { get; set; } = "MicroVision";
    }
}
