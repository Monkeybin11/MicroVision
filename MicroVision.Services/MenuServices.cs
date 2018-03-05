using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Prism.Commands;

namespace MicroVision.Services
{
    public interface IMenuServices
    {
        Menu MenuInstance { get; }
        void AddMenuItem(MenuItem item);

    }
    public class MenuServices:IMenuServices
    {
        public MenuServices()
        {
            _menuInstance = new Menu();
            _menuInstance.IsMainMenu = true;
            MenuItem item = new MenuItem();
            item.Command = new DelegateCommand(() => MessageBox.Show("Test"));
            item.Header = "_Test";
            AddMenuItem(item);
        }

        private readonly Menu _menuInstance;

        public Menu MenuInstance => _menuInstance;

        public void AddMenuItem(MenuItem item)
        {
            _menuInstance.Items.Add(item);
        }
    }
}
