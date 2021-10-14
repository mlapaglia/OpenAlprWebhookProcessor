using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Settings.GetDebubPlateGroups
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
            var query = _processorContext.RawPlateGroups.AsQueryable();

            if (onlyFailedPlateGroups)
            {
                query = query.Where(x => x.PlateGroup == null);
            }

            var results = await query
                .Select(x => x.RawPlateGroup)
                .ToListAsync(cancellationToken);

            return JsonSerializer.Serialize(results);
        }
    }
}
