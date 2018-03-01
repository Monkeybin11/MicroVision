using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using Services;
using AVT.VmbAPINET;
using Grpc.Core;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Google.Protobuf;
using Google.Protobuf.Collections;

namespace CameraServiceNet
{
    public class FrameData
    {
        public FrameData(Frame frame)
        {
            FrameID = frame.FrameID;
            TimeStamp = frame.Timestamp;
            Width = frame.Width;
            Height = frame.Height;
            Buffer = (byte[]) frame.Buffer.Clone();
        }

        public ulong FrameID { get; }
        public ulong TimeStamp { get; }
        public uint Width { get; }
        public uint Height { get; }
        public byte[] Buffer { get; }
    }

    internal class GrayscalePalette
    {
        public static ColorPalette GetGrayScalePalette()
        {
            using (var bmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
            {
                var cp = bmp.Palette;
                var entries = cp.Entries;
                for (int i = 0; i < entries.Length; i++)
                {
                    entries[i] = Color.FromArgb(i, i, i);
                }
                return cp;
            }
        }
    }
    public class CameraServiceImpl : Services.VimbaCamera.VimbaCameraBase
    {
        private Vimba _vimbaInstance = new Vimba();
        private Camera _cameraInstance = null;
        private int _numFrames = 1;
        private List<FrameData> _frameBuffer = new List<FrameData>();
        public CameraServiceImpl()
        {
        }

        ~CameraServiceImpl()
        {
            try
            {
                _cameraInstance?.Close();
                _vimbaInstance.Shutdown();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        /// configure the common parameters that are not required to be configured for each time.
        /// </summary>
        private void ConfigureCommonParameters()
        {
            if (_cameraInstance == null)
            {
                throw new ApplicationException($"camera is not at correct state");
            }

            try
            {
                _cameraInstance.StopContinuousImageAcquisition();
            }
            catch (Exception)
            {
                // ignored
            }

            var features = _cameraInstance.Features;
            features["LineSelector"].StringValue = "Line1";
            features["LineMode"].StringValue = "Output";
            features["LineInverter"].BoolValue = true;
            features["LineSource"].StringValue = "FrameActive";
            features["AcquisitionMode"].StringValue = "MultiFrame";
            features["AcquisitionFrameRateMode"].StringValue = "Basic";
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
            var ret = new BufferedFramesResponse();
            try
            {
                foreach (var frameData in _frameBuffer)
                {
                    var image = new Bitmap((int)frameData.Width, (int)frameData.Height, PixelFormat.Format8bppIndexed);
                    image.Palette = GrayscalePalette.GetGrayScalePalette();
                    var wholeBitmap = new Rectangle(0, 0, image.Width, image.Height);
                    var bitmapData = image.LockBits(wholeBitmap, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                    Marshal.Copy(frameData.Buffer,0 , bitmapData.Scan0, frameData.Buffer.Length);
                    image.UnlockBits(bitmapData);

                    var output = new MemoryStream();
                    image.Save(output, ImageFormat.Png);
                    output.Seek(0, 0);
                    var bs = ByteString.FromStream(output);

                    ret.Images.Add(bs);
                }
            }
            catch (Exception e)
            {
                ret.Error = ServiceHelper.BuildError(e, Error.Types.Level.Error);
            }

            return Task.FromResult(ret);
        }

        public override Task<CameraAcquisitionResponse> RequestCameraAcquisition(CameraAcquisitionRequest request, Grpc.Core.ServerCallContext context)
        {
            var ret = new CameraAcquisitionResponse();
            try
            {
                _frameBuffer.Clear();
                _cameraInstance.StartContinuousImageAcquisition(_numFrames);
            }
            catch (Exception e)
            {
                ret.Error = ServiceHelper.BuildError(e, Error.Types.Level.Error);
            }

            return Task.FromResult(ret);
        }

        /// <summary>
        /// open or close the camera
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
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
                        _cameraInstance.OnFrameReceived += FrameReceivedHandler;
                        ConfigureCommonParameters();
                        ret.IsConnected = true;
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
                        ret.IsConnected = false;
                    }
                    catch (Exception e)
                    {
                        ret.Error = ServiceHelper.BuildError(e, Error.Types.Level.Error);
                    }
                    break;
            }
            return Task.FromResult(ret);
        }

        /// <summary>
        /// get the camera ID list
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<CameraListResponse> RequestCameraList(CameraListRequest request, Grpc.Core.ServerCallContext context)
        {
            var ret = new CameraListResponse();
            try
            {
                var camList = new List<string>();
                foreach (Camera camera in _vimbaInstance.Cameras)
                {
                    camList.Add(camera.Id);
                }
                ret.CameraList.AddRange(camList);
            }
            catch (Exception e)
            {
                ret.Error = ServiceHelper.BuildError(e, Error.Types.Level.Error);
            }

            return Task.FromResult(ret);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<CameraParametersResponse> RequestCameraParameters(CameraParametersRequest request, Grpc.Core.ServerCallContext context)
        {
            var ret = new CameraParametersResponse();
            if (request.Write)
            {
                try
                {
                    // configure camera parameters
                    var features = _cameraInstance.Features;
                    features["ExposureTime"].FloatValue = request.Params.ExposureTime;
                    features["AcquisitionFrameRate"].FloatValue = request.Params.FrameRate;
                    features["Gain"].FloatValue = request.Params.Gain;
                    features["AcquisitionFrameCount"].IntValue = _numFrames = request.Params.NumFrames;
                }
                catch (Exception e)
                {
                    ret.Error = ServiceHelper.BuildError(e, Error.Types.Level.Error);
                    return Task.FromResult(ret);
                }
            }
            else
            {
                // read parameters
                try
                {
                    var features = _cameraInstance.Features;
                    ret.Params = new CameraParameters()
                    {
                        ExposureTime = features["ExposureTime"].FloatValue,
                        FrameRate = features["AcquisitionFrameRate"].FloatValue,
                        Gain = features["Gain"].FloatValue,
                        NumFrames = _numFrames
                    };
                }
                catch (Exception e)
                {
                    ret.Error = ServiceHelper.BuildError(e, Error.Types.Level.Error);
                    return Task.FromResult(ret);
                }
            }
            return Task.FromResult(ret);
        }

        private void FrameReceivedHandler(Frame frame)
        {
            _frameBuffer.Add(new FrameData(frame));
        }

        public override Task RequestFrameStream(Grpc.Core.IAsyncStreamReader<CameraAcquisitionRequest> requestStream, Grpc.Core.IServerStreamWriter<BufferedFramesResponse> responseStream, Grpc.Core.ServerCallContext context)
        {
            return base.RequestFrameStream(requestStream, responseStream, context);
        }

        public override Task<TemperatureResponse> RequestTemperature(TemperatureRequest request, ServerCallContext context)
        {
            var ret = new TemperatureResponse();
            try
            {
                ret.Temperature = _cameraInstance.Features["DeviceTemperature"].FloatValue;
            }
            catch (Exception e)
            {
                ret.Error = ServiceHelper.BuildError(e, Error.Types.Level.Error);
            }

            return Task.FromResult(ret);
        }
    }
}
