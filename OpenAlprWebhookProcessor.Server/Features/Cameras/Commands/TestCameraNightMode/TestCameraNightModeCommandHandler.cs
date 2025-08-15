using Mediator;
using OpenAlprWebhookProcessor.CameraUpdateService;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.TestCameraNightMode
{
    public class TestCameraNightModeCommandHandler : ICommandHandler<TestCameraNightModeCommand>
    {
        private readonly ISimpleCameraScheduler _simpleCameraScheduler;

        public TestCameraNightModeCommandHandler(ISimpleCameraScheduler simpleCameraScheduler)
        {
            _simpleCameraScheduler = simpleCameraScheduler;
        }

        public async ValueTask<Unit> Handle(TestCameraNightModeCommand request, CancellationToken cancellationToken = default)
        {
            await _simpleCameraScheduler.ExecuteDayNightModeAsync(
                request.CameraId,
                SunriseSunset.Sunset,
                cancellationToken);

            return Unit.Value;
        }
    }
} 