using Mediator;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Queries.SearchLicensePlates
{
    public class SearchLicensePlatesQueryHandler : IQueryHandler<SearchLicensePlatesQuery, SearchLicensePlateResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public SearchLicensePlatesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<SearchLicensePlateResponse> Handle(SearchLicensePlatesQuery request, CancellationToken cancellationToken = default)
        {
            var ignoredPlates = await GetIgnoredPlatesAsync(cancellationToken);
            var platesToIgnoreForFiltering = request.IncludeIgnoredPlates ? new List<string>() : ignoredPlates;
            
            var plates = await _unitOfWork.PlateGroups.SearchPlatesAsync(
                request.PlateNumber,
                request.StrictMatch,
                request.RegexSearchEnabled,
                request.StartSearchOn,
                request.EndSearchOn,
                platesToIgnoreForFiltering,
                request.VehicleColor,
                request.VehicleMake,
                request.VehicleModel,
                request.VehicleType,
                request.VehicleRegion,
                request.FilterPlatesSeenLessThan,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            var totalCount = await _unitOfWork.PlateGroups.GetSearchResultsCountAsync(
                request.PlateNumber,
                request.StrictMatch,
                request.RegexSearchEnabled,
                request.StartSearchOn,
                request.EndSearchOn,
                platesToIgnoreForFiltering,
                request.VehicleColor,
                request.VehicleMake,
                request.VehicleModel,
                request.VehicleType,
                request.VehicleRegion,
                request.FilterPlatesSeenLessThan,
                cancellationToken);

            var platesToAlert = await GetPlatesToAlertAsync(cancellationToken);
            var licensePlates = new List<LicensePlate>();

            foreach (var plate in plates)
            {
                licensePlates.Add(PlateMapper.MapPlate(
                    plate,
                    ignoredPlates,
                    platesToAlert));
            }

            var enricher = await _unitOfWork.Enrichers.FirstOrDefaultAsync(e => e.IsEnabled, cancellationToken);
            if (enricher == null)
            {
                licensePlates.ForEach(x => x.CanBeEnriched = false);
            }

            return new SearchLicensePlateResponse
            {
                Plates = licensePlates,
                TotalCount = totalCount,
            };
        }

        private async Task<List<string>> GetIgnoredPlatesAsync(CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.Ignores.SelectAsync(x => x.PlateNumber, cancellationToken);
        }

        private async Task<List<string>> GetPlatesToAlertAsync(CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.Alerts.SelectAsync(x => x.PlateNumber, cancellationToken);
        }
    }
} 