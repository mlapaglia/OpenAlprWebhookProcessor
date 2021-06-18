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
            var response = new GetLicensePlateFiltersResponse
            {
                VehicleMakes = await _processerContext.PlateGroups
                    .Where(x => !string.IsNullOrWhiteSpace(x.VehicleMake))
                    .Select(x => x.VehicleMake)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToListAsync(cancellationToken),
                VehicleModels = await _processerContext.PlateGroups
                    .Where(x => !string.IsNullOrWhiteSpace(x.VehicleMake))
                    .Select(x => x.VehicleMakeModel)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToListAsync(cancellationToken),
                VehicleColors = await _processerContext.PlateGroups
                    .Where(x => !string.IsNullOrWhiteSpace(x.VehicleColor))
                    .Select(x => x.VehicleColor)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToListAsync(cancellationToken),
                VehicleTypes = await _processerContext.PlateGroups
                    .Where(x => !string.IsNullOrWhiteSpace(x.VehicleType))
                    .Select(x => x.VehicleType)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToListAsync(cancellationToken),
                VehicleYears = await _processerContext.PlateGroups
                    .Where(x => !string.IsNullOrWhiteSpace(x.VehicleYear))
                    .Select(x => x.VehicleYear)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToListAsync(cancellationToken),
            };

            response.VehicleMakes = response.VehicleMakes
                .Select(x => _textInfo.ToTitleCase(x))
                .ToList();

            response.VehicleModels = response.VehicleModels
                .Select(x => _textInfo.ToTitleCase(x.Split('_')[1]))
                .ToList();

            response.VehicleColors = response.VehicleColors
                .Select(x => _textInfo.ToTitleCase(x))
                .ToList();

            response.VehicleTypes = response.VehicleTypes
                .Select(x => _textInfo.ToTitleCase(x))
                .ToList();

            return response;
        }
    }
}
