using Mediator;

namespace OpenAlprWebhookProcessor.Features.Alerts.Commands.UpsertPushover
{
    public class UpsertPushoverCommand : ICommand
    {
        public PushoverRequest Request { get; set; }

        public UpsertPushoverCommand(PushoverRequest request)
        {
            Request = request;
        }
    }
} 