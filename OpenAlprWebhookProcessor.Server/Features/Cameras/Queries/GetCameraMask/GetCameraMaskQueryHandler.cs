using Mediator;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Queries.GetCameraMask
{
    public class GetCameraMaskQueryHandler : IQueryHandler<GetCameraMaskQuery, List<MaskCoordinate>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCameraMaskQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<List<MaskCoordinate>> Handle(GetCameraMaskQuery query, CancellationToken cancellationToken = default)
        {
            var cameraMask = await _unitOfWork.CameraMasks.FirstOrDefaultAsync(x => x.CameraId == query.CameraId, cancellationToken);

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