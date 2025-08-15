using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Commands.EnrichPlate
{
    public class EnrichLicensePlateRequestHandler
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILicensePlateEnricherClient _licensePlateEnricherClient;
        
        public EnrichLicensePlateRequestHandler(
            ILicensePlateEnricherClient licensePlateEnricherClient,
            IUnitOfWork unitOfWork)
        {
            _licensePlateEnricherClient = licensePlateEnricherClient;
            _unitOfWork = unitOfWork;
        }

        public async Task HandleAsync(Guid plateId, CancellationToken cancellationToken = default)
        {
            var plateGroup = await _unitOfWork.PlateGroups.GetQueryable()
                .AsNoTracking()
                .Where(x => x.Id == plateId)
                .FirstOrDefaultAsync(cancellationToken);

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
                cancellationToken);

            plateGroup.VehicleType = enrichResult.Style;
            plateGroup.VehicleMake = enrichResult.Make;
            plateGroup.VehicleMakeModel = enrichResult.Make + " " + enrichResult.Model;
            plateGroup.VehicleYear = enrichResult.Year;
            plateGroup.IsEnriched = true;

            _unitOfWork.PlateGroups.Update(plateGroup);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
