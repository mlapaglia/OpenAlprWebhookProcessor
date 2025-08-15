using Mediator;
using OpenAlprWebhookProcessor.CameraUpdateService;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.TriggerAutofocus
{
    public class TriggerAutofocusCommandHandler : IQueryHandler<TriggerAutofocusCommand, bool>
    {
        private readonly ISimpleCameraScheduler _simpleCameraScheduler;

        public TriggerAutofocusCommandHandler(ISimpleCameraScheduler simpleCameraScheduler)
        {
            _simpleCameraScheduler = simpleCameraScheduler;
        }

        public async ValueTask<bool> Handle(TriggerAutofocusCommand request, CancellationToken cancellationToken = default)
        {
            return await _simpleCameraScheduler.TriggerAutofocusAsync(
                request.CameraId,
                cancellationToken);
        }
    }
} 