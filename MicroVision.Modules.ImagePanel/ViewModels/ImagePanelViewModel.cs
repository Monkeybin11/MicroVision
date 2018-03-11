using MicroVision.Services;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private readonly ICameraService _cameraService;
        private BitmapImage _display = null;

        public BitmapImage Display
        {
            get { return _display; }
            set { SetProperty(ref _display, value);  }
        }



        public ImagePanelViewModel(ICameraService cameraService )
        {
            _cameraService = cameraService;
            _cameraService.PropertyChanged += CameraServiceOnPropertyChanged;
        }

        private void CameraServiceOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var bitmap = new BitmapImage();
            var ms = new MemoryStream(_cameraService.Image);
            bitmap.BeginInit();
            bitmap.StreamSource = ms;
            bitmap.EndInit();
            bitmap.Freeze();
            Application.Current.Dispatcher.Invoke(()=>Display = bitmap);
        }
    }
}
