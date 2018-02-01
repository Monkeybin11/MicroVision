using System.Runtime.InteropServices;
using Microsoft.Practices.Unity;
using Prism.Unity;
using System.Windows;
using MicroVision.Modules.ParameterPanel;
using MicroVision.Modules.StatusPanel;
using MicroVision.Modules.StatusPanel.Views;
using MicroVision.Services;

/*
using MicroVision.Services;
*/
using MicroVision.Views;
using Prism.Modularity;

namespace MicroVision
{
    class Bootstrapper : UnityBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return this.Container.Resolve<MainWindow>();
        }
        protected override void InitializeShell()
        {
            if (Application.Current.MainWindow != null)
                Application.Current.MainWindow.Show();
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

            //Container.RegisterType<IServices, Services.Services>(new InjectionConstructor(typeof(string)));
            Container.RegisterType<ILogService, LogService>(new PerResolveLifetimeManager());
            Container.RegisterType<IParameterService, ParameterService>(new ContainerControlledLifetimeManager());
        }

        protected override void ConfigureModuleCatalog()
        {
            base.ConfigureModuleCatalog();

            var catalog = (ModuleCatalog)ModuleCatalog;
            catalog.AddModule(typeof(ParameterPanel));
            catalog.AddModule(typeof(StatusPanelModule));
        }
    }
}
