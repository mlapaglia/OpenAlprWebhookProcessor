using Microsoft.AspNetCore.SignalR;
using OpenAlprWebhookProcessor.ProcessorHub;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;
using System;
using System.IO;

namespace OpenAlprWebhookProcessor.SystemLogs
{
    public class SignalrSink : ILogEventSink
    {
        private readonly IHubContext<ProcessorHub.ProcessorHub, IProcessorHub> _processorHub;

        private readonly MessageTemplateTextFormatter _formatter;

        public SignalrSink(IHubContext<ProcessorHub.ProcessorHub, IProcessorHub> processorHub)
        {
            _processorHub = processorHub;
            _formatter = new MessageTemplateTextFormatter(
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}");
        }

        public void Emit(LogEvent logEvent)
        {
            using var writer = new StringWriter();
            _formatter.Format(logEvent, writer);
            var formattedLog = writer.ToString().TrimEnd(); // Remove trailing newline

            _processorHub.Clients.All.ProcessInformationLogged(formattedLog);
        }
    }
}
