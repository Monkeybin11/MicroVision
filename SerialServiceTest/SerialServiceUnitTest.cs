using System;
using System.IO;
using System.Runtime.Remoting.Channels;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Services;

namespace SerialServiceTest
{
    [TestFixture]
    public class SerialServiceBasicUnitTest : SerialTestBase
    {

        [Category("Basic")]
        [Test]
        public void TestGetInfo()
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
            TestContext.WriteLine(
                $"Info package: \n \tFirmware = {info.FirmwareVersion} \n \tHardware = {info.HardwareVersion} \n \tService = {info.ServiceVersion}");
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
            var disconnect = client.RequestConnectToPort(new ConnectionRequest() {Connect = false});
            Assert.IsNull(disconnect.Error);
        }

        [Category("Nauty")]
        [Description("Should it fail when the connection argument is invalid")]
        [Test]
        public void TestConnectWithoutName()
        {
            var connect = client.RequestConnectToPort(new ConnectionRequest() {Connect = true});
            Assert.NotNull(connect.Error);
            TestContext.WriteLine($"When no COM name is given, you will get error {connect.Error}");
        }

        [Category("Nauty")]
        [Description("Should it fail when the connection is already opened")]
        [Test]
        public void TestRepeatedOpen()
        {
            var connect = client.RequestConnectToPort(new ConnectionRequest() {ComPort = _comPort, Connect = true});
            Assert.IsNull(connect.Error, "The first connection should not fail");
            connect = client.RequestConnectToPort(new ConnectionRequest() {ComPort = _comPort, Connect = true});
            Assert.NotNull(connect.Error, "The second connection should fail");
            TestContext.WriteLine($"When you try to reopen the COM port, it should give {connect.Error}");
        }

        [Category("Basic")]
        [Description("Read power code, write, and read again")]
        [Test]
        public async Task TestPowerConfiguration()
        {
            const int delay = 2000;

            int powerCode = 9;

            var connect = client.RequestConnectToPort(new ConnectionRequest() {ComPort = _comPort, Connect = true});
            Assert.IsNull(connect.Error, "The connection should not fail");

            var powerStatusResponse = client.RequestPowerStatus(new PowerStatusRequest() {Write = false});
            Assert.IsNull(powerStatusResponse.Error, "Error when read the power status");
            _writer.WriteLine($"Initial power code is {powerStatusResponse.PowerCode}");

            await Task.Delay(delay);
            powerStatusResponse =
                client.RequestPowerStatus(new PowerStatusRequest() {Write = true, PowerCode = powerCode});
            Assert.IsNull(powerStatusResponse.Error, "Error when read the power status");
            Assert.AreEqual(powerStatusResponse.PowerCode, powerCode);
            _writer.WriteLine($"After write, power code is {powerStatusResponse.PowerCode}");

            await Task.Delay(delay);
            powerStatusResponse = client.RequestPowerStatus(new PowerStatusRequest() {Write = false});
            Assert.IsNull(powerStatusResponse.Error, "Error when read the power status");
            Assert.AreEqual(powerStatusResponse.PowerCode, powerCode);
            _writer.WriteLine($"Read again, power code is {powerStatusResponse.PowerCode}");

            await Task.Delay(delay);
            powerStatusResponse = client.RequestPowerStatus(new PowerStatusRequest() {Write = true, PowerCode = 0});
            Assert.IsNull(powerStatusResponse.Error, "Error when read the power status");
            Assert.AreEqual(powerStatusResponse.PowerCode, 0);
            _writer.WriteLine($"Finally, reset the power code to 0, power code is {powerStatusResponse.PowerCode}");
        }

        [Category("Basic")]
        [Test]
        public void TestReadCurrent()
        {
            client.RequestConnectToPort(new ConnectionRequest() {Connect = true, ComPort = _comPort});
            var currentStatusResponse = client.RequestCurrentStatus(new CurrentStatusRequest());
            Assert.IsNull(currentStatusResponse.Error, "Error occured in the current read process");
            _writer.WriteLine($"The current reading is {currentStatusResponse.Current}");
        }

        private void FocusExecutionCommonCode(int slowdown)
        {
            var openPowerResult = client.RequestPowerStatus(new PowerStatusRequest() {Write = true, PowerCode = 5});
            Assert.IsNull(openPowerResult.Error, "Failed to switch on power");
            var response = client.RequestFocusStatus(new FocusStatusRequest()
            {
                AutoPower = true,
                Steps = 3000,
                DriverPower = true,
                SlowdownFactor = slowdown
            });
            Assert.IsNull(response.Error);
            _writer.WriteLine($"After raising command, the slow down factor is {response.SlowdownFactor}");
            response = client.RequestFocusStatus(new FocusStatusRequest()
            {
                AutoPower = true,
                Steps = -3000,
                DriverPower = true,
                SlowdownFactor = 0
            });
            Assert.IsNull(response.Error);
            _writer.WriteLine($"After dropping command, the slow down factor is {response.SlowdownFactor}");
            client.RequestPowerStatus(new PowerStatusRequest() {Write = true, PowerCode = 0});
        }

        [Category("Integration")]
        [Test]
        //[TestCase(0)]
        //[TestCase(100)]
        [TestCase(1000)]
        //[TestCase(5000)]
        public void TestFocusExecution(int slowdown)
        {
            var openPortResult =
                client.RequestConnectToPort(new ConnectionRequest() {Connect = true, ComPort = _comPort});
            Assert.IsNull(openPortResult.Error, "Port Open Failed");

            FocusExecutionCommonCode(slowdown);
        }

        [Test]
        [Category("Integration")]
        [TestCase(500)]
        public void TestReadDuringExecution(int delay)
        {
            var openPortResult =
                client.RequestConnectToPort(new ConnectionRequest() {Connect = true, ComPort = _comPort});
            Assert.IsNull(openPortResult.Error, "Port Open Failed");

            var cancel = new CancellationTokenSource();
            var currentReadingTask = Task.Factory.StartNew(() =>
            {
                while (!cancel.IsCancellationRequested)
                {
                    var currentStatusRequest = new CurrentStatusRequest();
                    var currentStatusResponse = client.RequestCurrentStatus(currentStatusRequest);

                    Assert.IsNull(currentStatusResponse.Error, "Port Open Failed");
                    _writer.WriteLine($"Current: {currentStatusResponse.Current}");
                    Task.Delay(delay).Wait();
                }
            }, cancel.Token);

            FocusExecutionCommonCode(5000);
            cancel.Cancel();
            currentReadingTask.Wait(1000);
        }

        [Test]
        [Category("Integration")]
        public void TestArmTriggerAndTimeOut()
        {
            var openPortResult =
                client.RequestConnectToPort(new ConnectionRequest() {Connect = true, ComPort = _comPort});
            Assert.IsNull(openPortResult.Error, "Port Open Failed");
            client.RequestPowerStatus(new PowerStatusRequest() {Write = true, PowerCode = 3});
            Task.Delay(5000).Wait();
            var armTriggerResult = client.RequestArmTrigger(new ArmTriggerRequest()
            {
                ArmTrigger = true,
                LaserConfiguration = new LaserStatusRequest() {DurationUs = 1000, Intensity = 255},
                MaxTriggerTimeUs = 100000
            });
            Assert.IsNull(armTriggerResult.Error, "Arm trigger generated an error");
            Assert.AreEqual(true, armTriggerResult.TriggerAutoDisarmed);
        }

        [Test]
        public void TestStreamControl()
        {
            var openPortResult =
                client.RequestConnectToPort(new ConnectionRequest() {Connect = true, ComPort = _comPort});
            Assert.IsNull(openPortResult.Error, "Port Open Failed");
            client.RequestPowerStatus(new PowerStatusRequest() {Write = true, PowerCode = 3});
            Task.Delay(5000).Wait();
            var stream = client.StreamRequestArmTrigger(new CallOptions());

            var cancel = new CancellationTokenSource();
            // receive
            Task.Run(async () =>
            {
                while (await stream.ResponseStream.MoveNext(cancel.Token))
                {
                    _writer.WriteLine($"Got new message! {stream.ResponseStream.Current}");
                    
                }
            });

            var requestData = new ArmTriggerRequest()
            {
                ArmTrigger = true,
                MaxTriggerTimeUs = 10000,
                LaserConfiguration = new LaserStatusRequest() {Intensity = 255, DurationUs = 45}
            };

            for (int i = 0; i < 3; i++)
            {
                stream.RequestStream.WriteAsync(requestData);
                Task.Delay(500).Wait();
            }

            cancel.Cancel();
        }

        [Test]
        public void TestSoftwareReset()
        {
            var openPortResult =
                client.RequestConnectToPort(new ConnectionRequest() {Connect = true, ComPort = _comPort});
            Assert.IsNull(openPortResult.Error, "Port Open Failed");

            var result = client.RequestSoftwareReset(new Empty());
            Assert.IsNull(result.Error);
        }
    }
}