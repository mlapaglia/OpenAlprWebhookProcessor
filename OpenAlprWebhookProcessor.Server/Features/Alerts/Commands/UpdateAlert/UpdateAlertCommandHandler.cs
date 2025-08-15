using Mediator;
using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Alerts.Commands.UpdateAlert
{
    public class UpdateAlertCommandHandler : ICommandHandler<UpdateAlertCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateAlertCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<Unit> Handle(UpdateAlertCommand command, CancellationToken cancellationToken = default)
        {
            var alert = command.Alert;
            var dbAlert = await _unitOfWork.Alerts.GetByIdAsync(alert.Id, cancellationToken);
            
            if (dbAlert == null)
            {
                throw new InvalidOperationException($"Alert with ID {alert.Id} not found");
            }

            dbAlert.PlateNumber = alert.PlateNumber.ToUpper();
            dbAlert.Description = alert.Description;
            dbAlert.IsStrictMatch = alert.StrictMatch;

            _unitOfWork.Alerts.Update(dbAlert);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
