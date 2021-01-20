using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<List<Alert>> HandleAsync()
        {
            var alerts = new List<Alert>();

            foreach (var alert in await _processorContext.Alerts.ToListAsync())
            {
                alerts.Add(new Alert());
            }

            return alerts;
        }
    }
}