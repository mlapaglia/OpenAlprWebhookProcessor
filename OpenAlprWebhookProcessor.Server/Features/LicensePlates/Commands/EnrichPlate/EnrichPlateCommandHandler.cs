using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Commands.EnrichPlate
{
    public class EnrichPlateCommandHandler : IRequestHandler<EnrichPlateCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILicensePlateEnricherClient _licensePlateEnricherClient;

        public EnrichPlateCommandHandler(
            IUnitOfWork unitOfWork,
            ILicensePlateEnricherClient licensePlateEnricherClient)
        {
            _unitOfWork = unitOfWork;
            _licensePlateEnricherClient = licensePlateEnricherClient;
        }

        public async Task Handle(EnrichPlateCommand request, CancellationToken cancellationToken)
        {
            var plateGroup = await _unitOfWork.PlateGroups.GetByIdWithDetailsAsync(
                request.PlateId,
                cancellationToken);

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

            if (enrichResult != null)
            {
                plateGroup.IsEnriched = true;
                plateGroup.VehicleType = enrichResult.Style;
                plateGroup.VehicleMake = enrichResult.Make;
                
                _unitOfWork.PlateGroups.Update(plateGroup);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else
            {
                throw new InvalidOperationException("Failed to enrich plate data.");
            }
        }
    }
} 