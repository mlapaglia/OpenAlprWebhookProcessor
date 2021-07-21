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

        public async Task HandleAsync(
            Guid plateId,
            CancellationToken cancellationToken)
        {
            var plateToEnrich = await _processorContext.PlateGroups.FirstOrDefaultAsync(x => x.Id == plateId);

            var result = await _licensePlateEnricherClient.GetLicenseInformationAsync(
                plateToEnrich.BestNumber,
                plateToEnrich.VehicleRegion,
                cancellationToken);

            plateToEnrich.VehicleMake = result.Make;
            plateToEnrich.VehicleMakeModel = result.Make + " " + result.Model;
            plateToEnrich.VehicleType = result.Style;
            plateToEnrich.VehicleYear = result.Year;
            //plateToEnrich.VehicleColor = ??

            await _processorContext.SaveChangesAsync(cancellationToken);
        }
    }
}
