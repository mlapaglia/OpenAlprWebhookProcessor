using Flurl.Http;
using Flurl.Http.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace OpenAlprWebhookProcessor.CameraUpdateService.Hikvision
{
    public class HikvisionCamera : ICamera
    {
        private readonly Data.Camera _camera;

        private readonly IFlurlClientCache _flurlClientCache;

        public HikvisionCamera(
            Data.Camera camera,
            IFlurlClientCache flurlClientCache)
        {
            _camera = camera;
            _flurlClientCache = flurlClientCache;
        }

        public async Task ClearCameraTextAsync(
            CancellationToken cancellationToken = default)
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
            CancellationToken cancellationToken = default)
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
            CancellationToken cancellationToken = default)
        {
            var xmlContent = $"<ImageChannel version=\"2.0\" xmlns=\"http://www.hikvision.com/ver20/XMLSchema\"><IrcutFilter version=\"2.0\" xmlns=\"http://www.hikvision.com/ver20/XMLSchema\"><IrcutFilterType>{(sunriseSunset == SunriseSunset.Sunrise ? "day" : "night")}</IrcutFilterType></IrcutFilter></ImageChannel>";

            var client = GetConfiguredFlurlClient();

            try
            {
                var response = await client
                    .Request("/ISAPI/Image/channels/1")
                    .PutStringAsync(xmlContent, cancellationToken: cancellationToken);

                response.ResponseMessage.EnsureSuccessStatusCode();
            }
            catch (FlurlHttpException ex)
            {
                throw new HttpRequestException($"Error setting sunrise/sunset for camera {_camera.Id}: {ex.Message}", ex);
            }
        }

        private async Task PushCameraTextAsync(
            VideoOverlay videoOverlay,
            CancellationToken cancellationToken = default)
        {
            string xmlContent;
            using (var stringWriter = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(stringWriter))
                {
                    var serializer = new XmlSerializer(typeof(VideoOverlay));
                    serializer.Serialize(writer, videoOverlay);
                    xmlContent = stringWriter.ToString();
                }
            }

            var client = GetConfiguredFlurlClient();

            try
            {
                var response = await client
                    .Request(_camera.UpdateOverlayTextUrl)
                    .PutStringAsync(xmlContent, cancellationToken: cancellationToken);

                response.ResponseMessage.EnsureSuccessStatusCode();
            }
            catch (FlurlHttpException ex)
            {
                throw new HttpRequestException($"Error setting video overlay for camera {_camera.Id}: {ex.Message}", ex);
            }
        }

        private IFlurlClient GetConfiguredFlurlClient()
        {
            return _flurlClientCache.GetOrAdd($"camera*{_camera.Id}", $"http://{_camera.IpAddress}", (fluentClientBuilder) =>
            {
                if (!string.IsNullOrEmpty(_camera.CameraUsername) && !string.IsNullOrEmpty(_camera.CameraPassword))
                {
                    fluentClientBuilder.ConfigureInnerHandler(handler =>
                    {
                        handler.UseDefaultCredentials = true;
                        handler.Credentials = new NetworkCredential(
                            _camera.CameraUsername,
                            _camera.CameraPassword);
                    });
                }
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

        public async Task<Stream> GetSnapshotAsync(CancellationToken cancellationToken = default)
        {
            var client = GetConfiguredFlurlClient();

            try
            {
                var response = await client
                    .Request("/ISAPI/Streaming/channels/1/picture")
                    .GetAsync(cancellationToken: cancellationToken);

                return await response.GetStreamAsync();
            }
            catch (FlurlHttpException ex)
            {
                throw new HttpRequestException($"Error getting snapshot from camera {_camera.Id}: {ex.Message}", ex);
            }
        }

        public Task SetZoomAndFocusAsync(
            ZoomFocus zoomAndFocus,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ZoomFocus> GetZoomAndFocusAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> TriggerAutoFocusAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}