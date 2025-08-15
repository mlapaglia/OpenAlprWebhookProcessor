using Mediator;
using OpenAlprWebhookProcessor.CameraUpdateService;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.TestCameraOverlay
{
    public class TestCameraOverlayCommandHandler : ICommandHandler<TestCameraOverlayCommand>
    {
        private readonly ISimpleCameraScheduler _simpleCameraScheduler;

        public TestCameraOverlayCommandHandler(ISimpleCameraScheduler simpleCameraScheduler)
        {
            _simpleCameraScheduler = simpleCameraScheduler;
        }

        public async ValueTask<Unit> Handle(TestCameraOverlayCommand request, CancellationToken cancellationToken = default)
        {
            await _simpleCameraScheduler.ScheduleOverlayAsync(new CameraUpdateRequest()
            {
                Id = request.CameraId,
                IsTest = true,
                LicensePlate = "test",
                AlertDescription = "test",
                OpenAlprProcessingTimeMs = 1000,
                ProcessedPlateConfidence = 100,
                VehicleDescription = "test vehicle"
            }, cancellationToken);

            return Unit.Value;
        }
    }
} 