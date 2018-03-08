using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using MicroVision.Core.Events;
using MicroVision.Core.Exceptions;
using MicroVision.Services.Annotations;
using MicroVision.Services.GrpcReference;
using Prism.Events;
using Services;

namespace MicroVision.Services
{
    public interface ICameraService
    {
        List<string> CameraUpdateList();
        void VimbaInstanceControl(ConnectionCommands command);
    }

    public class CameraService : ICameraService
    {
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

        #endregion
    }
}
