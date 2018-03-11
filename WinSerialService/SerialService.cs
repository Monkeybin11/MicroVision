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

namespace WinSerialService
{
    public partial class SerialService : ServiceBase
    {
        private Server _server;
        public SerialService()
        {
            InitializeComponent();
        }

        public void OnDebug()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            _server = SerialServiceNet.ServerConsole.CreateServer();
            _server.Start();
        }

        protected override void OnStop()
        {
            _server.ShutdownAsync().Wait();

        }
    }
}
