using MediatR;
using OpenAlprWebhookProcessor.WebPushSubscriptions;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.WebPushSubscriptions.Commands.AddWebPushSubscription
{
    public class AddWebPushSubscriptionCommandHandler : IRequestHandler<AddWebPushSubscriptionCommand>
    {
        private readonly IWebPushSubscriptionsService _pushSubscriptionsService;

        public AddWebPushSubscriptionCommandHandler(IWebPushSubscriptionsService pushSubscriptionsService)
        {
            _pushSubscriptionsService = pushSubscriptionsService;
        }

        public async Task Handle(AddWebPushSubscriptionCommand request, CancellationToken cancellationToken)
        {
            _pushSubscriptionsService.Insert(request.Subscription);
        }
    }
} 