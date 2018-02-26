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
        public static int Main(string[] args)
        {
            string Uri = "localhost";
            int Port = 39946;
            var server = new Server()
            {
                Services = { VimbaCamera.BindService(new CameraServiceImpl()) },
                Ports = { new ServerPort(Uri, Port, ServerCredentials.Insecure) }
            };
            server.Start();
            Console.WriteLine($"Camera rpc server listens on {Uri}:{Port}");
            Console.ReadKey();
            return 0;
        }
    }
}
