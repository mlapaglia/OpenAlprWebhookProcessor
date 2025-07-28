using Mediator;
using OpenAlprWebhookProcessor.CameraUpdateService;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.TriggerAutofocus
{
    public class TriggerAutofocusCommandHandler : IQueryHandler<TriggerAutofocusCommand, bool>
    {
        private readonly ICameraUpdateService _cameraUpdateService;

        public TriggerAutofocusCommandHandler(ICameraUpdateService cameraUpdateService)
        {
            _cameraUpdateService = cameraUpdateService;
        }

        public async ValueTask<bool> Handle(TriggerAutofocusCommand request, CancellationToken cancellationToken = default)
        {
            return await _cameraUpdateService.TriggerAutofocusAsync(
                request.CameraId,
                cancellationToken);
        }
    }
} 