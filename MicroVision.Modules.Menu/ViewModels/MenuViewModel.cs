using MicroVision.Services;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroVision.Modules.Menu.ViewModels
{
    public class MenuViewModel : BindableBase
    {
        private readonly IMenuServices _menuServices;

        public System.Windows.Controls.Menu AppMenu => _menuServices.MenuInstance;

        public MenuViewModel(IMenuServices menuServices)
        {
            _menuServices = menuServices;
        }
    }
}
