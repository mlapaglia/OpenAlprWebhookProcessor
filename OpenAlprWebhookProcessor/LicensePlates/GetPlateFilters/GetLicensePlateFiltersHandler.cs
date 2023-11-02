using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.LicensePlates.GetPlateFilters
{
    public class GetLicensePlateFiltersHandler
    {
        private static readonly TextInfo _textInfo = CultureInfo.CurrentCulture.TextInfo;

        private readonly ProcessorContext _processerContext;

        public GetLicensePlateFiltersHandler(ProcessorContext processerContext)
        {
            _processerContext = processerContext;
        }

        public async Task<GetLicensePlateFiltersResponse> HandleAsync(CancellationToken cancellationToken)
        {
            _processerContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var response = new GetLicensePlateFiltersResponse
            {
                VehicleMakes = await _processerContext.PlateGroups
                    .Where(x => !string.IsNullOrWhiteSpace(x.VehicleMake))
                    .Select(x => x.VehicleMake)
                    .Distinct()
                    .ToListAsync(cancellationToken),
                VehicleModels = await _processerContext.PlateGroups
                    .Where(x => !string.IsNullOrWhiteSpace(x.VehicleMakeModel))
                    .Select(x => x.VehicleMakeModel)
                    .Distinct()
                    .ToListAsync(cancellationToken),
                VehicleColors = await _processerContext.PlateGroups
                    .Where(x => !string.IsNullOrWhiteSpace(x.VehicleColor))
                    .Select(x => x.VehicleColor)
                    .Distinct()
                    .ToListAsync(cancellationToken),
                VehicleTypes = await _processerContext.PlateGroups
                    .Where(x => !string.IsNullOrWhiteSpace(x.VehicleType))
                    .Select(x => x.VehicleType)
                    .Distinct()
                    .ToListAsync(cancellationToken),
                VehicleYears = await _processerContext.PlateGroups
                    .Where(x => !string.IsNullOrWhiteSpace(x.VehicleYear))
                    .Select(x => x.VehicleYear)
                    .Distinct()
                    .ToListAsync(cancellationToken),
                VehicleRegions = await _processerContext.PlateGroups
                    .Where(x => !string.IsNullOrWhiteSpace(x.VehicleRegion))
                    .Select(x => x.VehicleRegion)
                    .Distinct()
                    .ToListAsync(cancellationToken),
            };

            response.VehicleMakes = response.VehicleMakes
                .OrderBy(x => x)
                .Select(x => _textInfo?.ToTitleCase(x.Split('_')[0]))
                .ToList();

            response.VehicleModels = response.VehicleModels
                .OrderBy(x => x)
                .Select(x => _textInfo?.ToTitleCase(x.Split('_')[1]))
                .ToList();

            response.VehicleColors = response.VehicleColors
                .OrderBy(x => x)
                .Select(x => _textInfo?.ToTitleCase(x))
                .ToList();

            response.VehicleTypes = response.VehicleTypes
                .OrderBy(x => x)
                .Select(x => _textInfo?.ToTitleCase(x))
                .ToList();

            return response;
        }
    }
}
