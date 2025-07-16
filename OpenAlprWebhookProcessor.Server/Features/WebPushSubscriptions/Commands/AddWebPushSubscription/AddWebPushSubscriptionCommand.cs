using Lib.Net.Http.WebPush;
using MediatR;

namespace OpenAlprWebhookProcessor.Features.WebPushSubscriptions.Commands.AddWebPushSubscription
{
    public class AddWebPushSubscriptionCommand : IRequest
    {
        public PushSubscription Subscription { get; set; }

        public AddWebPushSubscriptionCommand(PushSubscription subscription)
        {
            Subscription = subscription;
        }
    }
} 