using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace MicroVision.Services
{
    public interface ILogService
    {
        ILog Logger { get; }
        void ConfigureLogger(string loggerName);
    }
    public class LogService : ILogService
    {
        private ILog _logger;
        public ILog Logger { get => _logger; }
        
        public LogService()
        {
            _logger = LogManager.GetLogger("DefaultLogger");
        }
       
        public void ConfigureLogger(string loggerName)
        {
            _logger = LogManager.GetLogger(loggerName);
        }

    }
}
