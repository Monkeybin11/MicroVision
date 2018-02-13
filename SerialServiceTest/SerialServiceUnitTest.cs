﻿using System;
using System.Runtime.Remoting.Channels;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NUnit.Framework;
using Services;

namespace SerialServiceTest
{
    [TestFixture]
    [SingleThreaded]
    public class SerialServiceBasicUnitTest
    {
        private string Uri = "localhost";
        private int Port = 39945;

        private Channel channel;
        private CameraController.CameraControllerClient client;
        private TestContext testContextInstance;

        private string _comPort = "COM23";

        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        [SetUp]
        public void Init()
        {
            channel = new Channel(Uri, Port, ChannelCredentials.Insecure);
            client = new CameraController.CameraControllerClient(channel);
        }

        [Category("Basic")]
        [Test]
        public void TestGetInfo ()
        {
            var info = client.GetInfo(new Empty());
            TestContext.WriteLine(
                $"Info package: \n \tFirmware = {info.FirmwareVersion} \n \tHardware = {info.HardwareVersion} \n \tService = {info.ServiceVersion}");
        }

        [Category("Basic")]
        [Test]
        public async Task TestGetInfoAsync()
        {
            var info = await client.GetInfoAsync(new Empty());
            TestContext.WriteLine($"Info package: \n \tFirmware = {info.FirmwareVersion} \n \tHardware = {info.HardwareVersion} \n \tService = {info.ServiceVersion}");
        }

        [Category("Basic")]
        [Test]
        public void TestIsConnected()
        {
            var isConnected = client.IsConnected(new Empty());
            TestContext.WriteLine($"IsConnected={isConnected.IsConnected}");
        }

        [Category("Basic")]
        [Test]
        public void TestGetComList()
        {
            var comList = client.RequestComList(new ComListRequest());
            TestContext.WriteLine($"COM Detected: {comList.ComPort}");
        }

        [Category("Basic")]
        [Test]
        public void TestConnectAndClose()
        {
            var connect = client.RequestConnectToPort(new ConnectionRequest() {ComPort = _comPort, Connect = true});
            Assert.IsNull(connect.Error);
            var disconnect = client.RequestConnectToPort(new ConnectionRequest() { Connect = false });
            Assert.IsNull(disconnect.Error);
        }

        [Category("Nauty")]
        [Description("Should it fail when the connection argument is invalid")]
        [Test]
        public void TestConnectWithoutName()
        {
            var connect = client.RequestConnectToPort(new ConnectionRequest() {Connect = true });
            Assert.NotNull(connect.Error);
            TestContext.WriteLine($"When no COM name is given, you will get error {connect.Error}");
        }

        [Category("Nauty")]
        [Description("Should it fail when the connection is already opened")]
        [Test]
        public void TestRepeatedOpen()
        {
            var connect = client.RequestConnectToPort(new ConnectionRequest() { ComPort = _comPort, Connect = true });
            Assert.IsNull(connect.Error, "The first connection should not fail");
            connect = client.RequestConnectToPort(new ConnectionRequest() { ComPort = _comPort, Connect = true });
            Assert.NotNull(connect.Error, "The second connection should fail");
            TestContext.WriteLine($"When you try to reopen the COM port, it should give {connect.Error}");
        }


        [TearDown]
        public void Cleanup()
        {
            // shutdown the COM connection
            client.RequestConnectToPort(new ConnectionRequest() {Connect = false});
            channel.ShutdownAsync().Wait();
        }
    }
}