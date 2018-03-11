using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace SerialServiceNet
{
    [TestFixture]
    class ResponseDispatcherTest
    {
        [Test]
        [TestCase("r", "10323")]
        [TestCase("r", "1 1 1")]
        public async Task TestAsyncVersion(string prefix, string message)
        {
            var feedLine = prefix + " " + message;

            var responseDispatcher = new ResponseDispatcher();
            var item = responseDispatcher.RegisterAwaiter(prefix);
            var taskAwaiter = responseDispatcher.WaitForResultAsync(item).GetAwaiter();
            taskAwaiter.OnCompleted(() =>
            {
                Assert.AreEqual(taskAwaiter.GetResult(), message);
            });
            await Task.Delay(100);
            responseDispatcher.FeedMessage(feedLine);
        }

        [Test]
        [TestCase("r", "12312")]
        [Repeat(10)]
        public void TestSyncVersion(string prefix, string message)
        {
            var feedLine = prefix + " " + message;
            var responseDispatcher = new ResponseDispatcher();
            string waitForResult = null;
            var taskRegister = Task.Run(() => { waitForResult = responseDispatcher.WaitForResult(prefix); });
            var taskDispatcher = Task.Run(() =>
            {
                Task.Delay(100).Wait();
                responseDispatcher.FeedMessage(feedLine);
            });
            Task.WaitAll(taskRegister);
            Assert.AreEqual(message, waitForResult);
        }
        [Test]
        public void TestFeedAfterTimeout()
        {
            var responseDispatcher = new ResponseDispatcher();

            Assert.Throws<AggregateException>(() => responseDispatcher.WaitForResult("r"));
        }

        [Test]
        [Category("Integration")]
        [Repeat(10)]
        public async Task  TestManyWaiters()
        {
            var responseDispatcher = new ResponseDispatcher();
            var tasks = new List<Task<string>>();
            var item_r = responseDispatcher.RegisterAwaiter("r");
            var item_e = responseDispatcher.RegisterAwaiter("e");
            var item_S = responseDispatcher.RegisterAwaiter("S");
            tasks.Add(responseDispatcher.WaitForResultAsync(item_r, -1));
            tasks.Add(responseDispatcher.WaitForResultAsync(item_e, -1));
            tasks.Add(responseDispatcher.WaitForResultAsync(item_S, -1));
            await Task.Delay(10);
            responseDispatcher.FeedMessage("S SParam");
            responseDispatcher.FeedMessage("e eParam");
            responseDispatcher.FeedMessage("r rParam");

            
            Assert.AreEqual("rParam", tasks[0].Result);
            Assert.AreEqual("eParam", tasks[1].Result);
            Assert.AreEqual("SParam", tasks[2].Result);
        }

        [Test]
        [Category("Integration")]
        [TestCase("r", "13531", "240f")]
        [TestCase("r", "very", "llllooooonnnnggg")]
        [TestCase("r", "lllllloooonnnggg", "very")]
        [Repeat(5)]
        public void TestRepeatedPrefix(string prefix, string firstBody, string secondBody)
        {
            //TODO: add a slightly misaligned test
            var responseDispatcher = new ResponseDispatcher();
            var tasks = new List<Task>
            {
                Task.Run(() =>
                {
                    // simulate a task calling for the first time
                    var firstResult = responseDispatcher.WaitForResult("r", -1);
                    Assert.AreEqual(firstBody, firstResult);
                }),
                Task.Run(() =>
                {
                    // another task calling successively
                    Task.Delay(1).Wait();
                    var secondResult = responseDispatcher.WaitForResult("r",-1);
                    Assert.AreEqual(secondBody, secondResult);
                })
            };
            Task.Delay(100).Wait();
            responseDispatcher.FeedMessage(prefix + " " + firstBody);
            Task.Delay(100).Wait();
            responseDispatcher.FeedMessage(prefix + " " + secondBody);
            Task.WaitAll(tasks.ToArray());
        }
    }
}
