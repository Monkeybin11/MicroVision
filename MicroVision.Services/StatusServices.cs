using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroVision.Services.Models;
using Prism.Mvvm;

namespace MicroVision.Services
{
    public interface IStatusService
    {
        VimbaConnectionStatus VimbaConnectionStatus { get; }
        ComConnectionStatus ComConnectionStatus { get; }
    }

    public class StatusServices : IStatusService
    {
        public VimbaConnectionStatus VimbaConnectionStatus { get; } = new VimbaConnectionStatus();
        public ComConnectionStatus ComConnectionStatus { get; } = new ComConnectionStatus();
    }
}