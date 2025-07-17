using MediatR;
using OpenAlprWebhookProcessor.CameraUpdateService;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.TestCameraNightMode
{
    public class TestCameraNightModeCommandHandler : IRequestHandler<TestCameraNightModeCommand>
    {
        private readonly ICameraUpdateService _cameraUpdateService;

        public TestCameraNightModeCommandHandler(ICameraUpdateService cameraUpdateService)
        {
            _cameraUpdateService = cameraUpdateService;
        }

        public Task Handle(TestCameraNightModeCommand request, CancellationToken cancellationToken)
        {
            _cameraUpdateService.EnqueueDayNight(
                request.CameraId,
                SunriseSunset.Sunset);

            return Task.CompletedTask;
        }
    }
} 