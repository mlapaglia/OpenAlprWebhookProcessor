using Mediator;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenAlprWebhookProcessor.CameraUpdateService;

namespace OpenAlprWebhookProcessor.Features.Settings.Queries.GetScheduledJobs
{
    public class GetScheduledJobsQueryHandler : IRequestHandler<GetScheduledJobsQuery, GetScheduledJobsResponse>
    {
        private readonly ISimpleCameraScheduler _simpleCameraScheduler;

        public GetScheduledJobsQueryHandler(ISimpleCameraScheduler simpleCameraScheduler)
        {
            _simpleCameraScheduler = simpleCameraScheduler;
        }

        public ValueTask<GetScheduledJobsResponse> Handle(GetScheduledJobsQuery request, CancellationToken cancellationToken)
        {
            var scheduledJobs = _simpleCameraScheduler.GetAllScheduledJobs();
            
            var scheduledJobDtos = scheduledJobs.Select(job => new ScheduledJobDto
            {
                JobId = job.JobId,
                ScheduledExecutionTime = job.ScheduledExecutionTime,
                JobType = job.JobType,
                CameraId = job.CameraId,
                SunriseSunsetType = job.SunriseSunsetType,
            }).ToList();

            var response = new GetScheduledJobsResponse
            {
                ScheduledJobs = scheduledJobDtos
            };

            return ValueTask.FromResult(response);
        }
    }
}