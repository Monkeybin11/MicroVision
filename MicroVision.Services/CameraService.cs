using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MicroVision.Core.Events;
using MicroVision.Core.Exceptions;
using MicroVision.Services.Annotations;
using MicroVision.Services.GrpcReference;
using Prism.Events;
using Prism.Mvvm;
using Services;

namespace MicroVision.Services
{
    public class CameraTrigger : ITrigger
    {
        private AsyncDuplexStreamingCall<CameraAcquisitionRequest, BufferedFramesResponse> _stream = null;
        private readonly CameraService _srv;
        private CameraAcquisitionRequest _buf = new CameraAcquisitionRequest();
        private object _lock = new object();
        private async void HandleResponse()
        {
            try
            {
                while (await _stream.ResponseStream.MoveNext(System.Threading.CancellationToken.None))
                {
                    var current = _stream.ResponseStream.Current;
                    if (current.Error != null)
                    {
                        OnError?.Invoke(this, new OnErrorArgs() { Message = current.Error.Message });
                    }

                    if (current.Error != null)
                    {
                        OnError?.Invoke(this,
                            new OnErrorArgs() { Message = "Failed to acquire image" });
                    }
                    else
                    {
                        _srv.Image = current.Images[0].ToByteArray();
                    }
                    
                }
            }
            catch (Exception e)
            {
                OnError?.Invoke(this, new OnErrorArgs() { Message = e.Message });
            }
        }
        public CameraTrigger(AsyncDuplexStreamingCall<CameraAcquisitionRequest, BufferedFramesResponse> stream, CameraService srv)
        {
            _stream = stream;
            _srv = srv;
            Task.Run(()=>HandleResponse());
        }

        public void InvokeTrigger()
        {
            lock (_lock)
            {
                _stream.RequestStream.WriteAsync(_buf);
            }
        }

        public void DestroyTrigger()
        {
            lock (_lock)
            {
                _stream.RequestStream.CompleteAsync().Wait();
            }
        }

        public event CameraControllerTrigger.ErrorEvent OnError;
    }
    public interface ICameraService : INotifyPropertyChanged
    {
        List<string> CameraUpdateList();
        void VimbaInstanceControl(ConnectionCommands command);
        void Connect(string cameraId);
        void Disconnect();
        double GetTemperature();
        CameraTrigger StreamAcquisition();
        byte[] Image { get; set; }
        void ConfigureCamera(CameraParametersRequest param);
    }

    public class CameraService : ICameraService, INotifyPropertyChanged
    {
        private byte[] _image;
        public byte[] Image
        {
            get => _image;
            set { _image = value; OnPropertyChanged();}
        }

        private readonly IEventAggregator _eventAggregator;
        private readonly IRpcService _rpcService;
        private readonly ILogService _log;
        
        private const string CameraConnectionErrorPrompt = "Failed to connect to camera rpc server";

        public CameraService(IEventAggregator eventAggregator, IRpcService rpcService, ILogService log)
        {
            _eventAggregator = eventAggregator;
            _rpcService = rpcService;
            _log = log;


            _eventAggregator.GetEvent<ShutDownEvent>().Subscribe(RestoreCameraRpcStatus);
        }

        private void RestoreCameraRpcStatus()
        {
            VimbaInstanceControl(ConnectionCommands.Disconnect);
        }

        /// <summary>
        /// Rpc invocation wrapper
        /// </summary>
        /// <typeparam name="TReturnType"></typeparam>
        /// <param name="func"></param>
        /// <param name="runtimeExceptionPrompt"></param>
        /// <param name="connectionExceptionPrompt"></param>
        /// <param name="rethrow"></param>
        /// <param name="checkError"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        private TReturnType TryInvoke<TReturnType>([NotNull] Func<TReturnType> func,
            string runtimeExceptionPrompt, string connectionExceptionPrompt = CameraConnectionErrorPrompt,
            bool rethrow = false, bool checkError = true, Action<string> onError = null)
        {
            var ret = default(TReturnType);
            try
            {
                ret = func.Invoke();
            }
            catch (Exception e)
            {
                _log.Logger.Error(e);
                _eventAggregator.GetEvent<ExceptionEvent>()
                    .Publish(new CameraRpcServerConnectionException(connectionExceptionPrompt));
                if (rethrow) throw;
                return ret;
            }

            bool hasError = false;
            Error error = null;

            if (checkError)
            {
                try
                {
                    // what will happen if the object does not contain error at all?
                    error = (Error)typeof(TReturnType).GetProperty("Error")?.GetValue(ret);
                    if (error != null) hasError = true;
                }
                catch (Exception e)
                {
                    error = new Error()
                    {
                        Level = Error.Types.Level.Error,
                        Message = e.Message,
                        Timestamp = Timestamp.FromDateTime(DateTime.Now)
                    };
                    hasError = true;
                }
            }

            if (ret == null || hasError)
            {
                _eventAggregator.GetEvent<ExceptionEvent>()
                    .Publish(new CameraRuntimeException($"{runtimeExceptionPrompt} {error?.Message}"));
            }

            return ret;
        }
        #region Service methods

        public void ConfigureCamera(CameraParametersRequest param)
        {
            var ret = TryInvoke(() => _rpcService.CameraClient.RequestCameraParameters(param), "Failed to update camera list");
        }
        public List<string> CameraUpdateList()
        {
            var ret = TryInvoke(() => _rpcService.CameraClient.RequestCameraList(new CameraListRequest()), "Failed to update camera list");

            return ret?.CameraList.ToList();
        }

        public void VimbaInstanceControl(ConnectionCommands command)
        {
            var ret = TryInvoke(() =>
                _rpcService.CameraClient.VimbaInstanceControl(new VimbaInstanceControlRequest() {Command = command}), "Failed to control the Vimba instance");
        }

        public void Connect(string cameraId)
        {
            var runtimeExceptionPrompt = $"Failed to connect {cameraId}";
            var ret = TryInvoke(() =>
                    _rpcService.CameraClient.RequestCameraConnection(
                        new CameraConnectionRequest() {Command = ConnectionCommands.Connect, CameraID = cameraId}),
                runtimeExceptionPrompt);

            if (ret.IsConnected)
            {
                _eventAggregator.GetEvent<VimbaConnectedEvent>().Publish();
                _eventAggregator.GetEvent<NotifyOperationEvent>().Publish($"Connected to camera: {cameraId}");
            }
        }

        public void Disconnect()
        {
            var runtimeExceptionPrompt = $"Failed to disconnect the camera";
            var ret = TryInvoke(() =>
                    _rpcService.CameraClient.RequestCameraConnection(
                        new CameraConnectionRequest() { Command = ConnectionCommands.Disconnect }),
                runtimeExceptionPrompt);

            if (!ret.IsConnected)
            {
                _eventAggregator.GetEvent<VimbaDisconnectedEvent>().Publish();
                _eventAggregator.GetEvent<NotifyOperationEvent>().Publish("Camera disconnected");
            }
        }

        public double GetTemperature()
        {
            var runtimeExceptionPrompt = $"Failed to disconnect the camera";
            var ret = TryInvoke(() =>
                    _rpcService.CameraClient.RequestTemperature(
                        new TemperatureRequest()),
                runtimeExceptionPrompt);

            return ret.Temperature;
        }

        public CameraTrigger StreamAcquisition()
        {
            return new  CameraTrigger(_rpcService.CameraClient.RequestFrameStream(), this);
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
