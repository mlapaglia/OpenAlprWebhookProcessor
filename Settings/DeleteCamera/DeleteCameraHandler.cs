using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
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

        public async Task HandleAsync(long cameraId)
        {
            var camera = await _processorContext.Cameras.FirstOrDefaultAsync(x => x.OpenAlprCameraId == cameraId);

            _processorContext.Remove(camera);
            await _processorContext.SaveChangesAsync();
        }
    }
}
