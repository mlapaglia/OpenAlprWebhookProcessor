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
            var dbRequest = _processerContext.PlateGroups.AsQueryable();

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
                    dbRequest = dbRequest.Where(x => x.PossibleNumbers.Contains(request.PlateNumber) || request.PlateNumber == x.BestNumber);
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

            var platesToIgnore = (await _processerContext.Ignores
                .Select(x => x.PlateNumber)
                .ToListAsync(cancellationToken));

            if (!request.FilterIgnoredPlates)
            {
                dbRequest = dbRequest.Where(x => !platesToIgnore.Contains(x.PossibleNumbers) && !platesToIgnore.Contains(x.BestNumber));
            }

            if (!string.IsNullOrWhiteSpace(request.VehicleMake))
            {
                dbRequest = dbRequest.Where(x => x.VehicleMake.Contains(request.VehicleMake.ToLower()));
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

            var totalCount = await dbRequest.CountAsync(cancellationToken);

            dbRequest = dbRequest
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

            return new SearchLicensePlateResponse()
            {
                Plates = licensePlates,
                TotalCount = totalCount,
            };
        }

        private async Task<List<string>> GetPlatesToAlertAsync(CancellationToken cancellationToken)
        {
            return (await _processerContext.Alerts
                .Select(x => x.PlateNumber)
                .ToListAsync(cancellationToken));
        }
    }
}
