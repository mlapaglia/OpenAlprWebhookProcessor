using Mediator;

namespace OpenAlprWebhookProcessor.Features.WebPushSubscriptions.Commands.DeleteWebPushSubscription
{
    public class DeleteWebPushSubscriptionCommand : ICommand
    {
        public string Endpoint { get; set; }

        public DeleteWebPushSubscriptionCommand(string endpoint)
        {
            Endpoint = endpoint;
        }
    }
} 