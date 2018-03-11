using MicroVision.Modules.ImagePanel.Views;
using Prism.Modularity;
using Prism.Regions;
using System;
using Microsoft.Practices.Unity;
using Prism.Unity;

namespace MicroVision.Modules.ImagePanel
{
    public class ImagePanelModule : IModule
    {
        private IRegionManager _regionManager;
        private IUnityContainer _container;

        public ImagePanelModule(IUnityContainer container, IRegionManager regionManager)
        {
            _container = container;
            _regionManager = regionManager;
        }

        public void Initialize()
        {
            _container.RegisterTypeForNavigation<Views.ImagePanel>();
            _regionManager.RegisterViewWithRegion("ImagePanel", typeof(Views.ImagePanel));

        }
    }
}