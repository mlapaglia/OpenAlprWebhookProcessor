using Mediator;
using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Alerts.Commands.AddAlert
{
    public class AddAlertCommandHandler : ICommandHandler<AddAlertCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddAlertCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<Unit> Handle(AddAlertCommand command, CancellationToken cancellationToken)
        {
            var existingAlerts = await _unitOfWork.Alerts.FindAsync(
                x => x.PlateNumber == command.Alert.PlateNumber.ToUpper(),
                cancellationToken);

            if (!existingAlerts.Any())
            {
                var newAlert = new Data.Alert()
                {
                    Description = command.Alert.Description,
                    IsStrictMatch = command.Alert.StrictMatch,
                    PlateNumber = command.Alert.PlateNumber.ToUpper(),
                };

                await _unitOfWork.Alerts.AddAsync(newAlert, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else
            {
                throw new ArgumentException("alert already exists");
            }
            
            return Unit.Value;
        }
    }
} 