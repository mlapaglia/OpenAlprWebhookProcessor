using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Settings.GetDebubPlateGroups
{
    public class DeleteDebugPlateGroupRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        public DeleteDebugPlateGroupRequestHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task HandleAsync(CancellationToken cancellationToken)
        {
            await _processorContext.Database.ExecuteSqlRawAsync("DELETE FROM RawPlateGroups;", cancellationToken);
        }
    }
}
