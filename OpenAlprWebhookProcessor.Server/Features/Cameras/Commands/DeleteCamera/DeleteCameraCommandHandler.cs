using MediatR;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Commands.DeleteCamera
{
    public class DeleteCameraCommandHandler : IRequestHandler<DeleteCameraCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICameraUpdateService _cameraUpdateService;

        public DeleteCameraCommandHandler(
            IUnitOfWork unitOfWork,
            ICameraUpdateService cameraUpdateService)
        {
            _unitOfWork = unitOfWork;
            _cameraUpdateService = cameraUpdateService;
        }

        public async Task Handle(DeleteCameraCommand request, CancellationToken cancellationToken = default)
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