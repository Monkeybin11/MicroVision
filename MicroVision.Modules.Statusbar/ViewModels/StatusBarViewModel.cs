using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Prism.Events;
using MicroVision.Core.Events;
using Prism.Interactivity.InteractionRequest;

namespace MicroVision.Modules.Statusbar.ViewModels
{
    public class StatusBarViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private ImageSource _statusIcon;
        public ImageSource StatusIcon
        {
            get { return _statusIcon; }
            set { SetProperty(ref _statusIcon, value); }
        }
        private string _status;
        public string Status
        {
            get { return _status; }
            set { SetProperty(ref _status, value); }
        }

        private DelegateCommand _ShowStatusLogCommand;
        public DelegateCommand ShowStatusLogCommand =>
            _ShowStatusLogCommand ?? (_ShowStatusLogCommand = new DelegateCommand(ExecuteShowStatusLogCommand));

        public InteractionRequest<INotification> ShowStatusLogRequest { get; } = new InteractionRequest<INotification>();

        void ExecuteShowStatusLogCommand()
        {
            InitializeStatus();
            ShowStatusLogRequest.Raise(new Notification(){Title = "Status Log"});
        }

        public StatusBarViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<ExceptionEvent>().Subscribe(NotifyException);


            InitializeStatus();
        }

        private void InitializeStatus()
        {
            Status = "No Message";
            StatusIcon = CreateIcon(SystemIcons.Information);
        }

        private static ImageSource CreateIcon(Icon icon)
        {
            return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }
        private void NotifyException(Exception exception)
        {
            StatusIcon = CreateIcon(SystemIcons.Error);
            Status = exception.Message;
        }
    }
}
