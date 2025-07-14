using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Settings.Commands.DeleteDebugPlates
{
    public class DeleteDebugPlatesCommandHandler : IRequestHandler<DeleteDebugPlatesCommand>
    {
        private readonly ProcessorContext _processorContext;

        public DeleteDebugPlatesCommandHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task Handle(DeleteDebugPlatesCommand request, CancellationToken cancellationToken)
        {
            await _processorContext.Database.ExecuteSqlRawAsync("DELETE FROM RawPlateGroups;", cancellationToken);
        }
    }
} 