using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Settings.DeleteCamera
{
    public class DeleteCameraHandler
    {
        private readonly ProcessorContext _processorContext;

        public DeleteCameraHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task HandleAsync(Guid cameraId)
        {
            var camera = await _processorContext.Cameras.FirstOrDefaultAsync(x => x.Id == cameraId);

            _processorContext.Remove(camera);
            await _processorContext.SaveChangesAsync();
        }
    }
}
