using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;

namespace WinCameraService
{
    public partial class CameraService : ServiceBase
    {
        private Server _server;
        public CameraService()
        {
            InitializeComponent();

        }

        protected override void OnStart(string[] args)
        {
            _server = CameraServiceNet.ServerConsole.CreateServer();
            _server.Start();
        }

        public void OnDebug()
        {
            OnStart(null);
        }

        protected override void OnStop()
        {
            _server.ShutdownAsync().Wait();
        }
    }
}
