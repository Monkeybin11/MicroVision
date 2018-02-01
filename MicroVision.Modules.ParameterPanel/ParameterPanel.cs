using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Modularity;
using Prism.Regions;

namespace MicroVision.Modules.ParameterPanel
{
    public class ParameterPanel : IModule
    {
        private IRegionManager _regionManager;

        public ParameterPanel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void Initialize()
        {
            _regionManager.RegisterViewWithRegion("ParameterPanel", typeof(Views.ParameterPanel));
        }
    }
}
