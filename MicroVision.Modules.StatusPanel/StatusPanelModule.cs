using MicroVision.Modules.StatusPanel.Views;
using Prism.Modularity;
using Prism.Regions;
using System;
using Microsoft.Practices.Unity;
using MicroVision.Services;
using Prism.Unity;

namespace MicroVision.Modules.StatusPanel
{
    public class StatusPanelModule : IModule
    {
        #region private members
        private IRegionManager _regionManager;
        //private readonly IStatusService _statusService;
        private IUnityContainer _container;
        #endregion

        #region properties
        public IStatusService StatusService { get; private set; }
        #endregion
        public StatusPanelModule(IUnityContainer container, IRegionManager regionManager, IStatusService statusService)
        {
            _container = container;
            _regionManager = regionManager;
            //_statusService = statusService;
            StatusService = statusService;
        }

        public void Initialize()
        {
            _container.RegisterTypeForNavigation<Views.StatusPanel>();
            _regionManager.RegisterViewWithRegion("StatusPanel", typeof(Views.StatusPanel));
        }
    }
}