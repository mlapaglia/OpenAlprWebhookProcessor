using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Cameras;
using OpenAlprWebhookProcessor.Data;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.ImageRelay
{
    public class GetSnapshotHandler
    {
        private readonly ProcessorContext _processorContext;

        public GetSnapshotHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task<Stream> GetSnapshotAsync(
            Guid cameraId,
            CancellationToken cancellationToken)
        {
            var dbCamera = await _processorContext.Cameras.FirstOrDefaultAsync(x =>
                x.Id == cameraId,
                cancellationToken);

            var camera = CameraFactory.Create(dbCamera.Manufacturer, dbCamera);

            return await camera.GetSnapshotAsync(cancellationToken);
        }
    }
}
