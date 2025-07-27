using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Settings.Commands.DeleteDebugPlates
{
    public class DeleteDebugPlatesCommandHandler : IRequestHandler<DeleteDebugPlatesCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteDebugPlatesCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteDebugPlatesCommand request, CancellationToken cancellationToken = default)
        {
            var allRawPlateGroups = await _unitOfWork.RawPlateGroups.GetAllAsync(cancellationToken);
            _unitOfWork.RawPlateGroups.DeleteRange(allRawPlateGroups);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
} 