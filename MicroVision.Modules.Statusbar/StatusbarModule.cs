using MicroVision.Modules.Statusbar.Views;
using Prism.Modularity;
using Prism.Regions;
using System;
using Microsoft.Practices.Unity;
using Prism.Unity;

namespace MicroVision.Modules.Statusbar
{
    public class StatusbarModule : IModule
    {
        private IRegionManager _regionManager;
        private IUnityContainer _container;

        public StatusbarModule(IUnityContainer container, IRegionManager regionManager)
        {
            _container = container;
            _regionManager = regionManager;
        }

        public void Initialize()
        {
            _container.RegisterType<StatusBar>();
            _regionManager.RegisterViewWithRegion("StatusBar", typeof(StatusBar));
        }
    }
}