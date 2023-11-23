using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Cameras.UpsertMasks;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Cameras
{
    public class GetCameraMaskHandler
    {
        private readonly ProcessorContext _processorContext;

        public GetCameraMaskHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task<List<MaskCoordinate>> GetCameraMaskAsync(
            Guid cameraId,
            CancellationToken cancellationToken)
        {
            var maskCoordinates = await _processorContext.CameraMasks
                .AsNoTracking()
                .Where(x => x.CameraId == cameraId)
                .Select(x => x.Coordinates)
                .FirstOrDefaultAsync(cancellationToken);

            if (maskCoordinates == null)
            {
                return new List<MaskCoordinate>();
            }
            else
            {
                return JsonSerializer.Deserialize<List<MaskCoordinate>>(maskCoordinates);
            }
        }
    }
}
