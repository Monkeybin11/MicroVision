using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using NUnit.Framework.Constraints;
using Services;

namespace CameraServiceTest
{
    [TestFixture]
    public class TestClass
    {
        private VimbaCamera.VimbaCameraClient _client;
        private Channel _channel;
        private string _cameraId = "DEV????";

        public delegate void InsertRunnable();

        public void StartVimbaAndShutDown(InsertRunnable runnable = null)
        {
            {
                var response =
                    _client.VimbaInstanceControl(
                        new VimbaInstanceControlRequest() { Command = ConnectionCommands.Connect });
                Assert.IsNull(response.Error);
                Assert.IsTrue(response.IsStarted);
            }
            runnable?.Invoke();
            {
                var response =
                    _client.VimbaInstanceControl(
                        new VimbaInstanceControlRequest() { Command = ConnectionCommands.Disconnect });
                Assert.IsNull(response.Error);
                Assert.IsFalse(response.IsStarted);
            }
        }

        [SetUp]
        public void Setup()
        {
            try
            {
                _channel = new Channel("localhost", 39946, ChannelCredentials.Insecure);
                _client = new VimbaCamera.VimbaCameraClient(_channel);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [TearDown]
        public void TearDown()
        {
            _channel.ShutdownAsync().Wait();
        }

        [Test]
        public void TestStartVimbaAndShutDown()
        {
            StartVimbaAndShutDown();
        }

        [Test]
        public void TestGetCameraList()
        {
            StartVimbaAndShutDown(() =>
            {
                var response = _client.RequestCameraList(new CameraListRequest());
                Assert.IsNull(response.Error);
                TestContext.WriteLine(response.CameraList);
            });
        }

        [Test]
        public void TestConnectToCamera()
        {
            StartVimbaAndShutDown(() =>
            {
                var response = _client.RequestCameraConnection(
                    new CameraConnectionRequest() {CameraID = _cameraId, Command = ConnectionCommands.Connect});

                Assert.IsNull(response.Error);

            });
        }
    }
}
