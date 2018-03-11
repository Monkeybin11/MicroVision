using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using NUnit.Framework;
using Services;

namespace SerialServiceTest
{
    [TestFixture]
    public class SerialTestBase
    {
        protected string Uri = "localhost";
        protected int Port = 39945;

        protected Channel channel;
        protected CameraController.CameraControllerClient client;

        protected string _comPort = "COM23";
        protected TextWriter _writer;

        [SetUp]
        public void Init()
        {
            _writer = TestContext.Out;
            channel = new Channel(Uri, Port, ChannelCredentials.Insecure);
            client = new CameraController.CameraControllerClient(channel);
        }

        [TearDown]
        public void Cleanup()
        {
            // shutdown the COM connection
            client.RequestPowerStatus(new PowerStatusRequest() { Write = true, PowerCode = 0 });
            client.RequestConnectToPort(new ConnectionRequest() { Connect = false });
            channel.ShutdownAsync().Wait();
        }
    }
}
