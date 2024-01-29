using System;

namespace OpenAlprWebhookProcessor.LicensePlates.GetLicensePlateCounts
{
    public class DayCount
    {
        public DateTimeOffset Date { get; set; }

        public int Count { get; set; }
    }
}
