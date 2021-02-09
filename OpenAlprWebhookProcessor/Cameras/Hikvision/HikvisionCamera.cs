using OpenAlprWebhookProcessor.Cameras.Hikvision;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Net;
using OpenAlprWebhookProcessor.CameraUpdateService;
using OpenAlprWebhookProcessor.Data;

namespace OpenAlprWebhookProcessor.Cameras
{
    public class HikvisionCamera : ICamera
    {
        private readonly Data.Camera _camera;

        private readonly HttpClient _httpClient;

        public HikvisionCamera(Data.Camera camera)
        {
            _camera = camera;
            _httpClient = GetHttpClient();
        }

        public async Task ClearCameraTextAsync(
            CancellationToken cancellationToken)
        {
            var videoOverlayRequest = CreateBaseVideoOverlayRequest();

            videoOverlayRequest.TextOverlayList.TextOverlay.Add(
                new TextOverlay()
                {
                    Id = "1",
                    Enabled = "false",
                    DisplayText = string.Empty,
                });

            videoOverlayRequest.TextOverlayList.TextOverlay.Add(
                new TextOverlay()
                {
                    Id = "2",
                    Enabled = "false",
                    DisplayText = string.Empty,
                });

            videoOverlayRequest.TextOverlayList.TextOverlay.Add(
                new TextOverlay()
                {
                    Id = "3",
                    Enabled = "false",
                    DisplayText = string.Empty,
                });

            videoOverlayRequest.TextOverlayList.TextOverlay.Add(
                new TextOverlay()
                {
                    Id = "4",
                    Enabled = "false",
                    DisplayText = string.Empty,
                });

            await PushCameraTextAsync(
                videoOverlayRequest,
                cancellationToken);
        }

        public async Task SetCameraTextAsync(
            CameraUpdateRequest updateRequest,
            CancellationToken cancellationToken)
        {
            var videoOverlayRequest = CreateBaseVideoOverlayRequest();

            videoOverlayRequest.TextOverlayList.TextOverlay.Add(
                new TextOverlay()
                {
                    Id = "1",
                    Enabled = "true",
                    DisplayText = updateRequest.LicensePlate,
                });

            videoOverlayRequest.TextOverlayList.TextOverlay.Add(
                new TextOverlay()
                {
                    Id = "2",
                    Enabled = "true",
                    DisplayText = updateRequest.VehicleDescription,
                });

            videoOverlayRequest.TextOverlayList.TextOverlay.Add(
                new TextOverlay()
                {
                    Id = "3",
                    Enabled = "true",
                    DisplayText = $"Processing Time: {updateRequest.OpenAlprProcessingTimeMs}ms",
                });

            videoOverlayRequest.TextOverlayList.TextOverlay.Add(
                new TextOverlay()
                {
                    Id = "4",
                    Enabled = "true",
                    DisplayText = $"Confidence: {updateRequest.ProcessedPlateConfidence}%",
                });

            await PushCameraTextAsync(
                videoOverlayRequest,
                cancellationToken);
        }

        public async Task TriggerDayNightModeAsync(
            SunriseSunset sunriseSunset,
            CancellationToken cancellationToken)
        {
            var body = new StringContent($"<ImageChannel version=\"2.0\" xmlns=\"http://www.hikvision.com/ver20/XMLSchema\"><IrcutFilter version=\"2.0\" xmlns=\"http://www.hikvision.com/ver20/XMLSchema\"><IrcutFilterType>{(sunriseSunset == SunriseSunset.Sunrise ? "day" : "night")}</IrcutFilterType></IrcutFilter></ImageChannel>");
            
            var response = await _httpClient.PostAsync(
                $"http://{_camera.UpdateDayNightModeUrl}/ISAPI/Image/channels/1",
                body,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new ArgumentException("unable to set sunrise/sunset: " + await response.Content.ReadAsStringAsync(cancellationToken));
            }
        }

        private async Task PushCameraTextAsync(
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

                    var response = await _httpClient.PutAsync(
                        _camera.UpdateOverlayTextUrl,
                        new StringContent(stringWriter.ToString()),
                        cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new ArgumentException("unable to update video overlay: " + await response.Content.ReadAsStringAsync(cancellationToken));
                    }
                }
            }
        }

        private HttpClient GetHttpClient()
        {
            return new HttpClient(new HttpClientHandler()
            {
                UseDefaultCredentials = true,
                Credentials = new NetworkCredential(
                    _camera.CameraUsername,
                    _camera.CameraPassword),
            });
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

        public async Task<Stream> GetSnapshotAsync(CancellationToken cancellationToken)
        {
            var result = await _httpClient.GetAsync(
                $"http://{_camera.IpAddress}/ISAPI/Streaming/channels/1/picture",
                cancellationToken);

            return await result.Content.ReadAsStreamAsync(cancellationToken);
        }
    }
}
