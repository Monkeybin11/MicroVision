using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MicroVision.Modules.Statusbar.Models;
using MicroVision.Modules.Statusbar.Notifications;
using Prism.Interactivity.InteractionRequest;

namespace MicroVision.Modules.Statusbar.ViewModels
{
    public class StatusLogViewModel : BindableBase, IInteractionRequestAware
    {
        private ObservableCollection<StatusEntry> _statusItems;
        public ObservableCollection<StatusEntry> StatusItems
        {
            get { return _statusItems; }
            set { SetProperty(ref _statusItems, value); }
        }

        private IStatusLogNotification _notification;
        public INotification Notification
        {
            get => (INotification) _notification;
            set
            {
                SetProperty(ref _notification, (IStatusLogNotification) value);
                StatusItems = new ObservableCollection<StatusEntry>(_notification.Logs);;
            }
        }

        public StatusLogViewModel()
        {
            
        }

        

        public Action FinishInteraction { get; set; }
    }
}
