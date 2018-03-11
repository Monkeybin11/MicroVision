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
        private class QueuedResponseHandler
        {
            public string Identifier = null;
            public ManualResetEvent Signal = new ManualResetEvent(false);
            public string MessageBody = null;
        }

        private List<QueuedResponseHandler> _pool = new List<QueuedResponseHandler>();
        private Dictionary<string, object> _lock = new Dictionary<string, object>();


        private object _poolLock = new object();

        /// <summary>
        /// Called by serial receive dispatcher and resolve the waiting items
        /// </summary>
        /// <param name="message"></param>
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
                QueuedResponseHandler foundItem = null;
                try
                {
                    foundItem = _pool.Where(handler => handler.Identifier == prefix).First();
                }
                catch (Exception e){}

                if (foundItem != null)
                {
                    // resolve the target
                    foundItem.MessageBody = msgbody;
                    foundItem.Signal.Set();
                }
            }
        }

        public Task<string> WaitForResultAsync(string responsePrefix, int timeout = 1000)
        {
            return Task.Run(() =>
            {
                var item = new QueuedResponseHandler()
                {
                    Identifier = responsePrefix,
                    Signal = new ManualResetEvent(false)
                };
                lock (_poolLock)
                {   
                    _pool.Add(item);
                }

                var triggered = item.Signal.WaitOne(timeout);

                lock (_poolLock)
                {
                    _pool.Remove(item);
                }

                if (!triggered) throw new TimeoutException($"Wait for result of command {responsePrefix} time out");
                return Task.FromResult(item.MessageBody);
                
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