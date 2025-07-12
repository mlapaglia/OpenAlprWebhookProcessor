using System;

namespace OpenAlprWebhookProcessor.Server.Features.LicensePlates.Queries.GetLicensePlateCounts
{
    public class DayCount
    {
        public DateTimeOffset Date { get; set; }

        public int Count { get; set; }
    }
}
