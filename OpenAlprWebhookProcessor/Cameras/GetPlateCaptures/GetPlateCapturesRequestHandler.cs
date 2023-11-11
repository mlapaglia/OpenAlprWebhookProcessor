using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Cameras.GetPlateCaptures
{
    public class GetPlateCapturesHandler
    {
        private readonly ProcessorContext _processorContext;

        public GetPlateCapturesHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task<List<string>> HandleAsync(
            Guid cameraId,
            CancellationToken cancellationToken)
        {
            var openAlprCameraId = await _processorContext.Cameras
                .AsNoTracking()
                .Where(x => x.Id == cameraId)
                .Select(x => x.OpenAlprCameraId)
                .FirstOrDefaultAsync(cancellationToken);

            var capturedPlates = await _processorContext.PlateGroups
                .AsNoTracking()
                .Where(x => x.OpenAlprCameraId == openAlprCameraId)
                .Where(x => x.PlateImage != null)
                .Select(x => x.OpenAlprUuid)
                .Take(10)
                .ToListAsync(cancellationToken);

            return capturedPlates.Select(x => Flurl.Url.Combine($"/images/{x}")).ToList();
        }
    }
}