using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using MicroVision.Services.Models;
using Services;

namespace MicroVision.Services
{
    public class CameraControllerTrigger : ITrigger
    {
        public delegate void ErrorEvent(object sender, OnErrorArgs args);

        private object _triggerLock = new object();
        private AsyncDuplexStreamingCall<ArmTriggerRequest, ArmTriggerResponse> _stream;

        private ArmTriggerRequest _requestBuffer = new ArmTriggerRequest()
        {
            ArmTrigger = true,
            MaxTriggerTimeUs = 100000,
            LaserConfiguration = new LaserStatusRequest() {DurationUs = 30, Intensity = 255}
        };

        public void InvokeTrigger()
        {
            lock (_triggerLock)
            {
                _stream.RequestStream.WriteAsync(_requestBuffer);
            }
        }

        public void DestroyTrigger()
        {
            lock (_triggerLock)
            {
                _stream.RequestStream.CompleteAsync().Wait();
            }
        }

        public CameraControllerTrigger(AsyncDuplexStreamingCall<ArmTriggerRequest, ArmTriggerResponse> stream)
        {
            _stream = stream;
            Task.Run(() => HandleResponse());
        }

        private async void HandleResponse()
        {
            try
            {
                while (await _stream.ResponseStream.MoveNext(CancellationToken.None))
                {
                    var current = _stream.ResponseStream.Current;
                    if (current.Error != null)
                    {
                        OnError?.Invoke(this, new OnErrorArgs() {Message = current.Error.Message});
                    }

                    if (current.TriggerAutoDisarmed)
                    {
                        OnError?.Invoke(this,
                            new OnErrorArgs() {Message = "Laser not reset. Check the wiring to the camera"});
                    }
                }
            }
            catch (Exception e)
            {
                OnError?.Invoke(this, new OnErrorArgs() {Message = e.Message});
            }
        }

        public event ErrorEvent OnError;
    }
}