using Mediator;
using OpenAlprWebhookProcessor.Features.WebhookForwards.Queries.GetWebhookForwards;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Features.WebhookForwards.Commands.UpsertWebhookForwards
{
    public class UpsertWebhookForwardsCommand : ICommand
    {
        public List<WebhookForwardDto> WebhookForwards { get; set; }

        public UpsertWebhookForwardsCommand(List<WebhookForwardDto> webhookForwards)
        {
            WebhookForwards = webhookForwards;
        }
    }
} 