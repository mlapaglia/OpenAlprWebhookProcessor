using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Queries.GetCameraMask
{
    public class GetCameraMaskQueryHandler : IRequestHandler<GetCameraMaskQuery, List<MaskCoordinate>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCameraMaskQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<MaskCoordinate>> Handle(GetCameraMaskQuery request, CancellationToken cancellationToken)
        {
            var cameraMask = await _unitOfWork.CameraMasks.FirstOrDefaultAsync(x => x.CameraId == request.CameraId, cancellationToken);

            if (cameraMask?.Coordinates == null)
            {
                return new List<MaskCoordinate>();
            }
            else
            {
                return JsonSerializer.Deserialize<List<MaskCoordinate>>(cameraMask.Coordinates);
            }
        }
    }
} 