using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Common.API
{
    public class LogConfiguration
    {
        public static void InitLogger(IConfigurationRoot Configuration, string logPath)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.RollingFile(logPath)
                .CreateLogger();
        }

        public static void AddSerilogToLoggerFactory(ILoggerFactory loggerFactory) => loggerFactory.AddSerilog();
    }
}
