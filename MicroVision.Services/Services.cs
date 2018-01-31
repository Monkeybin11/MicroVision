using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace MicroVision.Services
{
    public interface IServices
    {
        ILog Logger { get; }
    }

    public class Services : IServices
    {

        public Services(string moduleName)
        {
            Logger = LogManager.GetLogger(moduleName ?? "DefaultLogger");
        }

        public ILog Logger { get; }
    }
}
