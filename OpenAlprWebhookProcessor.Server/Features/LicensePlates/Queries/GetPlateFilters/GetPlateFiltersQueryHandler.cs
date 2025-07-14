using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetPlateFilters;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetPlateFilters
{
    public class GetPlateFiltersQueryHandler : IRequestHandler<GetPlateFiltersQuery, GetLicensePlateFiltersResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetPlateFiltersQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<GetLicensePlateFiltersResponse> Handle(GetPlateFiltersQuery request, CancellationToken cancellationToken)
        {
            var plateGroups = _unitOfWork.PlateGroups.GetQueryable();

            var vehicleColors = await plateGroups
                .Where(x => !string.IsNullOrEmpty(x.VehicleColor))
                .Select(x => x.VehicleColor)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync(cancellationToken);

            var vehicleMakes = await plateGroups
                .Where(x => !string.IsNullOrEmpty(x.VehicleMakeModel))
                .Select(x => x.VehicleMakeModel)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync(cancellationToken);

            var vehicleTypes = await plateGroups
                .Where(x => !string.IsNullOrEmpty(x.VehicleType))
                .Select(x => x.VehicleType)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync(cancellationToken);

            var vehicleRegions = await plateGroups
                .Where(x => !string.IsNullOrEmpty(x.VehicleRegion))
                .Select(x => x.VehicleRegion)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync(cancellationToken);

            return new GetLicensePlateFiltersResponse
            {
                VehicleColors = vehicleColors,
                VehicleMakes = vehicleMakes,
                VehicleTypes = vehicleTypes,
                VehicleRegions = vehicleRegions
            };
        }
    }
} 