using MediatR;

namespace OpenAlprWebhookProcessor.Features.Alerts.Commands.UpsertPushover
{
    public class UpsertPushoverCommand : IRequest
    {
        public PushoverRequest Request { get; set; }

        public UpsertPushoverCommand(PushoverRequest request)
        {
            Request = request;
        }
    }
} 