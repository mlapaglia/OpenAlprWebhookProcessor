using MediatR;
using OpenAlprWebhookProcessor.CameraUpdateService;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.TriggerAutofocus
{
    public class TriggerAutofocusCommandHandler : IRequestHandler<TriggerAutofocusCommand, bool>
    {
        private readonly ICameraUpdateService _cameraUpdateService;

        public TriggerAutofocusCommandHandler(ICameraUpdateService cameraUpdateService)
        {
            _cameraUpdateService = cameraUpdateService;
        }

        public async Task<bool> Handle(TriggerAutofocusCommand request, CancellationToken cancellationToken = default)
        {
            return await _cameraUpdateService.TriggerAutofocusAsync(
                request.CameraId,
                cancellationToken);
        }
    }
} 