using Mediator;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetWebhookForwards;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Features.Settings.Commands.UpsertWebhookForwards
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