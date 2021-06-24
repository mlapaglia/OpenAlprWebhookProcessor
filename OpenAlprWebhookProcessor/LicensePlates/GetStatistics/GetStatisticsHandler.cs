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

            var recentRecords = await _processorContext.PlateGroups
                .Where(x => x.ReceivedOnEpoch > endingEpoch)
                .Where(x => x.BestNumber == plateNumber || x.PossibleNumbers.Contains(plateNumber))
                .CountAsync(cancellationToken);

            plateStatistics.Last90Days = recentRecords;

            var firstSeenEpoch = await _processorContext.PlateGroups
                .Where(x => x.ReceivedOnEpoch > endingEpoch)
                .Where(x => x.BestNumber == plateNumber || x.PossibleNumbers.Contains(plateNumber))
                .Select(x => x.ReceivedOnEpoch)
                .FirstOrDefaultAsync(cancellationToken);

            if (firstSeenEpoch != 0)
            {
                plateStatistics.FirstSeen = DateTimeOffset.FromUnixTimeMilliseconds(firstSeenEpoch);
            }

            var lastSeenEpoch = await _processorContext.PlateGroups
                .Where(x => x.ReceivedOnEpoch > endingEpoch)
                .Where(x => x.BestNumber == plateNumber || x.PossibleNumbers.Contains(plateNumber))
                .OrderByDescending(x => x.ReceivedOnEpoch)
                .Select(x => x.ReceivedOnEpoch)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastSeenEpoch != 0)
            {
                plateStatistics.LastSeen = DateTimeOffset.FromUnixTimeMilliseconds(lastSeenEpoch);
            }

            return plateStatistics;
        }
    }
}
