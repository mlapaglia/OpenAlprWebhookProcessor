using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Server.Cameras;
using OpenAlprWebhookProcessor.Server.Data;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Server.ImageRelay.SnapshotRelay
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
            var dbCamera = await _processorContext.Cameras
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == cameraId,
                    cancellationToken);

            var camera = CameraFactory.Create(dbCamera.Manufacturer, dbCamera);

            int timeout = 5000;
            var task = camera.GetSnapshotAsync(cancellationToken);
            if (await Task.WhenAny(task, Task.Delay(timeout, cancellationToken)) == task)
            {
                return await task;

            }
            else
            {
                throw new TimeoutException("Unable to get image from camera");
            }
        }
    }
}
