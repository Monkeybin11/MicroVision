using Microsoft.Practices.Unity;
using Prism.Unity;
using System.Windows;
using MicroVision.Views;

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
    }
}
