using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Settings.Enrichers
{
    public class UpsertEnricherRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        public UpsertEnricherRequestHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task HandleAsync(Enricher enricher)
        {
            var dbEnricher = await _processorContext.Enrichers.Where(x => x.Id == enricher.Id).FirstOrDefaultAsync();

            if (dbEnricher == null)
            {
                dbEnricher = new Data.Enricher()
                {
                    ApiKey = enricher.ApiKey,
                    EnricherType = enricher.EnricherType,
                    IsEnabled = enricher.IsEnabled,
                    RunAlways = enricher.RunAlways,
                    RunAtNight = enricher.RunAtNight,
                    RunManually = enricher.RunManually,
                };

                _processorContext.Add(dbEnricher);
            }
            else
            {
                dbEnricher.ApiKey = enricher.ApiKey;
                dbEnricher.EnricherType = enricher.EnricherType;
                dbEnricher.IsEnabled = enricher.IsEnabled;
                dbEnricher.RunAlways = enricher.RunAlways;
                dbEnricher.RunAtNight = enricher.RunAtNight;
                dbEnricher.RunManually = enricher.RunManually;
            }

            await _processorContext.SaveChangesAsync();
        }
    }
}
