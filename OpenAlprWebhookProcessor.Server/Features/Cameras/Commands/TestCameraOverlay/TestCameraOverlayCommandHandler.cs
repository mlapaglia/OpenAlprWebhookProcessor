using Mediator;
using OpenAlprWebhookProcessor.CameraUpdateService;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.TestCameraOverlay
{
    public class TestCameraOverlayCommandHandler : ICommandHandler<TestCameraOverlayCommand>
    {
        private readonly ICameraUpdateService _cameraUpdateService;

        public TestCameraOverlayCommandHandler(ICameraUpdateService cameraUpdateService)
        {
            _cameraUpdateService = cameraUpdateService;
        }

        public async ValueTask<Unit> Handle(TestCameraOverlayCommand request, CancellationToken cancellationToken = default)
        {
            await _cameraUpdateService.ScheduleOverlayRequestAsync(new CameraUpdateService.CameraUpdateRequest()
            {
                Id = request.CameraId,
                IsTest = true,
                LicensePlate = "test",
                AlertDescription = "test",
                OpenAlprProcessingTimeMs = 1000,
                ProcessedPlateConfidence = 100,
                VehicleDescription = "test vehicle"
            });

            return Unit.Value;
        }
    }
} 