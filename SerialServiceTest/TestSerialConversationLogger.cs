using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SerialServiceNet;

namespace SerialServiceTest
{
    [TestFixture]
    public class TestSerialConversationLogger
    {
        [Test]
        public void TestTranscription()
        {
            var ms = new MemoryStream();
            var conversationLogger = new SerialConversationLogger(ms);

            conversationLogger.WrittenToSerial("r\n");
            conversationLogger.ReceivedBySerial("r 0\n");

            ms.Seek(0, 0);
            var sr = new StreamReader(ms);
            
            TestContext.Write(sr.ReadToEnd());
        }
    }
}
