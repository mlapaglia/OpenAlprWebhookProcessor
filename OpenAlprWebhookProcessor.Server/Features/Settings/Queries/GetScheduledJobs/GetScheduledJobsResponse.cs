using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Features.Settings.Queries.GetScheduledJobs
{
    public class GetScheduledJobsResponse
    {
        public List<ScheduledJobDto> ScheduledJobs { get; set; } = new List<ScheduledJobDto>();
    }
}