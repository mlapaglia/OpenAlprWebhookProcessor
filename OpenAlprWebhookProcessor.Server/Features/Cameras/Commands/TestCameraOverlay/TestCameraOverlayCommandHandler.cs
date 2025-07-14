using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.TestCameraOverlay
{
    public class TestCameraOverlayCommandHandler : IRequestHandler<TestCameraOverlayCommand>
    {
        private readonly CameraUpdateService.CameraUpdateService _cameraUpdateService;

        public TestCameraOverlayCommandHandler(CameraUpdateService.CameraUpdateService cameraUpdateService)
        {
            _cameraUpdateService = cameraUpdateService;
        }

        public Task Handle(TestCameraOverlayCommand request, CancellationToken cancellationToken)
        {
            _cameraUpdateService.ScheduleOverlayRequest(new CameraUpdateService.CameraUpdateRequest()
            {
                Id = request.CameraId,
                IsTest = true,
                LicensePlate = "test",
                AlertDescription = "test",
                OpenAlprProcessingTimeMs = 1000,
                ProcessedPlateConfidence = 100,
                VehicleDescription = "test vehicle"
            });

            return Task.CompletedTask;
        }
    }
} 