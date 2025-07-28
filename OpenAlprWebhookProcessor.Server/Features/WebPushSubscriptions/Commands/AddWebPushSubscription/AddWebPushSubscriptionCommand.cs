using Lib.Net.Http.WebPush;
using Mediator;

namespace OpenAlprWebhookProcessor.Features.WebPushSubscriptions.Commands.AddWebPushSubscription
{
    public class AddWebPushSubscriptionCommand : ICommand
    {
        public PushSubscription Subscription { get; set; }

        public AddWebPushSubscriptionCommand(PushSubscription subscription)
        {
            Subscription = subscription;
        }
    }
} 