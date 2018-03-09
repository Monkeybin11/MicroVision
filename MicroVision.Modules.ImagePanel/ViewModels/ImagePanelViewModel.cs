using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MicroVision.Modules.ImagePanel.ViewModels
{
    public class ImagePanelViewModel : BindableBase
    {
        private ImageSource _display = null;

        public ImageSource Display
        {
            get { return _display; }
            set { SetProperty(ref _display, value);  }
        }

        private string[] files;
        private static Random rnd = new Random();

        public ImagePanelViewModel()
        {
            files = Directory.GetFiles(@"C:\Users\wuyua\imgoutput", "*.png");
            
            var t = new Timer(1000);
            t.Elapsed += T_Elapsed;
            t.Start();
        }

        private void T_Elapsed(object sender, ElapsedEventArgs e)
        {
            int r = rnd.Next(files.Length);
            Application.Current.Dispatcher.Invoke(() => Display = new BitmapImage(new Uri( files[r])));
        }
    }
}
