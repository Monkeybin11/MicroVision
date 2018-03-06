using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Practices.Unity;
using MicroVision.Core.Events;
using MicroVision.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;

namespace MicroVision.ViewModels
{
    class MainWindowViewModel : BindableBase
    {
        private readonly ILogService _logService;
        private readonly IEventAggregator _eventAggregator;

        public InteractionRequest<IConfirmation> RaiseRpcConnectionFailedDialog { get; set; } =
            new InteractionRequest<IConfirmation>();


        public MainWindowViewModel(ILogService logservice, IEventAggregator eventAggregator)
        {
            _logService = logservice;
            _eventAggregator = eventAggregator;
            _logService.ConfigureLogger(GetType().Name);
            _eventAggregator.GetEvent<HardwareRpcConnedtionFailedEvent>().Subscribe(RpcServerConnectionFailedHandler, ThreadOption.UIThread);
        }

        private void RpcServerConnectionFailedHandler(string s)
        {
            RaiseRpcConnectionFailedDialog.Raise(new Confirmation()
            {
                Content =
                    $"Fatal Error Occured: {s}. Please check the backend server configuration in app.config file. The program will close.",
                Title = "Error"
            }, confirmation =>
            {
                if (confirmation.Confirmed)
                {
                    Application.Current.Shutdown();
                }
            });
        }

        public string Title { get; set; } = "MicroVision";
    }
}