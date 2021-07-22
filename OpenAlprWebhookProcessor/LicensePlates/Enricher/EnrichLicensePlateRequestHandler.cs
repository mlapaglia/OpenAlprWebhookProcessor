using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.LicensePlates.Enricher
{
    public class EnrichLicensePlateRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        private readonly ILicensePlateEnricherClient _licensePlateEnricherClient;
        public EnrichLicensePlateRequestHandler(
            ILicensePlateEnricherClient licensePlateEnricherClient,
            ProcessorContext processorContext)
        {
            _licensePlateEnricherClient = licensePlateEnricherClient;
            _processorContext = processorContext;
        }

        public async Task HandleAsync(Guid plateId)
        {
            var plateGroup = await _processorContext.PlateGroups
                .Where(x => x.Id == plateId)
                .FirstOrDefaultAsync();

            if (plateGroup == null)
            {
                throw new ArgumentException("Plate Id not found.");
            }

            if (!plateGroup.VehicleRegion.StartsWith("us-"))
            {
                throw new ArgumentException("Plate must be United States region.");
            }

            if (plateGroup.IsEnriched)
            {
                throw new ArgumentException("Plate has already been enriched.");
            }

            var enrichResult = await _licensePlateEnricherClient.GetLicenseInformationAsync(
                plateGroup.BestNumber,
                plateGroup.VehicleRegion.Replace("us-", "").ToUpper(),
                default);

            plateGroup.VehicleType = enrichResult.Style;
            plateGroup.VehicleMake = enrichResult.Make;
            plateGroup.VehicleMakeModel = enrichResult.Make + " " + enrichResult.Model;
            plateGroup.VehicleYear = enrichResult.Year;
            plateGroup.IsEnriched = true;

            await _processorContext.SaveChangesAsync();
        }
    }
}
