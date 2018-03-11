using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Services;

namespace SerialServiceTest
{
    [TestFixture()]
    public class ContinuousReadTest : SerialTestBase
    {
        [Test]
        [TestCase(500)]
        public void TestContinuousRead(int interval)
        {
            client.RequestConnectToPort(new ConnectionRequest() {ComPort = _comPort, Connect = true});

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            var powerTask = Task.Run(async () =>
            {
                for (int i = 0; i < 15; i++)
                {
                    var ret = client.RequestPowerStatus(new PowerStatusRequest());
                    Assert.IsNull(ret.Error);
                    TestContext.WriteLine($"PowerStatus: {ret.PowerCode}");
                    await Task.Delay(interval);
                }
            });

            var currentTask = Task.Run(async () =>
            {
                for (int i = 0; i < 30; i++)
                {
                    var ret = client.RequestCurrentStatus(new CurrentStatusRequest());
                    Assert.IsNull(ret.Error);
                    TestContext.WriteLine($"Current: {ret.Current}");
                    await Task.Delay(interval/2);
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            Task.WaitAll(currentTask, powerTask);
            client.RequestConnectToPort(new ConnectionRequest() { Connect = false });
        }
    }
}
