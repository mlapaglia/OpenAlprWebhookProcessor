using Mediator;
using OpenAlprWebhookProcessor.CameraUpdateService;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.SetZoomAndFocus
{
    public class SetZoomAndFocusCommandHandler : ICommandHandler<SetZoomAndFocusCommand>
    {
        private readonly ISimpleCameraScheduler _simpleCameraScheduler;

        public SetZoomAndFocusCommandHandler(ISimpleCameraScheduler simpleCameraScheduler)
        {
            _simpleCameraScheduler = simpleCameraScheduler;
        }

        public async ValueTask<Unit> Handle(SetZoomAndFocusCommand request, CancellationToken cancellationToken)
        {
            await _simpleCameraScheduler.SetZoomAndFocusAsync(
                request.CameraId,
                request.ZoomAndFocus,
                cancellationToken);
            return Unit.Value;
        }
    }
} 