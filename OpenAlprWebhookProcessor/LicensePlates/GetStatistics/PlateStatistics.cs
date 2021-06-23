using System;

namespace OpenAlprWebhookProcessor.LicensePlates.GetStatistics
{
    public class PlateStatistics
    {
        public int Last90Days { get; set; }

        public DateTimeOffset FirstSeen { get; set; }
    }
}
