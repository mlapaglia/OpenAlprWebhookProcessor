using Mediator;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Settings.Commands.DeleteDebugPlates
{
    public class DeleteDebugPlatesCommandHandler : ICommandHandler<DeleteDebugPlatesCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteDebugPlatesCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<Unit> Handle(DeleteDebugPlatesCommand request, CancellationToken cancellationToken)
        {
            var allRawPlateGroups = await _unitOfWork.RawPlateGroups.GetAllAsync(cancellationToken);
            _unitOfWork.RawPlateGroups.DeleteRange(allRawPlateGroups);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
} 