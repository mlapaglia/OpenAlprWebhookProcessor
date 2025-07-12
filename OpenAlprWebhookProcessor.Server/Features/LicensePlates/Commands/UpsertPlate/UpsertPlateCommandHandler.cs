using MediatR;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Commands.UpsertPlate
{
    public class UpsertPlateCommandHandler : IRequestHandler<UpsertPlateCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpsertPlateCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpsertPlateCommand request, CancellationToken cancellationToken)
        {
            var existingPlate = await _unitOfWork.PlateGroups.GetByIdAsync(request.Id, cancellationToken);
            
            if (existingPlate != null)
            {
                // Update existing plate
                existingPlate.BestNumber = request.PlateNumber;
                existingPlate.VehicleColor = request.VehicleColor;
                existingPlate.VehicleMakeModel = $"{request.VehicleMake} {request.VehicleModel}".Trim();
                existingPlate.VehicleType = request.VehicleType;
                existingPlate.VehicleRegion = request.VehicleRegion;
                existingPlate.Latitude = request.Latitude;
                existingPlate.Longitude = request.Longitude;
                existingPlate.IsEnriched = request.IsEnriched;
                existingPlate.EnrichedData = request.EnrichedData;
                existingPlate.ProcessingTimeMs = request.ReceivedOnEpoch;
                
                _unitOfWork.PlateGroups.Update(existingPlate);
            }
            else
            {
                // Create new plate
                var newPlate = new PlateGroup
                {
                    Id = request.Id,
                    BestNumber = request.PlateNumber,
                    VehicleColor = request.VehicleColor,
                    VehicleMakeModel = $"{request.VehicleMake} {request.VehicleModel}".Trim(),
                    VehicleType = request.VehicleType,
                    VehicleRegion = request.VehicleRegion,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    IsEnriched = request.IsEnriched,
                    EnrichedData = request.EnrichedData,
                    ProcessingTimeMs = request.ReceivedOnEpoch,
                    ReceivedOnEpoch = request.ReceivedOnEpoch
                };

                await _unitOfWork.PlateGroups.AddAsync(newPlate, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
} 