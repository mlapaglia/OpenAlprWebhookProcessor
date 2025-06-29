using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using NSubstitute;
using OpenAlprWebhookProcessor.Server.CameraUpdateService;

namespace Tests.CameraUpdateService
{
    [TestFixture]
    public class CameraSchedulingTests
    {
        private ICameraUpdateService _mockCameraUpdateService;

        private IBackgroundJobClient _mockBackgroundJobClient;

        [SetUp]
        public void SetUp()
        {
            _mockCameraUpdateService = Substitute.For<ICameraUpdateService>();
            _mockBackgroundJobClient = Substitute.For<IBackgroundJobClient>();
        }

        [Test]
        public void ExecuteSingleDayNightTask_ShouldEnqueueJob()
        {
            var cameraId = Guid.NewGuid();
            CameraScheduling.ExecuteSingleDayNightTask(
                SunriseSunset.Sunrise,
                cameraId,
                _mockCameraUpdateService,
                _mockBackgroundJobClient);

            _mockBackgroundJobClient.Received(1).Create(
                Arg.Is<Job>(x => x.Method.Name == "ProcessSunriseSunsetJobAsync"), Arg.Any<EnqueuedState>());
        }
    }
}
