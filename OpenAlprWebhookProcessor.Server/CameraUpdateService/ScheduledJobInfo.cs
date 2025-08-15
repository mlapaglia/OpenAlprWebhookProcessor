using System;

namespace OpenAlprWebhookProcessor.CameraUpdateService
{
    public class ScheduledJobInfo
    {
        public string JobId { get; set; }

        public DateTimeOffset ScheduledExecutionTime { get; set; }

        public ScheduledJobType JobType { get; set; }

        public Guid? CameraId { get; set; }

        public SunriseSunset? SunriseSunsetType { get; set; }
    }
}
