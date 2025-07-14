using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Settings.Queries.GetDebugPlates
{
    public class GetDebugPlatesQueryHandler : IRequestHandler<GetDebugPlatesQuery, string>
    {
        private readonly ProcessorContext _processorContext;

        public GetDebugPlatesQueryHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task<string> Handle(GetDebugPlatesQuery request, CancellationToken cancellationToken)
        {
            var query = _processorContext.RawPlateGroups
                .AsNoTracking()
                .AsQueryable();

            var stopEpoch = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeMilliseconds();

            query = query.Where(x => x.ReceivedOnEpoch > stopEpoch);

            if (request.OnlyFailedPlateGroups)
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