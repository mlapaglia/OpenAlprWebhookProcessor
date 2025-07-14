using MediatR;
using OpenAlprWebhookProcessor.CameraUpdateService;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.TestCameraDayMode
{
    public class TestCameraDayModeCommandHandler : IRequestHandler<TestCameraDayModeCommand>
    {
        private readonly CameraUpdateService.CameraUpdateService _cameraUpdateService;

        public TestCameraDayModeCommandHandler(CameraUpdateService.CameraUpdateService cameraUpdateService)
        {
            _cameraUpdateService = cameraUpdateService;
        }

        public Task Handle(TestCameraDayModeCommand request, CancellationToken cancellationToken)
        {
            _cameraUpdateService.EnqueueDayNight(
                request.CameraId,
                SunriseSunset.Sunrise);

            return Task.CompletedTask;
        }
    }
} 