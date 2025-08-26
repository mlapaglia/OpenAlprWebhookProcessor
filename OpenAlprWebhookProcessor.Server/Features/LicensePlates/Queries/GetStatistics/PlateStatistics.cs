using System;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetStatistics
{
    public class PlateStatistics
    {
        public int Last90Days { get; set; }

        public int TotalSeen { get; set; }

        public DateTimeOffset FirstSeen { get; set; }

        public DateTimeOffset LastSeen { get; set; }
    }

    public class PlateStatisticsAggregation
    {
        public int TotalCount { get; set; }

        public int Last90DaysCount { get; set; }

        public long MinEpoch { get; set; }

        public long MaxEpoch { get; set; }
    }
}
