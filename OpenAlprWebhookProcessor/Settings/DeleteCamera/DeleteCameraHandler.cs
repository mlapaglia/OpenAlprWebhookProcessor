using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Settings.DeleteCamera
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

            _processorContext.Remove(camera);
            await _processorContext.SaveChangesAsync();

            await _cameraUpdateService.DeleteSunriseSunsetAsync(camera.Id);
        }
    }
}
