using MediatR;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Features.Settings.Queries.GetWebhookForwards
{
    public class GetWebhookForwardsQuery : IRequest<List<WebhookForwardDto>>
    {
    }
} 