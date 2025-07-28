using Mediator;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Commands.EditPlate
{
    public class EditPlateCommandHandler : ICommandHandler<EditPlateCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public EditPlateCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<Unit> Handle(EditPlateCommand request, CancellationToken cancellationToken = default)
        {
            var existingPlate = await _unitOfWork.PlateGroups.GetByIdAsync(
                request.Id,
                cancellationToken);
            
            if (existingPlate != null)
            {
                existingPlate.BestNumber = request.PlateNumber;
                
                _unitOfWork.PlateGroups.Update(existingPlate);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else
            {
                throw new System.ArgumentException($"Plate with ID {request.Id} not found");
            }
            return Unit.Value;
        }
    }
} 