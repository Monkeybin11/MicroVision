using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Services;

namespace SerialServiceNet
{
    public static class ServerConsole
    {
        public static Server CreateServer()
        {
            string Uri = "localhost";
            int Port = 39945;
            var server = new Server()
            {
                Services = { CameraController.BindService(new SerialSericeImpl()) },
                Ports = { new ServerPort(Uri, Port, ServerCredentials.Insecure) }
            };
            return server;
        }
        public static int Main(string[] args)
        {
            var server = CreateServer();
            server.Start();
            Console.ReadKey();
            return 0;
        }
    }
}