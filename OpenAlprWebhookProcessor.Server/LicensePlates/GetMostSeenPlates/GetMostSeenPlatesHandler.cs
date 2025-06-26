using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.LicensePlates.GetMostSeenPlates
{
    public class GetMostSeenPlatesHandler
    {
        private readonly ProcessorContext _processorContext;

        public GetMostSeenPlatesHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task<GetMostSeenPlatesResponse> HandleAsync(
            GetMostSeenPlatesRequest request,
            CancellationToken cancellationToken)
        {
            var aWeekAgo = DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeMilliseconds();

            var results = await _processorContext.PlateGroups
                .AsNoTracking()
                .Where(x => x.ReceivedOnEpoch > aWeekAgo)
                .GroupBy(x => x.BestNumber)
                .Select(x => new MostSeenCount
                {
                    PlateNumber = x.Key,
                    Count = x.Count(),
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync(cancellationToken);

            return new GetMostSeenPlatesResponse()
            {
                Counts = results,
            };
        }
    }
}
