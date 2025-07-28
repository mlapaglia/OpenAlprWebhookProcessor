using Mediator;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.WebPushSubscriptions.Commands.DeleteWebPushSubscription
{
    public class DeleteWebPushSubscriptionCommandHandler : ICommandHandler<DeleteWebPushSubscriptionCommand>
    {
        private readonly IWebPushSubscriptionsService _pushSubscriptionsService;

        public DeleteWebPushSubscriptionCommandHandler(IWebPushSubscriptionsService pushSubscriptionsService)
        {
            _pushSubscriptionsService = pushSubscriptionsService;
        }

        public async ValueTask<Unit> Handle(
            DeleteWebPushSubscriptionCommand request,
            CancellationToken cancellationToken = default)
        {
            await _pushSubscriptionsService.DeleteAsync(request.Endpoint, cancellationToken);
            return Unit.Value;
        }
    }
} 