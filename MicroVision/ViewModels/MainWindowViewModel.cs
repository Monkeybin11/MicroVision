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
        private readonly IServices _services;

        public MainWindowViewModel(IUnityContainer container)
        {
            _services = container.Resolve<IServices>(new ParameterOverride("moduleName", "MainWindow"));
            _services.Logger.Info("Test!");
        }

        public string Title { get; set; } = "MicroVision";
    }
}
