using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Settings.GetDebugPlateGroups
{
    public class GetDebugPlateGroupRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        public GetDebugPlateGroupRequestHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task<string> HandleAsync(
            bool onlyFailedPlateGroups,
            CancellationToken cancellationToken)
        {
            var query = _processorContext.RawPlateGroups
                .AsNoTracking()
                .AsQueryable();

            var stopEpoch = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeMilliseconds();

            query = query.Where(x => x.ReceivedOnEpoch > stopEpoch);

            if (onlyFailedPlateGroups)
            {
                query = query.Where(x => !x.WasProcessedCorrectly);
            }

            var results = await query
                .Select(x => x.RawPlateGroup)
                .ToListAsync(cancellationToken);

            return "[" + String.Join(",", results.Take(10).ToList()) + "]";
        }
    }
}
