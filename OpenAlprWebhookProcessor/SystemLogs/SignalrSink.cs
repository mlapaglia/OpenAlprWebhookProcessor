using Microsoft.AspNetCore.SignalR;
using OpenAlprWebhookProcessor.ProcessorHub;
using Serilog.Core;
using Serilog.Events;
using System;

namespace OpenAlprWebhookProcessor.SystemLogs
{
    public class SignalrSink : ILogEventSink
    {
        private readonly IHubContext<ProcessorHub.ProcessorHub, IProcessorHub> _processorHub;

        public SignalrSink(IHubContext<ProcessorHub.ProcessorHub, IProcessorHub> processorHub)
        {
            _processorHub = processorHub;
        }

        public void Emit(LogEvent logEvent)
        {
            _processorHub.Clients.All.ProcessInformationLogged($"{DateTimeOffset.UtcNow} {logEvent.RenderMessage()}");
        }
    }
}
