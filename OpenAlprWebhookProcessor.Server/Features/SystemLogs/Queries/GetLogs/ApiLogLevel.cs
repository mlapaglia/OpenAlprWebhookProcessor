using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace OpenAlprWebhookProcessor.Features.SystemLogs.Queries.GetLogs
{
    public enum ApiLogLevel
    {
        Verbose = 0,
        Debug = 1, 
        Information = 2,
        Warning = 3,
        Error = 4,
        Critical = 5,
    }

    public static class ApiLogLevelExtensions
    {
        public static LogLevel ToSystemLogLevel(this ApiLogLevel apiLevel)
        {
            return apiLevel switch
            {
                ApiLogLevel.Verbose => LogLevel.Trace,
                ApiLogLevel.Debug => LogLevel.Debug,
                ApiLogLevel.Information => LogLevel.Information,
                ApiLogLevel.Warning => LogLevel.Warning,
                ApiLogLevel.Error => LogLevel.Error,
                ApiLogLevel.Critical => LogLevel.Critical,
                _ => LogLevel.Information
            };
        }

        public static ApiLogLevel ToApiLogLevel(this LogEventLevel serilogLevel)
        {
            return serilogLevel switch
            {
                LogEventLevel.Verbose => ApiLogLevel.Verbose,
                LogEventLevel.Debug => ApiLogLevel.Debug,
                LogEventLevel.Information => ApiLogLevel.Information,
                LogEventLevel.Warning => ApiLogLevel.Warning,
                LogEventLevel.Error => ApiLogLevel.Error,
                LogEventLevel.Fatal => ApiLogLevel.Critical,
                _ => ApiLogLevel.Information
            };
        }
    }
}
