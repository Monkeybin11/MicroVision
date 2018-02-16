using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SerialServiceNet
{
    public class ResponseDispatcher
    {
        private class SignalDictItem
        {
            public ManualResetEvent Signal = new ManualResetEvent(false);
            public string MessageBody = null;
        }

        private Dictionary<string, SignalDictItem> _pool = new Dictionary<string, SignalDictItem>();
        private Dictionary<string, object> _lock = new Dictionary<string, object>();

        private Dictionary<string, SignalDictItem> Pool
        {
            get
            {
                lock (_poolLock)
                {
                    return _pool;
                }
            }
        }

        private object _poolLock = new object();
        private object _lockProt = new object();

        // some function that accept the stream in, and get the prefix, and return the corresponding waiting thread
        public void FeedMessage(string message)
        {
            //separate the prefix

            //locate the awaiting task

            //release the task with message

            //if no message is registered, abadon this message and throw a warning

            var tokens = message.Split(' ');
            var prefix = tokens[0];
            var msgbody = String.Join(" ", tokens.Skip(1)).Trim();

            lock (_poolLock)
            {
                if (Pool.ContainsKey(prefix))
                {
                    Pool[prefix].MessageBody = msgbody;
                    Pool[prefix].Signal.Set();
                }
            }
        }

        public Task<string> WaitForResultAsync(string responsePrefix, int timeout = 1000)
        {
            return Task.Run(() =>
            {
                lock (_lockProt)
                {
                    if (!_lock.ContainsKey(responsePrefix))
                    {
                        _lock[responsePrefix] = new object();
                    }
                }

                lock (_lock[responsePrefix])
                {
                    var signalDictItem = new SignalDictItem();
                    lock (_poolLock)
                    {
                        signalDictItem.Signal.Reset();
                        Pool[responsePrefix] = signalDictItem;
                    }

                    var triggered = signalDictItem.Signal.WaitOne(timeout);
                    lock (_poolLock)
                    {
                        Pool.Remove(responsePrefix);
                    }

                    if (!triggered) throw new TimeoutException("Wait for result time out");
                    return signalDictItem.MessageBody;
                }
            });
            
        }

        public string WaitForResult(string responsePrefix, int timeout = 1000)
        {
            var task = WaitForResultAsync(responsePrefix, timeout);
            task.Wait();
            return task.Result;
        }
    }
}