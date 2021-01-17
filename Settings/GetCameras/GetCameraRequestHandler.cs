using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Settings.GetCameras
{
    public class GetCameraRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        public GetCameraRequestHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task<List<Camera>> HandleAsync()
        {
            var cameras = new List<Camera>();

            foreach(var camera in await _processorContext.Cameras.ToListAsync())
            {
                cameras.Add(new Camera()
                {
                    CameraPassword = camera.CameraPassword,
                    CameraUsername = camera.CameraUsername,
                    Latitude = camera.Latitude,
                    Longitude = camera.Longitude,
                    Manufacturer = camera.Manufacturer,
                    ModelNumber = camera.ModelNumber,
                    OpenAlprCameraId = camera.OpenAlprCameraId,
                    OpenAlprName = camera.OpenAlprName,
                    PlatesSeen = camera.PlatesSeen,
                    UpdateOverlayTextUrl = camera.UpdateOverlayTextUrl,
                    SampleImageUrl = CreateSampleImageUrl(camera.LatestProcessedPlateUuid, camera.UpdateOverlayTextUrl),
                });
            }

            return cameras;
        }

        private static string CreateSampleImageUrl(
            string imageUuid,
            string cameraIpAddress)
        {
            if (!string.IsNullOrEmpty(imageUuid) || !string.IsNullOrEmpty(cameraIpAddress))
            {
                return null;
            }

            return Flurl.Url.Combine(cameraIpAddress, imageUuid);
        }
    }
}
