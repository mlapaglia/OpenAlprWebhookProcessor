using MediatR;
using OpenAlprWebhookProcessor.WebPushSubscriptions;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.WebPushSubscriptions.Commands.DeleteWebPushSubscription
{
    public class DeleteWebPushSubscriptionCommandHandler : IRequestHandler<DeleteWebPushSubscriptionCommand>
    {
        private readonly IWebPushSubscriptionsService _pushSubscriptionsService;

        public DeleteWebPushSubscriptionCommandHandler(IWebPushSubscriptionsService pushSubscriptionsService)
        {
            _pushSubscriptionsService = pushSubscriptionsService;
        }

        public async Task Handle(DeleteWebPushSubscriptionCommand request, CancellationToken cancellationToken)
        {
            _pushSubscriptionsService.Delete(request.Endpoint);
        }
    }
} 