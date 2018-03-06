using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using MicroVision.Core.Events;
using Prism.Events;
using static Services.CameraController;
using static Services.VimbaCamera;

namespace MicroVision.Services.GrpcReference
{
    public interface IRpcService
    {
        VimbaCameraClient CameraClient { get; }
        CameraControllerClient CameraControllerClient { get; }
    }

    public class RpcService : IRpcService
    {
        private readonly IEventAggregator _eventAggregator;
        public VimbaCameraClient CameraClient { get; private set; }
        public CameraControllerClient CameraControllerClient { get; private set; }

        private Channel _cameraChannel;
        private Channel _cameraControllerChannel;

        public RpcService(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            Initialization();
        }

        private void Initialization()
        {
            var appSettings = ConfigurationManager.AppSettings;
            if (appSettings["DryRun"] == "false")
            {
                _cameraChannel = new Channel(appSettings["CameraRpcServer"], ChannelCredentials.Insecure);
                CameraClient = new VimbaCameraClient(_cameraChannel);

                // try to initiate the connection to test if the server is alive
                var timeout = DateTime.UtcNow.AddSeconds(5);
                new TaskFactory().StartNew(async () =>
                {
                    try
                    {
                        await _cameraChannel.ConnectAsync(timeout);
                    }
                    catch (Exception)
                    {
                        _eventAggregator.GetEvent<HardwareRpcConnedtionFailedEvent>()
                            .Publish("Camera Service Server Not Available");
                    }
                });
                

                _cameraControllerChannel = new Channel(appSettings["CameraControllerRpcServer"],
                    ChannelCredentials.Insecure);
                CameraControllerClient = new CameraControllerClient(_cameraControllerChannel);
                new TaskFactory().StartNew(async () =>
                {
                    try
                    {
                        await _cameraControllerChannel.ConnectAsync(timeout);
                    }
                    catch (Exception)
                    {
                        _eventAggregator.GetEvent<HardwareRpcConnedtionFailedEvent>()
                            .Publish("Camera Controller Server Not Available");
                    }
                });
                
            }
        }
    }
}