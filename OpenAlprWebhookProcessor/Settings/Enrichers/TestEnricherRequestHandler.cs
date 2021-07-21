using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.LicensePlates.Enricher;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Settings.Enrichers
{
    public class TestEnricherRequestHandler
    {
        private readonly ILicensePlateEnricherClient _licensePlateEnricherClient;

        private readonly ProcessorContext _processorContext;
        public TestEnricherRequestHandler(
            ProcessorContext processorContext,
            ILicensePlateEnricherClient licensePlateEnricherClient)
        {
            _processorContext = processorContext;
            _licensePlateEnricherClient = licensePlateEnricherClient;
        }

        public async Task<bool> HandleAsync(CancellationToken cancellationToken)
        {
            return await _licensePlateEnricherClient.TestAsync(cancellationToken);
        }
    }
}
