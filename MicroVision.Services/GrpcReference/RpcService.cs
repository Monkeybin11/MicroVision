using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using static Services.CameraController;
using static Services.VimbaCamera;

namespace MicroVision.Services.GrpcReference
{
    public interface IRpcService
    { 
        VimbaCameraClient CameraClient { get; }
        CameraControllerClient CameraControllerClient { get; }
    }
    public class RpcService:IRpcService
    {
        public VimbaCameraClient CameraClient { get; private set; }
        public CameraControllerClient CameraControllerClient { get; private set; }

        public RpcService()
        {
            Initialization();
        }

        private void Initialization()
        {
            var appSettings =  ConfigurationManager.AppSettings;
            if (appSettings["DryRun"] == "false")
            {
                CameraClient = new VimbaCameraClient(new Channel(appSettings["CameraRpcServer"], ChannelCredentials.Insecure));
                CameraControllerClient = new CameraControllerClient(new Channel(appSettings["CameraControllerRpcServer"], ChannelCredentials.Insecure));
            }
        }
    }
}
