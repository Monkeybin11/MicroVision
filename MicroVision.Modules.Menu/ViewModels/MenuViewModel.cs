using MicroVision.Services;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MicroVision.Core.Events;
using MicroVision.Modules.Menu.Properties;
using Prism.Events;

namespace MicroVision.Modules.Menu.ViewModels
{
    public class MenuViewModel : BindableBase
    {
        private readonly IMenuServices _menuServices;
        private readonly IEventAggregator _eventAggregator;

        public System.Windows.Controls.Menu AppMenu => _menuServices.MenuInstance;

        public ICommand SaveCommand { get; }
        public ICommand SaveAsCommand { get; }
        public ICommand LoadCommand { get; }

        private void SaveAsCommandHandler()
        {
            var result = SelectFileDlg();
            if (result != null)
            {
                _eventAggregator.GetEvent<SaveAsEvent>().Publish(result);
            }
        }

        private void LoadCommandHandler()
        {
            var result = SelectFileDlg();
            if (result != null)
            {
                 _eventAggregator.GetEvent<LoadEvent>().Publish(result);
            }
        }

        public MenuViewModel(IMenuServices menuServices, IEventAggregator eventAggregator)
        {
            _menuServices = menuServices;
            _eventAggregator = eventAggregator;

            SaveCommand = new DelegateCommand(() => _eventAggregator.GetEvent<SaveEvent>().Publish());
            SaveAsCommand = new DelegateCommand(SaveAsCommandHandler);
            LoadCommand = new DelegateCommand(LoadCommandHandler);

            MenuItem item = (MenuItem) Application.Current.Resources["ProfileFileMenuItem"];
            _menuServices.AddMenuItem(item);
        }

        public string SelectFileDlg()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".json";
            dlg.Filter = "JSON Files (*.json)|*.json|All Files|*.*";
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                return dlg.FileName;
            }

            return null;
        }
    }
}
