using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.LicensePlates.Enricher
{
    public interface ILicensePlateEnricherClient
    {
        Task<EnrichedLicensePlate> GetLicenseInformationAsync(
            string plateNumber,
            string state,
            CancellationToken cancellationToken);

        Task<bool> TestAsync(CancellationToken cancellationToken);
    }
}
