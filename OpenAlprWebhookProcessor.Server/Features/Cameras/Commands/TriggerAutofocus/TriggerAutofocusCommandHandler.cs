using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.TriggerAutofocus
{
    public class TriggerAutofocusCommandHandler : IRequestHandler<TriggerAutofocusCommand, bool>
    {
        private readonly CameraUpdateService.CameraUpdateService _cameraUpdateService;

        public TriggerAutofocusCommandHandler(CameraUpdateService.CameraUpdateService cameraUpdateService)
        {
            _cameraUpdateService = cameraUpdateService;
        }

        public async Task<bool> Handle(TriggerAutofocusCommand request, CancellationToken cancellationToken)
        {
            return await _cameraUpdateService.TriggerAutofocusAsync(
                request.CameraId,
                cancellationToken);
        }
    }
} 