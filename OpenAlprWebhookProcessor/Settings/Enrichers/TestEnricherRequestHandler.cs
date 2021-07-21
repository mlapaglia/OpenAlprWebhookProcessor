using OpenAlprWebhookProcessor.LicensePlates.Enricher;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Settings.Enrichers
{
    public class TestEnricherRequestHandler
    {
        private readonly ILicensePlateEnricherClient _licensePlateEnricherClient;

        public TestEnricherRequestHandler(ILicensePlateEnricherClient licensePlateEnricherClient)
        {
            _licensePlateEnricherClient = licensePlateEnricherClient;
        }

        public async Task<bool> HandleAsync(CancellationToken cancellationToken)
        {
            return await _licensePlateEnricherClient.TestAsync(cancellationToken);
        }
    }
}
