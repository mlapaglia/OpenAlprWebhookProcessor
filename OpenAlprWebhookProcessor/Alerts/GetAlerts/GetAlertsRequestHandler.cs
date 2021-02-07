using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Alerts
{
    public class GetAlertsRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        public GetAlertsRequestHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task<List<Alert>> HandleAsync(CancellationToken cancellationToken)
        {
            var alerts = new List<Alert>();

            foreach (var dbAlert in await _processorContext.Alerts.ToListAsync())
            {
                var alert = new Alert()
                {
                    Id = dbAlert.Id,
                    PlateNumber = dbAlert.PlateNumber,
                    StrictMatch = dbAlert.IsStrictMatch,
                    Description = dbAlert.Description,
                };

                alerts.Add(alert);
            }

            return alerts;
        }
    }
}