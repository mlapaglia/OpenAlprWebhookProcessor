using Microsoft.AspNetCore.SignalR;
using OpenAlprWebhookProcessor.Server.ProcessorHub;
using Serilog;
using Serilog.Configuration;
using System;

namespace OpenAlprWebhookProcessor.Server.SystemLogs
{
    public static class SignalrSinkExtension
    {
        public static LoggerConfiguration Signalr(
            this LoggerSinkConfiguration loggerConfiguration,
            IHubContext<ProcessorHub.ProcessorHub, IProcessorHub> processorHub,
            IFormatProvider formatProvider = null)
        {
            return loggerConfiguration.Sink(new SignalrSink(processorHub));
        }
    }
}
