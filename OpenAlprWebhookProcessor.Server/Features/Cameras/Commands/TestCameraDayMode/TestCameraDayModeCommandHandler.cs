using Mediator;
using OpenAlprWebhookProcessor.CameraUpdateService;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.TestCameraDayMode
{
    public class TestCameraDayModeCommandHandler : ICommandHandler<TestCameraDayModeCommand>
    {
        private readonly ICameraUpdateService _cameraUpdateService;

        public TestCameraDayModeCommandHandler(ICameraUpdateService cameraUpdateService)
        {
            _cameraUpdateService = cameraUpdateService;
        }

        public async ValueTask<Unit> Handle(TestCameraDayModeCommand request, CancellationToken cancellationToken = default)
        {
            await _cameraUpdateService.EnqueueDayNightAsync(
                request.CameraId,
                SunriseSunset.Sunrise);

            return Unit.Value;
        }
    }
} 