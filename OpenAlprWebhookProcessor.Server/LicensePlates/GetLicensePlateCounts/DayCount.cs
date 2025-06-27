using System;

namespace OpenAlprWebhookProcessor.Server.LicensePlates.GetLicensePlateCounts
{
    public class DayCount
    {
        public DateTimeOffset Date { get; set; }

        public int Count { get; set; }
    }
}
