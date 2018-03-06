#define useWinServer

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CameraServiceNet;
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
        private string _cameraId = "DEV_1AB22800055C";

        public delegate void InsertRunnable();

        public void StartVimbaAndShutDown(InsertRunnable runnable = null)
        {
            {
                var response =
                    _client.VimbaInstanceControl(
                        new VimbaInstanceControlRequest() {Command = ConnectionCommands.Connect});
                Assert.IsNull(response.Error);
                Assert.IsTrue(response.IsStarted);
            }
            runnable?.Invoke();
            {
                var response =
                    _client.VimbaInstanceControl(
                        new VimbaInstanceControlRequest() {Command = ConnectionCommands.Disconnect});
                Assert.IsNull(response.Error);
                Assert.IsFalse(response.IsStarted);
            }
        }

#if useWinServer
        private Process process;
        [OneTimeSetUp]
        public void StartWinServer()
        {
            process = Process.Start(@"C:\Users\wuyua\source\repos\MicroVision\CameraServiceNet\bin\Debug\CameraServiceNet.exe");
        }

        [OneTimeTearDown]
        public void StopWinServer()
        {
            process.Kill();
        }
#endif
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
                Assert.IsTrue(response.IsConnected);

                response = _client.RequestCameraConnection(
                    new CameraConnectionRequest() {CameraID = _cameraId, Command = ConnectionCommands.Disconnect});
                Assert.IsNull(response.Error);
                Assert.IsFalse(response.IsConnected);
            });
        }

        [Test]
        public void TestConfigureCamera()
        {
            double exposureTime = 45.0;
            int numFrames = 1;
            int gain = 10;
            double frameRate = 390.0;

            StartVimbaAndShutDown(() =>
            {
                _client.RequestCameraConnection(
                    new CameraConnectionRequest() {CameraID = _cameraId, Command = ConnectionCommands.Connect});

                var response = _client.RequestCameraParameters(new CameraParametersRequest()
                {
                    Write = true,
                    Params = new CameraParameters()
                    {
                        ExposureTime = exposureTime,
                        NumFrames = numFrames,
                        Gain = gain,
                        FrameRate = frameRate
                    }
                });
                Assert.IsNull(response.Error);

                // read out
                response = _client.RequestCameraParameters(new CameraParametersRequest() {Write = false});
                Assert.IsNull(response.Error);
                Assert.IsTrue(Math.Abs(response.Params.ExposureTime - exposureTime) < 1);
                Assert.IsTrue(Math.Abs(response.Params.FrameRate - frameRate) < 1);
                Assert.IsTrue(Math.Abs(response.Params.Gain - gain) < 1);
                Assert.AreEqual(response.Params.NumFrames, numFrames);
            });
        }

        [Test]
        public void TestRequestTemperature()
        {
            StartVimbaAndShutDown(() =>
            {
                _client.RequestCameraConnection(new CameraConnectionRequest()
                {
                    CameraID = _cameraId,
                    Command = ConnectionCommands.Connect
                });

                var response = _client.RequestTemperature(new TemperatureRequest());
                Assert.IsNull(response.Error);
                TestContext.WriteLine($"The device temperature is {response.Temperature}");
            });
        }

        [Test]
        public void TestCaptureAndReadBufferedFrames()
        {
            StartVimbaAndShutDown(() =>
            {
                _client.RequestCameraConnection(new CameraConnectionRequest()
                {
                    Command = ConnectionCommands.Connect,
                    CameraID = _cameraId
                });
                var response = _client.RequestCameraAcquisition(new CameraAcquisitionRequest());
                Assert.IsNull(response.Error);

                Task.Delay(1000).Wait();

                // read frame
                var frameResponse = _client.RequestBufferedFrames(new BufferedFramesRequest());
                Assert.IsNull(frameResponse.Error);
                TestContext.WriteLine($"There are {frameResponse.Images.Count} images.");
                foreach (var images in frameResponse.Images)
                {
                    string tempImageFile = Path.GetTempFileName();
                    using (var f = File.Open(tempImageFile, FileMode.OpenOrCreate))
                    {
                        images.WriteTo(f);
                    }

                    TestContext.AddTestAttachment(tempImageFile);
                    TestContext.WriteLine($"Image written to: {tempImageFile}");
                }
            });
        }

        [Test]
        public void TestResetDevice()
        {
            StartVimbaAndShutDown(() =>
            {
                _client.RequestCameraConnection(new CameraConnectionRequest()
                {
                    Command = ConnectionCommands.Connect,
                    CameraID = _cameraId
                });
                var response = _client.RequestReset(new ResetRequest());
                Assert.IsNull(response.Error);
            });
        }
    } // test class 
}