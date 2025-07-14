using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.DeleteCamera
{
    public class DeleteCameraCommandHandler : IRequestHandler<DeleteCameraCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CameraUpdateService.CameraUpdateService _cameraUpdateService;

        public DeleteCameraCommandHandler(
            IUnitOfWork unitOfWork,
            CameraUpdateService.CameraUpdateService cameraUpdateService)
        {
            _unitOfWork = unitOfWork;
            _cameraUpdateService = cameraUpdateService;
        }

        public async Task Handle(DeleteCameraCommand request, CancellationToken cancellationToken)
        {
            var camera = await _unitOfWork.Cameras.FirstOrDefaultAsync(x => x.Id == request.CameraId, cancellationToken);

            if (camera != null)
            {
                await _cameraUpdateService.DeleteSunriseSunsetAsync(camera.Id);

                _unitOfWork.Cameras.Delete(camera);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
} 