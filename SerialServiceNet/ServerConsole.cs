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
        static int Main(string[] args)
        {
            string Uri = "localhost";
            int Port = 39945;
            var server = new Server()
            {
                Services = {CameraController.BindService(new SerialSericeImpl())},
                Ports = {new ServerPort(Uri, Port, ServerCredentials.Insecure)}
            };
            server.Start();
            Console.WriteLine($"Camera controller rpc server listens on {Uri}:{Port}");
            Console.ReadKey();
            return 0;
        }
    }
}