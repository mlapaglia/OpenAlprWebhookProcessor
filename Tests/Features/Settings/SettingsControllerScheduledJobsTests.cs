using Mediator;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Features.Settings;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetScheduledJobs;

namespace Tests.Features.Settings
{
    [TestFixture]
    public class SettingsControllerScheduledJobsTests
    {
        private IMediator _mediator;
        private SettingsController _controller;

        [SetUp]
        public void SetUp()
        {
            _mediator = Substitute.For<IMediator>();
            _controller = new SettingsController(_mediator);
        }

        [Test]
        public async Task GetScheduledJobs_ReturnsOkResultWithScheduledJobs()
        {
            // Arrange
            var expectedResponse = new GetScheduledJobsResponse
            {
                ScheduledJobs = new List<ScheduledJobDto>
                {
                    new ScheduledJobDto
                    {
                        JobId = "job1",
                        ScheduledExecutionTime = DateTimeOffset.Now.AddHours(1),
                        JobType = ScheduledJobType.ClearOverlay,
                        CameraId = Guid.NewGuid()
                    }
                }
            };

            _mediator.Send(Arg.Any<GetScheduledJobsQuery>(), Arg.Any<CancellationToken>())
                     .Returns(expectedResponse);

            // Act
            var result = await _controller.GetScheduledJobs(CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(expectedResponse));

            await _mediator.Received(1).Send(Arg.Any<GetScheduledJobsQuery>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task GetScheduledJobs_WithEmptyResult_ReturnsOkResultWithEmptyList()
        {
            // Arrange
            var expectedResponse = new GetScheduledJobsResponse
            {
                ScheduledJobs = new List<ScheduledJobDto>()
            };

            _mediator.Send(Arg.Any<GetScheduledJobsQuery>(), Arg.Any<CancellationToken>())
                     .Returns(expectedResponse);

            // Act
            var result = await _controller.GetScheduledJobs(CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            
            var okResult = result.Result as OkObjectResult;
            var response = okResult.Value as GetScheduledJobsResponse;
            Assert.That(response.ScheduledJobs, Is.Empty);

            await _mediator.Received(1).Send(Arg.Any<GetScheduledJobsQuery>(), Arg.Any<CancellationToken>());
        }
    }
}