using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Queries.GetPlateCaptures
{
    public class GetPlateCapturesQueryHandler : IRequestHandler<GetPlateCapturesQuery, List<string>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetPlateCapturesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<string>> Handle(GetPlateCapturesQuery request, CancellationToken cancellationToken)
        {
            var camera = await _unitOfWork.Cameras.FirstOrDefaultAsync(x => x.Id == request.CameraId, cancellationToken);
            
            if (camera == null)
            {
                return new List<string>();
            }

            var capturedPlates = await _unitOfWork.PlateGroups.GetQueryable()
                .AsNoTracking()
                .Where(x => x.OpenAlprCameraId == camera.OpenAlprCameraId)
                .Where(x => x.PlateImage != null)
                .OrderByDescending(x => x.ReceivedOnEpoch)
                .Select(x => x.OpenAlprUuid)
                .Take(10)
                .ToListAsync(cancellationToken);

            return capturedPlates.Select(x => Flurl.Url.Combine($"/api/images/{x}")).ToList();
        }
    }
} 