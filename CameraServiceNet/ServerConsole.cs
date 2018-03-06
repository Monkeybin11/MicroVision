using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Services;

namespace CameraServiceNet
{
    public class ServerConsole
    {
        public static Server CreateServer()
        {
            string Uri = "localhost";
            int Port = 39946;
            var server = new Server()
            {
                Services = { VimbaCamera.BindService(new CameraServiceImpl()) },
                Ports = { new ServerPort(Uri, Port, ServerCredentials.Insecure) }
            };
            return server;
        }
        public static int Main(string[] args)
        {
            var server = CreateServer();
            server.Start();
            Console.Read();
            return 0;
        }
    }
}
