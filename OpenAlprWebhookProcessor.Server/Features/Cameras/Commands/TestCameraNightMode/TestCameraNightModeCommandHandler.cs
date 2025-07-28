using Mediator;
using OpenAlprWebhookProcessor.CameraUpdateService;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.TestCameraNightMode
{
    public class TestCameraNightModeCommandHandler : ICommandHandler<TestCameraNightModeCommand>
    {
        private readonly ICameraUpdateService _cameraUpdateService;

        public TestCameraNightModeCommandHandler(ICameraUpdateService cameraUpdateService)
        {
            _cameraUpdateService = cameraUpdateService;
        }

        public async ValueTask<Unit> Handle(TestCameraNightModeCommand request, CancellationToken cancellationToken = default)
        {
            await _cameraUpdateService.EnqueueDayNightAsync(
                request.CameraId,
                SunriseSunset.Sunset);

            return Unit.Value;
        }
    }
} 