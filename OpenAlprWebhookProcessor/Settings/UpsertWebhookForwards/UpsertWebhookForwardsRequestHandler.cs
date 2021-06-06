using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Settings.UpsertWebhookForwards
{
    public class UpsertWebhookForwardsRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        public UpsertWebhookForwardsRequestHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task HandleAsync(List<WebhookForward> webhookForwards)
        {
            webhookForwards = webhookForwards.Where(x => x.Destination != null).ToList();

            var dbForwards = await _processorContext.WebhookForwards.ToListAsync();

            var fowradsToRemove = dbForwards.Where(p => !webhookForwards.Any(p2 => p2.Id == p.Id));

            _processorContext.RemoveRange(fowradsToRemove);

            var forwardsToUpdate = dbForwards.Where(x => webhookForwards.Any(p2 => p2.Id == x.Id));

            foreach (var alertToUpdate in forwardsToUpdate)
            {
                var updatedAlert = webhookForwards.First(x => x.Id == alertToUpdate.Id);

                alertToUpdate.FowardingDestination = updatedAlert.Destination;
                alertToUpdate.IgnoreSslErrors = updatedAlert.IgnoreSslErrors;
            }

            var alertsToAdd = webhookForwards.Where(x => !dbForwards.Any(p2 => p2.Id == x.Id));

            foreach (var alertToAdd in alertsToAdd)
            {
                var addedForward = new Data.WebhookForward()
                {
                    FowardingDestination = alertToAdd.Destination,
                    IgnoreSslErrors = alertToAdd.IgnoreSslErrors,
                };

                _processorContext.WebhookForwards.Add(addedForward);
            }

            await _processorContext.SaveChangesAsync();
        }
    }
}
