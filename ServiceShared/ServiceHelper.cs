using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core.Logging;

namespace Services
{
    public class ServiceHelper
    {
        private static ConsoleLogger _logger = new ConsoleLogger();

        public ServiceHelper()
        { 
        }
        /// <summary>
        /// internal log function
        /// </summary>
        /// <typeparam name="T">Exception or string</typeparam>
        /// <param name="msg"></param>
        /// <param name="level"></param>
        public static void _log<T>(T msg, Error.Types.Level level)
        {
            if (typeof(T) == typeof(Exception))
            {
                var e = msg as Exception;
                switch (level)
                {
                    case Error.Types.Level.Error:
                    case Error.Types.Level.Fatal:
                        _logger.Error(e, e?.Message);
                        break;
                    case Error.Types.Level.Info:
                        _logger.Info(e?.Message);
                        break;
                    case Error.Types.Level.Warning:
                        _logger.Warning(e, e?.Message);
                        break;
                }
            }
            else
            {
                var message = msg as string;
                switch (level)
                {
                    case Error.Types.Level.Error:
                    case Error.Types.Level.Fatal:
                        _logger.Error(message);
                        break;
                    case Error.Types.Level.Info:
                        _logger.Info(message);
                        break;
                    case Error.Types.Level.Warning:
                        _logger.Warning(message);
                        break;
                }
            }
        }

        /// <summary>
        /// Internal use for build the error structure.
        /// </summary>
        /// <param name="e"> Exception thrown</param>
        /// <param name="level"> error level</param>
        /// <returns>Error object</returns>
        public static Error BuildError(Exception e, Error.Types.Level level)
        {
            _log(e, level);
            return new Error()
            {
                Level = level,
                Message = e.Message,
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
            };
        }

        public static Error BuildError(string message, Error.Types.Level level)
        {
            _log(message, level);
            return new Error() { Level = level, Message = message, Timestamp = Timestamp.FromDateTime(DateTime.UtcNow) };
        }
    }
}
