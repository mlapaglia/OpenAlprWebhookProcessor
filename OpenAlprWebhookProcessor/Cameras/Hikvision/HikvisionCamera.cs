using OpenAlprWebhookProcessor.Cameras.Hikvision;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Collections.Generic;
using OpenAlprWebhookProcessor.Cameras.Configuration;
using System.Linq;

namespace OpenAlprWebhookProcessor.Cameras
{
    public class HikvisionCamera : ICamera
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly ILogger _logger;

        private readonly CameraConfiguration _configuration;

        public HikvisionCamera(
            IHttpClientFactory httpClientFactory,
            ILogger<HikvisionCamera> logger,
            CameraConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task ClearCameraTextAsync(
            int openAlprCameraId,
            CancellationToken cancellationToken)
        {
            var cameraToUpdate = _configuration.HikvisionCameras.First(x => x.OpenAlprCameraId == openAlprCameraId);

            var videoOverlayRequest = CreateBaseVideoOverlayRequest();

            videoOverlayRequest.TextOverlayList.TextOverlay.Add(
                new TextOverlay()
                {
                    Id = "1",
                    Enabled = "false",
                    DisplayText = string.Empty,
                });

            await PushCameraTextAsync(
                cameraToUpdate,
                videoOverlayRequest,
                cancellationToken);
        }

        public async Task SetCameraTextAsync(
            int openAlprCameraId,
            string plateNumber,
            string vehicleDescription,
            CancellationToken cancellationToken)
        {
            var cameraToUpdate = _configuration.HikvisionCameras.First(x => x.OpenAlprCameraId == openAlprCameraId);

            var videoOverlayRequest = CreateBaseVideoOverlayRequest();

            videoOverlayRequest.TextOverlayList.TextOverlay.Add(
                new TextOverlay()
                {
                    Id = "1",
                    Enabled = "true",
                    DisplayText = plateNumber,
                });

            videoOverlayRequest.TextOverlayList.TextOverlay.Add(
                new TextOverlay()
                {
                    Id = "2",
                    Enabled = "true",
                    DisplayText = vehicleDescription,
                });

            await PushCameraTextAsync(
                cameraToUpdate,
                videoOverlayRequest,
                cancellationToken);
        }

        private async Task PushCameraTextAsync(
            HikvisionCameraConfiguration cameraToUpdate,
            VideoOverlay videoOverlay,
            CancellationToken cancellationToken)
        {
            using (var stringWriter = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(stringWriter))
                {
                    var serializer = new XmlSerializer(typeof(VideoOverlay));
                    serializer.Serialize(
                        writer,
                        videoOverlay);

                    var content = new StringContent(stringWriter.ToString());
                    _logger.LogInformation(stringWriter.ToString());

                    var client = _httpClientFactory.CreateClient(nameof(HikvisionCamera));

                    var response = await client.PutAsync(
                        cameraToUpdate.UpdateTextUrl,
                        content,
                        cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new ArgumentException("unable to update video overlay: " + await response.Content.ReadAsStringAsync());
                    }
                }
            }
        }

        private static VideoOverlay CreateBaseVideoOverlayRequest()
        {
            return new VideoOverlay()
            {
                Alignment = "customize",
                TextOverlayList = new TextOverlayList()
                {
                    TextOverlay = new List<TextOverlay>(),
                },
            };
        }
    }
}
