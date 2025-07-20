using MediatR;
using OpenAlprWebhookProcessor.CameraUpdateService;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.TestCameraDayMode
{
    public class TestCameraDayModeCommandHandler : IRequestHandler<TestCameraDayModeCommand>
    {
        private readonly ICameraUpdateService _cameraUpdateService;

        public TestCameraDayModeCommandHandler(ICameraUpdateService cameraUpdateService)
        {
            _cameraUpdateService = cameraUpdateService;
        }

        public Task Handle(TestCameraDayModeCommand request, CancellationToken cancellationToken)
        {
            _cameraUpdateService.EnqueueDayNightAsync(
                request.CameraId,
                SunriseSunset.Sunrise);

            return Task.CompletedTask;
        }
    }
} 