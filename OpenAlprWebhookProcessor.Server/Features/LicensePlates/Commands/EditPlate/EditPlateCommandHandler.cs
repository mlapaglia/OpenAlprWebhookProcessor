using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Commands.EditPlate
{
    public class EditPlateCommandHandler : IRequestHandler<EditPlateCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public EditPlateCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(EditPlateCommand request, CancellationToken cancellationToken)
        {
            var existingPlate = await _unitOfWork.PlateGroups.GetByIdAsync(request.Id, cancellationToken);
            
            if (existingPlate != null)
            {
                // Update existing plate
                existingPlate.BestNumber = request.PlateNumber;
                
                _unitOfWork.PlateGroups.Update(existingPlate);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else
            {
                throw new System.ArgumentException($"Plate with ID {request.Id} not found");
            }
        }
    }
} 