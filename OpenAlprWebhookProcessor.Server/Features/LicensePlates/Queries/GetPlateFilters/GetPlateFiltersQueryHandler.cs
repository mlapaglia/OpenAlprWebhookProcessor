using Mediator;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetPlateFilters
{
    public class GetPlateFiltersQueryHandler : IQueryHandler<GetPlateFiltersQuery, GetLicensePlateFiltersResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetPlateFiltersQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<GetLicensePlateFiltersResponse> Handle(
            GetPlateFiltersQuery request,
            CancellationToken cancellationToken = default)
        {
            var plateGroups = _unitOfWork.PlateGroups.GetQueryable();

            var vehicleColors = await plateGroups
                .Where(x => !string.IsNullOrEmpty(x.VehicleColor))
                .Select(x => x.VehicleColor)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync(cancellationToken);

            var vehicleMakeModels = await plateGroups
                .Where(x => !string.IsNullOrEmpty(x.VehicleMakeModel))
                .Select(x => x.VehicleMakeModel)
                .Distinct()
                .ToListAsync(cancellationToken);

            var vehicleMakes = vehicleMakeModels
                .Select(x => CapitalizeName(x.Split('_')[0]))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            var vehicleModels = vehicleMakeModels
                .Where(x => x.Contains('_'))
                .Select(x => CapitalizeName(x.Split('_', 2)[1]))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            var vehicleMakeModelMap = vehicleMakeModels
                .Where(x => x.Contains('_'))
                .GroupBy(x => CapitalizeName(x.Split('_')[0]))
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => CapitalizeName(x.Split('_', 2)[1])).Distinct().OrderBy(x => x).ToList()
                );

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
                VehicleModels = vehicleModels,
                VehicleMakeModelMap = vehicleMakeModelMap,
                VehicleTypes = vehicleTypes,
                VehicleRegions = vehicleRegions
            };
        }

        private static string CapitalizeName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            return char.ToUpper(name[0]) + name.Substring(1).ToLower();
        }
    }
} 