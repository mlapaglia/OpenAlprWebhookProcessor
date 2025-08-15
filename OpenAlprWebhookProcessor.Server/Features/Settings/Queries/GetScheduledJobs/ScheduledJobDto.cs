using System;
using OpenAlprWebhookProcessor.CameraUpdateService;

namespace OpenAlprWebhookProcessor.Features.Settings.Queries.GetScheduledJobs
{
    public class ScheduledJobDto
    {
        public string JobId { get; set; }

        public DateTimeOffset ScheduledExecutionTime { get; set; }

        public ScheduledJobType JobType { get; set; }

        public Guid? CameraId { get; set; }

        public SunriseSunset? SunriseSunsetType { get; set; }
    }
}