using Microsoft.AspNetCore.SignalR;
using Serilog;
using Serilog.Configuration;

namespace OpenAlprWebhookProcessor.SystemLogs
{
    public static class SignalrSinkExtension
    {
        public static LoggerConfiguration Signalr(
            this LoggerSinkConfiguration loggerConfiguration,
            IHubContext<ProcessorHub.ProcessorHub, ProcessorHub.IProcessorHub> processorHub)
        {
            return loggerConfiguration.Sink(new SignalrSink(processorHub));
        }
    }
}
