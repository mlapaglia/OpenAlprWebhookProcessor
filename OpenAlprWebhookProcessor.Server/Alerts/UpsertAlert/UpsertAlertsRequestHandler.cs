using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Alerts
{
    public class UpsertAlertsRequestHandler
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpsertAlertsRequestHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AddAlertAsync(Alert alert)
        {
            var existingAlerts = await _unitOfWork.Alerts.FindAsync(
                x => x.PlateNumber == alert.PlateNumber.ToUpper());

            if (existingAlerts.Any())
            {
                throw new ArgumentException("alert already exists");
            }

            var addedAlert = new Data.Alert()
            {
                Description = alert.Description,
                IsStrictMatch = alert.StrictMatch,
                PlateNumber = alert.PlateNumber.ToUpper(),
            };

            await _unitOfWork.Alerts.AddAsync(addedAlert);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpsertAlertsAsync(List<Alert> alerts)
        {
            alerts = alerts.Where(x => !string.IsNullOrWhiteSpace(x.PlateNumber)).ToList();

            var dbAlerts = (await _unitOfWork.Alerts.GetAllAsync()).ToList();

            var alertsToRemove = dbAlerts.Where(p => !alerts.Any(p2 => p2.Id == p.Id));

            _unitOfWork.Alerts.DeleteRange(alertsToRemove);

            var alertsToUpdate = dbAlerts.Where(x => alerts.Any(p2 => p2.Id == x.Id));

            foreach (var alertToUpdate in alertsToUpdate)
            {
                var updatedAlert = alerts.First(x => x.Id == alertToUpdate.Id);

                alertToUpdate.Description = updatedAlert.Description;
                alertToUpdate.IsStrictMatch = updatedAlert.StrictMatch;
                alertToUpdate.PlateNumber = updatedAlert.PlateNumber.ToUpper();
            }

            _unitOfWork.Alerts.UpdateRange(alertsToUpdate);

            var alertsToAdd = alerts.Where(x => !dbAlerts.Any(p2 => p2.Id == x.Id));

            foreach (var alertToAdd in alertsToAdd)
            {
                var addedAlert = new Data.Alert()
                {
                    Description = alertToAdd.Description,
                    IsStrictMatch = alertToAdd.StrictMatch,
                    PlateNumber = alertToAdd.PlateNumber.ToUpper(),
                };

                await _unitOfWork.Alerts.AddAsync(addedAlert);
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
