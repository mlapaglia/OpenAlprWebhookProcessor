using MediatR;

namespace OpenAlprWebhookProcessor.Features.WebPushSubscriptions.Commands.DeleteWebPushSubscription
{
    public class DeleteWebPushSubscriptionCommand : IRequest
    {
        public string Endpoint { get; set; }

        public DeleteWebPushSubscriptionCommand(string endpoint)
        {
            Endpoint = endpoint;
        }
    }
} 