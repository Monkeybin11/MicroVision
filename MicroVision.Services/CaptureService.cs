using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Media.Imaging;
using MicroVision.Services.GrpcReference;
using Timer = System.Timers.Timer;

namespace MicroVision.Services
{
    public interface ICaptureService
    {
        void Capture(int interval, int count);
        
        BitmapImage CurrentBitmapImage { get; set; }
        bool Capturing { get; }

        void Stop();
    }

    public class CaptureService : ICaptureService
    {
        private readonly ISerialService _serialService;
        private readonly ICameraService _cameraService;

        public CaptureService(ISerialService serialService, ICameraService cameraService)
        {
            _serialService = serialService;
            _cameraService = cameraService;
            _triggerTimer = new Timer();
            _triggerTimer.Elapsed += TriggerTimerOnElapsed;
        }

        private void TriggerTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _streamCameraControllerTrigger.InvokeTrigger();
            _streamImage.InvokeTrigger();
            if (_remains > 0)
            {
                // counted capture
                _remains--;
            }

            if (_remains == 0)
            {
                _triggerTimer.Stop();
            }
        }

        private bool _capturing;
        private Timer _triggerTimer = null;
        private CameraControllerTrigger _streamCameraControllerTrigger;
        private CameraTrigger _streamImage;

        private int _remains = 0;

        public void Capture(int interval, int count)
        {
            _capturing = true;
            _streamCameraControllerTrigger = _serialService.StreamTrigger();
            _streamImage = _cameraService.StreamAcquisition();
            _triggerTimer.Interval = interval;
            _remains = count;
            _triggerTimer.Start();
        }

        public BitmapImage CurrentBitmapImage { get; set; }

        public bool Capturing => _capturing;

        public void Stop()
        {
            _capturing = false;
            _triggerTimer.Stop();
            _streamCameraControllerTrigger.DestroyTrigger();
        }
    }
}
