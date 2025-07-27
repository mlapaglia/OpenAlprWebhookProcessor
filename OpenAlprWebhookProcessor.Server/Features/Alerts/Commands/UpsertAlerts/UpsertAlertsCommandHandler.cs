using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Alerts.Commands.UpsertAlerts
{
    public class UpsertAlertsCommandHandler : IRequestHandler<UpsertAlertsCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpsertAlertsCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpsertAlertsCommand request, CancellationToken cancellationToken = default)
        {
            var alerts = request.Alerts.Where(x => !string.IsNullOrWhiteSpace(x.PlateNumber)).ToList();

            var dbAlerts = await _unitOfWork.Alerts.GetAllAsync(cancellationToken);

            var alertsToRemove = dbAlerts.Where(p => !alerts.Any(p2 => p2.Id == p.Id));
            _unitOfWork.Alerts.DeleteRange(alertsToRemove);

            var alertsToUpdate = dbAlerts.Where(x => alerts.Any(p2 => p2.Id == x.Id));

            foreach (var alertToUpdate in alertsToUpdate)
            {
                var updatedAlert = alerts.First(x => x.Id == alertToUpdate.Id);
                alertToUpdate.PlateNumber = updatedAlert.PlateNumber.ToUpper();
                alertToUpdate.Description = updatedAlert.Description;
                alertToUpdate.IsStrictMatch = updatedAlert.StrictMatch;
                _unitOfWork.Alerts.Update(alertToUpdate);
            }

            var alertsToAdd = alerts.Where(x => !dbAlerts.Any(p2 => p2.Id == x.Id));

            foreach (var alertToAdd in alertsToAdd)
            {
                var newAlert = new Data.Alert()
                {
                    PlateNumber = alertToAdd.PlateNumber.ToUpper(),
                    Description = alertToAdd.Description,
                    IsStrictMatch = alertToAdd.StrictMatch,
                };

                await _unitOfWork.Alerts.AddAsync(newAlert, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
} 