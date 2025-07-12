using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.LicensePlates;
using OpenAlprWebhookProcessor.LicensePlates.SearchLicensePlates;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Queries.SearchLicensePlates
{
    public class SearchLicensePlatesQueryHandler : IRequestHandler<SearchLicensePlatesQuery, SearchLicensePlateResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public SearchLicensePlatesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<SearchLicensePlateResponse> Handle(SearchLicensePlatesQuery request, CancellationToken cancellationToken)
        {
            var platesToIgnore = await GetPlatesToIgnoreAsync(request.FilterIgnoredPlates, cancellationToken);
            
            var plates = await _unitOfWork.PlateGroups.SearchPlatesAsync(
                request.PlateNumber,
                request.StrictMatch,
                request.RegexSearchEnabled,
                request.StartSearchOn,
                request.EndSearchOn,
                platesToIgnore,
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
                platesToIgnore,
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
                    platesToIgnore,
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

        private async Task<List<string>> GetPlatesToIgnoreAsync(bool filterIgnoredPlates, CancellationToken cancellationToken)
        {
            if (filterIgnoredPlates)
            {
                return new List<string>();
            }

            var ignores = await _unitOfWork.Ignores.GetAllAsync(cancellationToken);
            return ignores.Select(x => x.PlateNumber).ToList();
        }

        private async Task<List<string>> GetPlatesToAlertAsync(CancellationToken cancellationToken)
        {
            var alerts = await _unitOfWork.Alerts.GetAllAsync(cancellationToken);
            return alerts.Select(x => x.PlateNumber).ToList();
        }
    }
} 