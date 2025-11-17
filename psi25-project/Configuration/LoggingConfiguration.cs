using Serilog;
using Serilog.Core;

namespace psi25_project.Configuration
{
    public static class LoggingConfiguration
    {
        /// <summary>
        /// Creates and configures the Serilog logger for the application.
        /// Logs to both console and rolling file.
        /// </summary>
        public static Logger CreateLogger()
        {
            return new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(
                    path: "logs/geohunt-.log",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    retainedFileCountLimit: 30)
                .Enrich.FromLogContext()
                .CreateLogger();
        }
    }
}
