using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System.Collections.Generic;
using System.Linq;
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
                    dbRequest = dbRequest.Where(x => x.BestNumber.Contains(request.PlateNumber));
                }
                else
                {
                    dbRequest = dbRequest.Where(x =>
                        x.BestNumber.Contains(request.PlateNumber)
                        || x.PossibleNumbers.Contains(request.PlateNumber));
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

            var platesToIgnore = await _processerContext.Ignores.ToListAsync(cancellationToken);

            if (!request.FilterIgnoredPlates)
            {
                foreach (var plateToIgnore in platesToIgnore)
                {
                    if (plateToIgnore.IsStrictMatch)
                    {
                        dbRequest = dbRequest.Where(x => plateToIgnore.PlateNumber != x.BestNumber);
                    }
                    else
                    {
                        dbRequest = dbRequest.Where(x =>
                            plateToIgnore.PlateNumber != x.BestNumber
                            || !x.PossibleNumbers.Contains(plateToIgnore.PlateNumber));
                    }
                }
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
                    platesToIgnore.Select(x => x.PlateNumber).ToList(),
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
            return (await _processerContext.Alerts.ToListAsync(cancellationToken))
                .Select(x => x.PlateNumber)
                .ToList();
        }
    }
}
