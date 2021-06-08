using Microsoft.AspNetCore.SignalR;
using Serilog;
using Serilog.Configuration;
using System;

namespace OpenAlprWebhookProcessor.SystemLogs
{
    public static class SignalrSinkExtension
    {
        public static LoggerConfiguration Signalr(
                  this LoggerSinkConfiguration loggerConfiguration,
                  IHubContext<ProcessorHub.ProcessorHub, ProcessorHub.IProcessorHub> processorHub,
                  IFormatProvider formatProvider = null)
        {
            return loggerConfiguration.Sink(new SignalrSink(processorHub));
        }
    }
}
