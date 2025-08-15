namespace OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetQuickStats
{
    public class GetQuickStatsResponse
    {
        public int TodayCount { get; set; }
        public int WeekCount { get; set; }
        public int MonthCount { get; set; }
        public int UniquePlatesThisWeek { get; set; }
        public int ActiveCameras { get; set; }
        public int AverageDailyPlates { get; set; }
    }
} 