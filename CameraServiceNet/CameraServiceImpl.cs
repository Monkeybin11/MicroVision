using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services;
using AVT.VmbAPINET;
namespace CameraServiceNet
{
    public class CameraServiceImpl : Services.VimbaCamera.VimbaCameraBase
    {
        private Vimba _vimbaInstance = new Vimba();
        private Camera _cameraInstance = null;
        public CameraServiceImpl()

        {
        }

        /// <summary>
        /// Control the start up and shut down behavior of the vimba instance
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<VimbaInstanceControlResponse> VimbaInstanceControl(VimbaInstanceControlRequest request, Grpc.Core.ServerCallContext context)
        {
            var ret = new VimbaInstanceControlResponse();
            switch (request.Command)
            {
                case ConnectionCommands.Connect:
                    try
                    {
                        _vimbaInstance.Startup();
                        ret.IsStarted = true;
                    }
                    catch (Exception e)
                    {
                        ret.Error = ServiceHelper.BuildError(e, Error.Types.Level.Error);
                        return Task.FromResult(ret);
                    }
                    break;
                case ConnectionCommands.Disconnect:
                    try
                    {
                        ret.IsStarted = false;
                        _vimbaInstance.Shutdown();
                    }
                    catch (Exception e)
                    {
                        ret.Error = ServiceHelper.BuildError(e, Error.Types.Level.Error);
                    }
                    break;
            }

            return Task.FromResult(ret);
        }

        public override Task<BufferedFramesResponse> RequestBufferedFrames(BufferedFramesRequest request, Grpc.Core.ServerCallContext context)
        {
            return base.RequestBufferedFrames(request, context);
        }

        public override Task<CameraAcquisitionResponse> RequestCameraAcquisition(CameraAcquisitionRequest request, Grpc.Core.ServerCallContext context)
        {
            return base.RequestCameraAcquisition(request, context);
        }

        public override Task<CameraConnectionResponse> RequestCameraConnection(CameraConnectionRequest request, Grpc.Core.ServerCallContext context)
        {
            var ret = new CameraConnectionResponse();
            switch (request.Command)
            {
                case ConnectionCommands.Connect:
                    try
                    {
                        _cameraInstance =
                                        _vimbaInstance.OpenCameraByID(request.CameraID, VmbAccessModeType.VmbAccessModeFull);
                    }
                    catch (Exception e)
                    {
                        ret.Error = ServiceHelper.BuildError(e, Error.Types.Level.Error);
                    }
                    break;
                case ConnectionCommands.Disconnect:
                    try
                    {
                        _cameraInstance.Close();
                    }
                    catch (Exception e)
                    {
                        ret.Error = ServiceHelper.BuildError(e, Error.Types.Level.Error);
                    }
                    break;
            }
            return Task.FromResult(ret);
        }

        public override Task<CameraListResponse> RequestCameraList(CameraListRequest request, Grpc.Core.ServerCallContext context)
        {
            var ret = new CameraListResponse();
            try
            {
                var camList = new List<string>();
                foreach (Camera camera in _vimbaInstance.Cameras)
                {
                    camList.Add(camera.Name);
                }
                ret.CameraList.AddRange(camList);
            }
            catch (Exception e)
            {
                ret.Error = ServiceHelper.BuildError(e, Error.Types.Level.Error);
            }

            return Task.FromResult(ret);
        }

        public override Task<CameraParametersResponse> RequestCameraParameters(CameraParametersRequest request, Grpc.Core.ServerCallContext context)
        {
            return base.RequestCameraParameters(request, context);
        }

        public override Task RequestFrameStream(Grpc.Core.IAsyncStreamReader<CameraAcquisitionRequest> requestStream, Grpc.Core.IServerStreamWriter<BufferedFramesResponse> responseStream, Grpc.Core.ServerCallContext context)
        {
            return base.RequestFrameStream(requestStream, responseStream, context);
        }
    }
}
