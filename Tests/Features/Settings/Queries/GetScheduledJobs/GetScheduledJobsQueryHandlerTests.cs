using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetScheduledJobs;

namespace Tests.Features.Settings.Queries.GetScheduledJobs
{
    [TestFixture]
    public class GetScheduledJobsQueryHandlerTests
    {
        private ISimpleCameraScheduler _cameraScheduler;
        private GetScheduledJobsQueryHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _cameraScheduler = Substitute.For<ISimpleCameraScheduler>();
            _handler = new GetScheduledJobsQueryHandler(_cameraScheduler);
        }

        [Test]
        public async Task Handle_WithScheduledJobs_ReturnsScheduledJobsResponse()
        {
            // Arrange
            var cameraId = Guid.NewGuid();
            var scheduledJobs = new List<ScheduledJobInfo>
            {
                new ScheduledJobInfo
                {
                    JobId = "job1",
                    ScheduledExecutionTime = DateTimeOffset.Now.AddHours(1),
                    JobType = ScheduledJobType.ClearOverlay,
                    CameraId = cameraId
                },
                new ScheduledJobInfo
                {
                    JobId = "job2",
                    ScheduledExecutionTime = DateTimeOffset.Now.AddHours(2),
                    JobType = ScheduledJobType.SunriseSunset,
                    CameraId = cameraId,
                    SunriseSunsetType = SunriseSunset.Sunrise
                }
            };

            _cameraScheduler.GetAllScheduledJobs().Returns(scheduledJobs);

            var query = new GetScheduledJobsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ScheduledJobs, Has.Count.EqualTo(2));
            
            var firstJob = result.ScheduledJobs.First(j => j.JobId == "job1");
            Assert.That(firstJob.JobType, Is.EqualTo(ScheduledJobType.ClearOverlay));
            Assert.That(firstJob.CameraId, Is.EqualTo(cameraId));
            Assert.That(firstJob.SunriseSunsetType, Is.Null);


            var secondJob = result.ScheduledJobs.First(j => j.JobId == "job2");
            Assert.That(secondJob.JobType, Is.EqualTo(ScheduledJobType.SunriseSunset));
            Assert.That(secondJob.CameraId, Is.EqualTo(cameraId));
            Assert.That(secondJob.SunriseSunsetType, Is.EqualTo(SunriseSunset.Sunrise));

        }

        [Test]
        public async Task Handle_WithNoScheduledJobs_ReturnsEmptyResponse()
        {
            // Arrange
            _cameraScheduler.GetAllScheduledJobs().Returns(new List<ScheduledJobInfo>());

            var query = new GetScheduledJobsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ScheduledJobs, Is.Not.Null);
            Assert.That(result.ScheduledJobs, Is.Empty);
        }
    }
}