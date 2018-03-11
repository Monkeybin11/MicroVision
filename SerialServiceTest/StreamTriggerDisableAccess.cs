using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services;

namespace SerialServiceTest
{
    [TestFixture]
    public class StreamTriggerDisableAccess : SerialTestBase
    {
        [Test]
        public void TestDisabledAccessDuringStream()
        {
            var ret = client.RequestConnectToPort(new ConnectionRequest() {Connect = true, ComPort = _comPort});
            Assert.IsNull(ret.Error);

            var retPower = client.RequestPowerStatus(new PowerStatusRequest() {PowerCode = 15, Write = true});
            Assert.IsNull(retPower.Error);

            Task.Delay(5000).Wait();
            var stream = client.StreamRequestArmTrigger();

            var requestCurrent = client.RequestCurrentStatus(new CurrentStatusRequest());
            Assert.IsNull(requestCurrent.Error);

            stream.RequestStream.WriteAsync(new ArmTriggerRequest()
            {
                LaserConfiguration = new LaserStatusRequest()
                {
                    DurationUs = 100,
                    Intensity = 255
                },
                MaxTriggerTimeUs = 100000,
                ArmTrigger = true
            });
            requestCurrent = client.RequestCurrentStatus(new CurrentStatusRequest());
            Assert.IsTrue(requestCurrent.Error.Level == Error.Types.Level.Warning);

            stream.RequestStream.CompleteAsync().Wait();

        }
    }
}