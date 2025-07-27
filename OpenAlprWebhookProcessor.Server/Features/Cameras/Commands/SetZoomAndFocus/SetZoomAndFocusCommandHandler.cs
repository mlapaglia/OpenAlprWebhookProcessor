using MediatR;
using OpenAlprWebhookProcessor.CameraUpdateService;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.SetZoomAndFocus
{
    public class SetZoomAndFocusCommandHandler : IRequestHandler<SetZoomAndFocusCommand>
    {
        private readonly ICameraUpdateService _cameraUpdateService;

        public SetZoomAndFocusCommandHandler(ICameraUpdateService cameraUpdateService)
        {
            _cameraUpdateService = cameraUpdateService;
        }

        public async Task Handle(SetZoomAndFocusCommand request, CancellationToken cancellationToken = default)
        {
            await _cameraUpdateService.SetZoomAndFocusAsync(
                request.CameraId,
                request.ZoomAndFocus,
                cancellationToken);
        }
    }
} 