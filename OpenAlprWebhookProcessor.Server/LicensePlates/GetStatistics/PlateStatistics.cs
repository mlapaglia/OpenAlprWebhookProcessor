using System;

namespace OpenAlprWebhookProcessor.Server.LicensePlates.GetStatistics
{
    public class PlateStatistics
    {
        public int Last90Days { get; set; }

        public int TotalSeen { get; set; }

        public DateTimeOffset FirstSeen { get; set; }

        public DateTimeOffset LastSeen { get; set; }
    }
}
