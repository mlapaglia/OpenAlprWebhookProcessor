using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Alerts.Commands.AddAlert
{
    public class AddAlertCommandHandler : IRequestHandler<AddAlertCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddAlertCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(AddAlertCommand request, CancellationToken cancellationToken)
        {
            var existingAlerts = await _unitOfWork.Alerts.FindAsync(
                x => x.PlateNumber == request.Alert.PlateNumber.ToUpper(),
                cancellationToken);

            if (!existingAlerts.Any())
            {
                var newAlert = new Data.Alert()
                {
                    Description = request.Alert.Description,
                    IsStrictMatch = request.Alert.StrictMatch,
                    PlateNumber = request.Alert.PlateNumber.ToUpper(),
                };

                await _unitOfWork.Alerts.AddAsync(newAlert, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else
            {
                throw new ArgumentException("alert already exists");
            }
        }
    }
} 