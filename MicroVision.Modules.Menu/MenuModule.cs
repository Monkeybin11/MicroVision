using MicroVision.Modules.Menu.Views;
using Prism.Modularity;
using Prism.Regions;
using System;
using Microsoft.Practices.Unity;
using Prism.Unity;

namespace MicroVision.Modules.Menu
{
    public class MenuModule : IModule
    {
        private IRegionManager _regionManager;
        private IUnityContainer _container;

        public MenuModule(IUnityContainer container, IRegionManager regionManager)
        {
            _container = container;
            _regionManager = regionManager;
        }

        public void Initialize()
        {
            _container.RegisterTypeForNavigation<Views.Menu>();
            _regionManager.RegisterViewWithRegion("Menu", typeof(Views.Menu));
        }
    }
}