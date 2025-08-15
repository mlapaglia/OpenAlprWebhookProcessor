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

        private readonly ISimpleCameraScheduler _simpleCameraScheduler;

        public DeleteCameraCommandHandler(
            IUnitOfWork unitOfWork, 
            ISimpleCameraScheduler simpleCameraScheduler)
        {
            _unitOfWork = unitOfWork;
            _simpleCameraScheduler = simpleCameraScheduler;
        }

        public async ValueTask<Unit> Handle(DeleteCameraCommand command, CancellationToken cancellationToken = default)
        {
            var camera = await _unitOfWork.Cameras.FirstOrDefaultAsync(x => x.Id == command.CameraId, cancellationToken);

            if (camera != null)
            {
                await _simpleCameraScheduler.RemoveCameraScheduleAsync(camera.Id, cancellationToken);

                _unitOfWork.Cameras.Delete(camera);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return Unit.Value;
        }
    }
}