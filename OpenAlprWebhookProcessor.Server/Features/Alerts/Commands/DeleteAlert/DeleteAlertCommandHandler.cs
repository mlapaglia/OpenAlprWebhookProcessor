using Mediator;
using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Alerts.Commands.DeleteAlert
{
    public class DeleteAlertCommandHandler : ICommandHandler<DeleteAlertCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteAlertCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<Unit> Handle(DeleteAlertCommand command, CancellationToken cancellationToken = default)
        {
            var alert = await _unitOfWork.Alerts.GetByIdAsync(command.Id, cancellationToken);
            
            if (alert == null)
            {
                throw new InvalidOperationException($"Alert with ID {command.Id} not found");
            }

            _unitOfWork.Alerts.Delete(alert);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
