using MediatR;
using OpenAlprWebhookProcessor.Features.LicensePlates.Commands.EnrichPlate;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Settings.Commands.TestEnrichers
{
    public class TestEnrichersCommandHandler : IRequestHandler<TestEnrichersCommand, bool>
    {
        private readonly ILicensePlateEnricherClient _licensePlateEnricherClient;

        public TestEnrichersCommandHandler(ILicensePlateEnricherClient licensePlateEnricherClient)
        {
            _licensePlateEnricherClient = licensePlateEnricherClient;
        }

        public async Task<bool> Handle(TestEnrichersCommand request, CancellationToken cancellationToken = default)
        {
            return await _licensePlateEnricherClient.TestAsync(cancellationToken);
        }
    }
} 