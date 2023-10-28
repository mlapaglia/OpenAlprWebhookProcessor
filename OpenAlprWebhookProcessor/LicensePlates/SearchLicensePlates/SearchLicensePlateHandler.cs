using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.LicensePlates.SearchLicensePlates
{
    public class SearchLicensePlateHandler
    {
        private readonly ProcessorContext _processerContext;

        public SearchLicensePlateHandler(ProcessorContext processorContext)
        {
            _processerContext = processorContext;
        }

        public async Task<SearchLicensePlateResponse> HandleAsync(
            SearchLicensePlateRequest request,
            CancellationToken cancellationToken)
        {
            _processerContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var dbRequest = _processerContext.PlateGroups
                .AsQueryable()
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.PlateNumber))
            {
                request.PlateNumber = request.PlateNumber
                    .Trim()
                    .ToUpper();

                if (request.StrictMatch)
                {
                    dbRequest = dbRequest.Where(x => x.BestNumber == request.PlateNumber);
                }
                else if (request.RegexSearchEnabled)
                {
                    dbRequest = dbRequest.Where(x => Regex.IsMatch(x.BestNumber, request.PlateNumber));
                }
                else
                {
                    dbRequest = dbRequest.Where(x => x.PossibleNumbers.Any(x => x.Number == request.PlateNumber) || request.PlateNumber == x.BestNumber);
                }
            }

            if (request.StartSearchOn != null)
            {
                var startEpochMilliseconds = request.StartSearchOn.Value.ToUnixTimeMilliseconds();
                dbRequest = dbRequest.Where(x => x.ReceivedOnEpoch >= startEpochMilliseconds);
            }

            if (request.EndSearchOn != null)
            {
                var endEpochMilliseconds = request.EndSearchOn.Value.ToUnixTimeMilliseconds();
                dbRequest = dbRequest.Where(x => x.ReceivedOnEpoch <= endEpochMilliseconds);
            }

            var platesToIgnore = await _processerContext.Ignores
                .Select(x => x.PlateNumber)
                .ToListAsync(cancellationToken);

            if (!request.FilterIgnoredPlates)
            {
                dbRequest = dbRequest.Where(x => !x.PossibleNumbers.Any(y => platesToIgnore.Contains(y.Number)) && !platesToIgnore.Contains(x.BestNumber));
            }

            if (!string.IsNullOrWhiteSpace(request.VehicleColor))
            {
                dbRequest = dbRequest.Where(x => x.VehicleColor.Contains(request.VehicleColor.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(request.VehicleMake))
            {
                dbRequest = dbRequest.Where(x => x.VehicleMakeModel.Contains(request.VehicleMake.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(request.VehicleModel))
            {
                dbRequest = dbRequest.Where(x => x.VehicleMakeModel.Contains(request.VehicleModel.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(request.VehicleType))
            {
                dbRequest = dbRequest.Where(x => x.VehicleType.Contains(request.VehicleType.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(request.VehicleRegion))
            {
                dbRequest = dbRequest.Where(x => x.VehicleRegion.Contains(request.VehicleRegion.ToLower()));
            }

            if (request.FilterPlatesSeenLessThan > 0)
            {
                var platesSeen = await _processerContext.PlateGroups
                    .AsNoTracking()
                    .GroupBy(x => x.BestNumber)
                    .Where(x => x.Count() > request.FilterPlatesSeenLessThan)
                    .Select(x => x.Key)
                    .ToListAsync(cancellationToken);

                dbRequest = dbRequest.Where(x => !platesSeen.Contains(x.BestNumber)); 
            }

            var totalCount = await dbRequest.CountAsync(cancellationToken);

            dbRequest = dbRequest
                .Include(x => x.PossibleNumbers)
                .OrderByDescending(x => x.ReceivedOnEpoch)
                .Skip(request.PageNumber * request.PageSize)
                .Take(request.PageSize);

            var results = await dbRequest.ToListAsync(cancellationToken);

            var licensePlates = new List<LicensePlate>();

            var platesToAlert = await GetPlatesToAlertAsync(cancellationToken);

            foreach (var plate in results)
            {
                licensePlates.Add(PlateMapper.MapPlate(
                    plate,
                    platesToIgnore,
                    platesToAlert));
            }

            var enricher = await _processerContext.Enrichers
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (enricher == null || !enricher.IsEnabled)
            {
                licensePlates.ForEach(x => x.CanBeEnriched = false);
            }

            return new SearchLicensePlateResponse()
            {
                Plates = licensePlates,
                TotalCount = totalCount,
            };
        }

        private async Task<List<string>> GetPlatesToAlertAsync(CancellationToken cancellationToken)
        {
            return await _processerContext.Alerts
                .AsNoTracking()
                .Select(x => x.PlateNumber)
                .ToListAsync(cancellationToken);
        }
    }
}
