using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Media.Imaging;
using MicroVision.Core.Events;
using MicroVision.Core.Exceptions;
using MicroVision.Services.GrpcReference;
using MicroVision.Services.Models;
using Prism.Events;
using Services;
using Timer = System.Timers.Timer;

namespace MicroVision.Services
{
    public class CaptureService : ICaptureService
    {
        public CaptureService(ISerialService serialService, ICameraService cameraService, IEventAggregator eventAggregator, IParameterServices parameterService)
        {
            _serialService = serialService;
            _cameraService = cameraService;
            _eventAggregator = eventAggregator;
            _parameterService = parameterService;

            _eventAggregator.GetEvent<ShutDownEvent>().Subscribe(Dispose);

            _triggerTimer = new Timer();
            _triggerTimer.Elapsed += TriggerTimerOnElapsed;

            ConfigureParameterChangeHandler();

        }

        private readonly ISerialService _serialService;
        private readonly ICameraService _cameraService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IParameterServices _parameterService;

        private bool _capturing;
        private Timer _triggerTimer = null;
        private CameraControllerTrigger _streamCameraControllerTrigger;
        private CameraTrigger _streamImage;

        private int _remains = 0;

        private object _configurationLock = new object();

        private void Dispose()
        {
            Stop();
            _triggerTimer.Dispose();
        }

        public void Capture(int interval, int count)
        {
            _capturing = true;
            _streamCameraControllerTrigger = _serialService.StreamTrigger();
            _streamImage = _cameraService.StreamAcquisition();

            _streamImage.OnError += CameraTriggerError;
            _streamCameraControllerTrigger.OnError += CameraControllerTriggerOnError;
            
            _triggerTimer.Interval = interval;
            _remains = count;
            _triggerTimer.Start();
            _cameraService.ConfigureCamera(new CameraParametersRequest() { Params = new CameraParameters() { NumFrames = 1, ExposureTime = 45, FrameRate = 390, Gain = 0 }, Write = true });
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

        private void ConfigureParameterChangeHandler()
        {
            _parameterService.Gain.PropertyChanged += CameraPropertyChangedHandler;
            _parameterService.ExposureTime.PropertyChanged += CameraPropertyChangedHandler;

            _parameterService.LaserDuration.PropertyChanged += LaserPropertyChangedHandler;
        }



        #region Parameter changed monitor
        private void CameraPropertyChangedHandler(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            lock (_configurationLock)
            {
                _cameraService.ConfigureCamera(new CameraParametersRequest()
                {
                    Params = new CameraParameters()
                    {
                        NumFrames = 1,
                        ExposureTime = _parameterService.ExposureTime.Value,
                        Gain = _parameterService.Gain.Value,
                        FrameRate = 390
                    },
                    Write = true
                });
            }
        }

        private void LaserPropertyChangedHandler(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            lock (_configurationLock)
            {
                _streamCameraControllerTrigger.SetLaserDuration(_parameterService.LaserDuration.Value);
            }
        }

        #endregion

        private void CameraControllerTriggerOnError(object sender, OnErrorArgs args)
        {
            _eventAggregator.GetEvent<ExceptionEvent>().Publish(new ComRuntimeException(args.Message));

        }

        private void CameraTriggerError(object sender, OnErrorArgs args)
        {
            _eventAggregator.GetEvent<ExceptionEvent>().Publish(new CameraRuntimeException(args.Message));
        }

        public bool Capturing => _capturing;

        public void Stop()
        {
            _capturing = false;
            _triggerTimer.Stop();
            try
            {
                _streamCameraControllerTrigger?.DestroyTrigger();
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                _streamImage?.DestroyTrigger();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
