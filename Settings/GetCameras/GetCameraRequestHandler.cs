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

            cameras.Add(new Camera()
            {
                CameraPassword = "asdf",
                CameraUsername = "asdf",
                IpAddress = "192.168.1.164",
                Latitude = 1004.34,
                Longitude = 19854.234,
                Manufacturer = Cameras.Configuration.CameraManufacturer.Hikvision,
                ModelNumber = "DS-2CD2335FWD-I",
                OpenAlprCameraId = 123,
                OpenAlprName = "mailbox-west",
                PlatesSeen = 1234,
                SampleImageUrl = new Uri("http://192.168.1.164:4382/img/U8HGWA66CW58AV9Q7YB4JJ749CV7WXNQLLNIBT4N-106232742-1610821103808.jpg"),
                UpdateOverlayTextUrl = new Uri("https://google.com"),
            });


            foreach(var camera in await _processorContext.Cameras.ToListAsync())
            {
                cameras.Add(new Camera()
                {
                    CameraPassword = camera.CameraPassword,
                    CameraUsername = camera.CameraUsername,
                    Latitude = camera.Latitude,
                    Longitude = camera.Longitude,
                    Manufacturer = camera.Manufacturer,
                    OpenAlprCameraId = camera.OpenAlprCameraId,
                    OpenAlprName = camera.OpenAlprName,
                    UpdateOverlayTextUrl = camera.UpdateOverlayTextUrl != null ? new Uri(camera.UpdateOverlayTextUrl) : null,
                });
            }

            return cameras;
        }
    }
}
