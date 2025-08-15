using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetHourlyStats
{
    public class GetHourlyStatsResponse
    {
        public List<HourlyCount> Counts { get; set; } = new List<HourlyCount>();
    }

    public class HourlyCount
    {
        public int Hour { get; set; }
        public int Count { get; set; }
    }
} 