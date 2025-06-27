using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Server.Data;
using System;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Server.Cameras.DeleteCamera
{
    public class DeleteCameraHandler
    {
        private readonly ProcessorContext _processorContext;

        private readonly CameraUpdateService.CameraUpdateService _cameraUpdateService;

        public DeleteCameraHandler(
            ProcessorContext processorContext,
            CameraUpdateService.CameraUpdateService cameraUpdateService)
        {
            _processorContext = processorContext;
            _cameraUpdateService = cameraUpdateService;
        }

        public async Task HandleAsync(Guid cameraId)
        {
            var camera = await _processorContext.Cameras.FirstOrDefaultAsync(x => x.Id == cameraId);

            await _cameraUpdateService.DeleteSunriseSunsetAsync(camera.Id);

            _processorContext.Remove(camera);
            await _processorContext.SaveChangesAsync();
        }
    }
}
