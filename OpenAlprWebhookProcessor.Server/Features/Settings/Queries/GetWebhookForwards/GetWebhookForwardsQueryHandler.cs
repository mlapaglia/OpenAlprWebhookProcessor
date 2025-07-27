using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Settings.Queries.GetWebhookForwards
{
    public class GetWebhookForwardsQueryHandler : IRequestHandler<GetWebhookForwardsQuery, List<WebhookForwardDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetWebhookForwardsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<WebhookForwardDto>> Handle(GetWebhookForwardsQuery request, CancellationToken cancellationToken = default)
        {
            var webhookForwards = await _unitOfWork.WebhookForwards.GetAllAsync(cancellationToken);
            var forwards = new List<WebhookForwardDto>();

            foreach (var webhook in webhookForwards)
            {
                var forward = new WebhookForwardDto()
                {
                    Destination = webhook.FowardingDestination,
                    Id = webhook.Id,
                    IgnoreSslErrors = webhook.IgnoreSslErrors,
                    ForwardGroupPreviews = webhook.ForwardGroupPreviews,
                    ForwardSinglePlates = webhook.ForwardSinglePlates,
                    ForwardGroups = webhook.ForwardGroups,
                };

                forwards.Add(forward);
            }

            return forwards;
        }
    }
} 