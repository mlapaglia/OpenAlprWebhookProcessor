using Mediator;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetIgnores;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Settings.Queries.GetWebhookForwards
{
    public class GetWebhookForwardsQueryHandler : IQueryHandler<GetWebhookForwardsQuery, List<WebhookForwardDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetWebhookForwardsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<List<WebhookForwardDto>> Handle(GetWebhookForwardsQuery query, CancellationToken cancellationToken)
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