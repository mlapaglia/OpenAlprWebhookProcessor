using Mediator;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.WebPushSubscriptions.Commands.AddWebPushSubscription
{
    public class AddWebPushSubscriptionCommandHandler : ICommandHandler<AddWebPushSubscriptionCommand>
    {
        private readonly IWebPushSubscriptionsService _pushSubscriptionsService;

        public AddWebPushSubscriptionCommandHandler(IWebPushSubscriptionsService pushSubscriptionsService)
        {
            _pushSubscriptionsService = pushSubscriptionsService;
        }

        public async ValueTask<Unit> Handle(AddWebPushSubscriptionCommand request, CancellationToken cancellationToken)
        {
            await _pushSubscriptionsService.InsertAsync(request.Subscription, cancellationToken);
            return Unit.Value;
        }
    }
} 