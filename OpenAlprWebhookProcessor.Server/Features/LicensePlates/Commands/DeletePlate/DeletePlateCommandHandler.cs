using Mediator;
using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Commands.DeletePlate
{
    public class DeletePlateCommandHandler : ICommandHandler<DeletePlateCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeletePlateCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<Unit> Handle(DeletePlateCommand request, CancellationToken cancellationToken)
        {
            var plateGroup = await _unitOfWork.PlateGroups.GetByIdAsync(
                request.Id,
                cancellationToken);
            
            if (plateGroup == null)
            {
                throw new ArgumentException($"Plate with ID {request.Id} not found");
            }

            _unitOfWork.PlateGroups.Delete(plateGroup);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
} 