using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using NUnit.Framework;
using Services;

namespace CameraServiceTest
{
    [TestFixture]
    public class StreamFrame
    {
        private Channel _channel;
        private VimbaCamera.VimbaCameraClient _client;

        private void ThrowIfError(object o)
        {
            object err = o.GetType().GetProperty("Error")?.GetValue(o);
            if (err != null) throw new Exception();
        }

        [SetUp]
        public void Setup()
        {
            _channel = new Channel("localhost:39946", ChannelCredentials.Insecure);
            _client = new VimbaCamera.VimbaCameraClient(_channel);

            ThrowIfError(
                _client.VimbaInstanceControl(new VimbaInstanceControlRequest() {Command = ConnectionCommands.Connect}));
            ThrowIfError(_client.RequestCameraConnection(new CameraConnectionRequest()
            {
                Command = ConnectionCommands.Connect,
                CameraID = TestClass.CameraId
            }));

            ThrowIfError(_client.RequestReset(new ResetRequest()));

            Task.Delay(1000).Wait();

            ThrowIfError(_client.RequestCameraConnection(new CameraConnectionRequest()
            {
                Command = ConnectionCommands.Connect,
                CameraID = TestClass.CameraId
            }));
        }

        [TearDown]
        public void Teardown()
        {
            ThrowIfError(_client.RequestCameraConnection(new CameraConnectionRequest()
            {
                Command = ConnectionCommands.Disconnect
            }));
            ThrowIfError(
                _client.VimbaInstanceControl(
                    new VimbaInstanceControlRequest() {Command = ConnectionCommands.Disconnect}));
        }

        [Test]
        public void TestStreamFrame()
        {
            var ret = _client.RequestCameraParameters(new CameraParametersRequest()
            {
                Params = new CameraParameters() {NumFrames = 1, ExposureTime = 100000, Gain = 10, FrameRate = 30}
            });
            Assert.IsNull(ret.Error);

            var stream = _client.RequestFrameStream();

            for (int i = 0; i < 5; i++)
            {
                stream.RequestStream.WriteAsync(new CameraAcquisitionRequest()).Wait();
                TestContext.WriteLine($"{DateTime.Now} Requested");
                stream.ResponseStream.MoveNext(CancellationToken.None).Wait();
                TestContext.WriteLine($"{DateTime.Now} Received");

                var current = stream.ResponseStream.Current;
                Assert.IsNull(current.Error);
                var image = current.Images;

                using (var f = File.Open($"{i}.png", FileMode.OpenOrCreate))
                {
                    image[0].WriteTo(f);
                }
                Task.Delay(500).Wait();
            }
        }
    }
}