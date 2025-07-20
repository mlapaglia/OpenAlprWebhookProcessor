using OpenAlprWebhookProcessor.Cameras;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.CameraUpdateService.Hikvision;
using OpenAlprWebhookProcessor.Features.Cameras.Configuration;
using System;
using System.Net.Http;

namespace OpenAlprWebhookProcessor.Features.Cameras
{
    public class CameraFactory : ICameraFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CameraFactory(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public ICamera Create(
            CameraManufacturer cameraManufacturer,
            Data.Camera camera)
        {
            return cameraManufacturer switch
            {
                CameraManufacturer.Dahua => new DahuaCamera(camera, _httpClientFactory),
                CameraManufacturer.Hikvision => new HikvisionCamera(camera, _httpClientFactory),
                _ => throw new ArgumentException("unknown camera manufacturer"),
            };
        }
    }
}
