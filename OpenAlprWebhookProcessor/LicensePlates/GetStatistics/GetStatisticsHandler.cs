using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.LicensePlates.GetStatistics
{
    public class GetStatisticsHandler
    {
        private readonly ProcessorContext _processorContext;

        public GetStatisticsHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task<PlateStatistics> HandleAsync(
            string plateNumber,
            CancellationToken cancellationToken)
        {
            var endingEpoch = DateTimeOffset.UtcNow.AddDays(-90).ToUnixTimeMilliseconds();

            var seenPlates = await _processorContext.PlateGroups
                .AsNoTracking()
                .Where(x => x.BestNumber == plateNumber)
                .Select(x => x.ReceivedOnEpoch)
                .ToListAsync(cancellationToken);

            var seenPossiblePlates = await _processorContext.PlateGroupPossibleNumbers
                .AsNoTracking()
                .Where(x => x.Number == plateNumber)
                .Select(x => x.PlateGroup.ReceivedOnEpoch)
                .ToListAsync(cancellationToken);

            seenPlates.AddRange(seenPossiblePlates);
            seenPlates = seenPlates.OrderBy(x => x).ToList();

            var plateStatistics = new PlateStatistics
            {
                TotalSeen = seenPlates.Count,
                Last90Days = seenPlates
                    .Count(x => x > endingEpoch)
            };

            var firstSeenEpoch = seenPlates
                .FirstOrDefault();

            if (firstSeenEpoch != 0)
            {
                plateStatistics.FirstSeen = DateTimeOffset.FromUnixTimeMilliseconds(firstSeenEpoch);
            }

            var lastSeenEpoch = seenPlates
                .LastOrDefault();

            if (lastSeenEpoch != 0)
            {
                plateStatistics.LastSeen = DateTimeOffset.FromUnixTimeMilliseconds(lastSeenEpoch);
            }

            return plateStatistics;
        }
    }
}