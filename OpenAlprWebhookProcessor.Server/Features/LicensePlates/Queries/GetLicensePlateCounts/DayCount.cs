using System;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetLicensePlateCounts
{
    public class DayCount
    {
        public DateTimeOffset Date { get; set; }

        public int Count { get; set; }
    }
}
