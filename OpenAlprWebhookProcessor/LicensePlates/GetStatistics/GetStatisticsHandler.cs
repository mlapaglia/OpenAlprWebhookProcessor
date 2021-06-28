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
            var plateStatistics = new PlateStatistics();

            var endingEpoch = DateTimeOffset.UtcNow.AddDays(-90).ToUnixTimeMilliseconds();

            var seenPlates = await _processorContext.PlateGroups
                .Where(x => x.BestNumber == plateNumber || x.PossibleNumbers.Contains(plateNumber))
                .Select(x => x.ReceivedOnEpoch)
                .ToListAsync(cancellationToken);

            plateStatistics.TotalSeen = seenPlates.Count;

            plateStatistics.Last90Days = seenPlates
                .Count(x => x > endingEpoch);

            var firstSeenEpoch = seenPlates
                .FirstOrDefault();

            if (firstSeenEpoch != 0)
            {
                plateStatistics.FirstSeen = DateTimeOffset.FromUnixTimeMilliseconds(firstSeenEpoch);
            }

            var lastSeenEpoch = seenPlates
                .OrderByDescending(x => x)
                .Select(x => x)
                .FirstOrDefault();

            if (lastSeenEpoch != 0)
            {
                plateStatistics.LastSeen = DateTimeOffset.FromUnixTimeMilliseconds(lastSeenEpoch);
            }

            return plateStatistics;
        }
    }
}
