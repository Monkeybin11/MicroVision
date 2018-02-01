using MicroVision.Modules.StatusPanel.Views;
using Prism.Modularity;
using Prism.Regions;
using System;
using Microsoft.Practices.Unity;
using Prism.Unity;

namespace MicroVision.Modules.StatusPanel
{
    public class StatusPanelModule : IModule
    {
        private IRegionManager _regionManager;
        private IUnityContainer _container;

        public StatusPanelModule(IUnityContainer container, IRegionManager regionManager)
        {
            _container = container;
            _regionManager = regionManager;
        }

        public void Initialize()
        {
            _container.RegisterTypeForNavigation<Views.StatusPanel>();
            _regionManager.RegisterViewWithRegion("StatusPanel", typeof(Views.StatusPanel));
        }
    }
}