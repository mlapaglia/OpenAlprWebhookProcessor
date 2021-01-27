using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Settings.GetAlerts
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

            foreach (var alert in await _processorContext.Alerts.ToListAsync(cancellationToken))
            {
                alerts.Add(new Alert());
            }

            return alerts;
        }
    }
}