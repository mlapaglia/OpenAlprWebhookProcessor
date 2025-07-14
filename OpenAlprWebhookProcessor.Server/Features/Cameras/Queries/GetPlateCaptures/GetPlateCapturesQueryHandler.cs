using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Queries.GetPlateCaptures
{
    public class GetPlateCapturesQueryHandler : IRequestHandler<GetPlateCapturesQuery, List<string>>
    {
        private readonly ProcessorContext _processorContext;

        public GetPlateCapturesQueryHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task<List<string>> Handle(GetPlateCapturesQuery request, CancellationToken cancellationToken)
        {
            var openAlprCameraId = await _processorContext.Cameras
                .AsNoTracking()
                .Where(x => x.Id == request.CameraId)
                .Select(x => x.OpenAlprCameraId)
                .FirstOrDefaultAsync(cancellationToken);

            var capturedPlates = await _processorContext.PlateGroups
                .AsNoTracking()
                .Where(x => x.OpenAlprCameraId == openAlprCameraId)
                .Where(x => x.PlateImage != null)
                .OrderByDescending(x => x.ReceivedOnEpoch)
                .Select(x => x.OpenAlprUuid)
                .Take(10)
                .ToListAsync(cancellationToken);

            return capturedPlates.Select(x => Flurl.Url.Combine($"/api/images/{x}")).ToList();
        }
    }
} 