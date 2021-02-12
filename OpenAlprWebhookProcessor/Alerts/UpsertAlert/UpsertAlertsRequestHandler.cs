using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Alerts
{
    public class UpsertAlertsRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        public UpsertAlertsRequestHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task AddAlertAsync(Alert alert)
        {
            var dbIgnores = await _processorContext.Alerts
                .Where(x => x.PlateNumber == alert.PlateNumber)
                .ToListAsync();

            if (dbIgnores != null)
            {
                var addedAlert = new Data.Alert()
                {
                    Description = alert.Description,
                    IsStrictMatch = alert.StrictMatch,
                    PlateNumber = alert.PlateNumber,
                };

                _processorContext.Alerts.Add(addedAlert);

                await _processorContext.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException("ignore already exists");
            }
        }

        public async Task UpsertAlertsAsync(List<Alert> alerts)
        {
            alerts = alerts.Where(x => !string.IsNullOrWhiteSpace(x.PlateNumber)).ToList();

            var dbAlerts = await _processorContext.Alerts.ToListAsync();

            var ignoresToRemove = dbAlerts.Where(p => !alerts.Any(p2 => p2.Id == p.Id));

            _processorContext.RemoveRange(ignoresToRemove);

            var alertsToUpdate = dbAlerts.Where(x => alerts.Any(p2 => p2.Id == x.Id));

            foreach (var alertToUpdate in alertsToUpdate)
            {
                var updatedAlert = alerts.First(x => x.Id == alertToUpdate.Id);

                alertToUpdate.Description = updatedAlert.Description;
                alertToUpdate.IsStrictMatch = updatedAlert.StrictMatch;
                alertToUpdate.PlateNumber = updatedAlert.PlateNumber;
            }

            var alertsToAdd = alerts.Where(x => !dbAlerts.Any(p2 => p2.Id == x.Id));

            foreach (var alertToAdd in alertsToAdd)
            {
                var addedAlert = new Data.Alert()
                {
                    Description = alertToAdd.Description,
                    IsStrictMatch = alertToAdd.StrictMatch,
                    PlateNumber = alertToAdd.PlateNumber,
                };

                _processorContext.Alerts.Add(addedAlert);
            }

            await _processorContext.SaveChangesAsync();
        }
    }
}
