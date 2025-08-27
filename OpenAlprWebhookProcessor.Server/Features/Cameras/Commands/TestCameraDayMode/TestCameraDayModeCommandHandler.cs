using Mediator;
using OpenAlprWebhookProcessor.CameraUpdateService;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.TestCameraDayMode
{
    public class TestCameraDayModeCommandHandler : ICommandHandler<TestCameraDayModeCommand>
    {
        private readonly ISimpleCameraScheduler _simpleCameraScheduler;

        public TestCameraDayModeCommandHandler(ISimpleCameraScheduler simpleCameraScheduler)
        {
            _simpleCameraScheduler = simpleCameraScheduler;
        }

        public async ValueTask<Unit> Handle(TestCameraDayModeCommand request, CancellationToken cancellationToken)
        {
            await _simpleCameraScheduler.ExecuteDayNightModeAsync(
                request.CameraId,
                SunriseSunset.Sunrise,
                cancellationToken);

            return Unit.Value;
        }
    }
} 