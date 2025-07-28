using Mediator;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.DeleteCamera
{
    public class DeleteCameraCommandHandler : ICommandHandler<DeleteCameraCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICameraUpdateService _cameraUpdateService;

        public DeleteCameraCommandHandler(
            IUnitOfWork unitOfWork, ICameraUpdateService cameraUpdateService)
        {
            _unitOfWork = unitOfWork;
            _cameraUpdateService = cameraUpdateService;
        }

        public async ValueTask<Unit> Handle(DeleteCameraCommand command, CancellationToken cancellationToken = default)
        {
            var camera = await _unitOfWork.Cameras.FirstOrDefaultAsync(x => x.Id == command.CameraId, cancellationToken);

            if (camera != null)
            {
                await _cameraUpdateService.DeleteSunriseSunsetAsync(camera.Id);

                _unitOfWork.Cameras.Delete(camera);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return Unit.Value;
        }
    }
}