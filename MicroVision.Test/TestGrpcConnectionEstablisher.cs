using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;

namespace MicroVision.Test
{
    [TestFixture]
    public class TestGrpcConnectionEstablisher
    {
        private Channel _channel;
        public const string Uri = "localhost:39945";

        [SetUp]
        public void Setup()
        {
            _channel = new Channel(Uri, ChannelCredentials.Insecure);
        }

        [Test]
        [Description("Without this utility the rpc channel should stay idle")]
        public void TestWithoutEstablisher()
        {
            Assert.AreNotEqual(ChannelState.Ready,_channel.State);
            var establisher = new Services.Models.GrpcConnectionEstablisher(_channel);
            Assert.AreEqual(establisher.Connected, false);
        }

        [Test]
        [Description("When it automatically refresh the channel it should be connected")]
        public void TestWithEstablisher()
        {
            var establisher = new Services.Models.GrpcConnectionEstablisher(_channel);
            establisher.StartKeepAlive();
            Task.Delay(1000).Wait();
            Assert.IsTrue(establisher.Connected);
            for (int i = 0; i < 8; i++)
            {
                Task.Delay(TimeSpan.FromMinutes(1)).Wait();
                Assert.IsTrue(establisher.Connected);
            }
            establisher.StopKeepAlive();
            establisher.Dispose();
        }

        [Test]
        [Description("When the channel was shutdown, the state should change")]
        public void TestShutdown()
        {
            var establisher = new Services.Models.GrpcConnectionEstablisher(_channel);
            establisher.StartKeepAlive();

            Task.Delay(1000).Wait();

            Assert.IsTrue(establisher.Connected);

            _channel.ShutdownAsync().Wait();

            Assert.IsFalse(establisher.Connected);

            establisher.StopKeepAlive();
            establisher.Dispose();
        }

    }
}
