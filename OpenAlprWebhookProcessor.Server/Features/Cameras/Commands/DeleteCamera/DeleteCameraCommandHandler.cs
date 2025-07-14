using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.DeleteCamera
{
    public class DeleteCameraCommandHandler : IRequestHandler<DeleteCameraCommand>
    {
        private readonly ProcessorContext _processorContext;
        private readonly CameraUpdateService.CameraUpdateService _cameraUpdateService;

        public DeleteCameraCommandHandler(
            ProcessorContext processorContext,
            CameraUpdateService.CameraUpdateService cameraUpdateService)
        {
            _processorContext = processorContext;
            _cameraUpdateService = cameraUpdateService;
        }

        public async Task Handle(DeleteCameraCommand request, CancellationToken cancellationToken)
        {
            var camera = await _processorContext.Cameras.FirstOrDefaultAsync(x => x.Id == request.CameraId, cancellationToken);

            await _cameraUpdateService.DeleteSunriseSunsetAsync(camera.Id);

            _processorContext.Remove(camera);
            await _processorContext.SaveChangesAsync(cancellationToken);
        }
    }
} 