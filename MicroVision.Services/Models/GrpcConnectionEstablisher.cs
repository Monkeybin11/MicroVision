using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Grpc.Core;

namespace MicroVision.Services.Models
{
    public class GrpcConnectionEstablisher 
    {
        private readonly Channel _channel;
        private Timer _timer;
        private TimeSpan _interval;

        public GrpcConnectionEstablisher(Channel channel) : this(channel, TimeSpan.FromMinutes(2))
        {
        }

        public GrpcConnectionEstablisher(Channel channel, TimeSpan interval)
        {
            _channel = channel;
            _interval = interval;
            _timer = new Timer(interval.TotalMilliseconds);
            _timer.Elapsed += (sender, args) => Refresh();
        }

        private async Task Refresh()
        {
            try
            {
                await _channel.ConnectAsync();
                await Task.Delay(1000);
                if (Connected)
                {
                    await _channel.WaitForStateChangedAsync(ChannelState.Ready);
                    if (_channel.State == ChannelState.TransientFailure)
                    {
                        await _channel.ConnectAsync();
                    }
                }
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        public void StartKeepAlive()
        {
            Refresh();
            _timer.Start();
        }

        public void StopKeepAlive()
        {
            _timer.Stop();
        }

        public void Dispose()
        {
            _timer.Stop();
            _timer.Dispose();
        }

        public bool Connected
        {
            get => _channel?.State == ChannelState.Ready;
        }
    }
}
