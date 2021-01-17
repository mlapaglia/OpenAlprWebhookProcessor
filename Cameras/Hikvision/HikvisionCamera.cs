﻿using OpenAlprWebhookProcessor.Cameras.Hikvision;
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

namespace OpenAlprWebhookProcessor.Cameras
{
    public static class HikvisionCamera
    {
        public static async Task ClearCameraTextAsync(
            Data.Camera cameraToUpdate,
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
                cameraToUpdate,
                videoOverlayRequest,
                cancellationToken);
        }

        public static async Task SetCameraTextAsync(
            Data.Camera cameraToUpdate,
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
                cameraToUpdate,
                videoOverlayRequest,
                cancellationToken);
        }

        private static async Task PushCameraTextAsync(
            Data.Camera cameraToUpdate,
            VideoOverlay videoOverlay,
            CancellationToken cancellationToken)
        {
            var client = new HttpClient(new HttpClientHandler()
            {
                UseDefaultCredentials = true,
                Credentials = new NetworkCredential(
                    cameraToUpdate.CameraUsername,
                    cameraToUpdate.CameraPassword),
            });

            using (var stringWriter = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(stringWriter))
                {
                    var serializer = new XmlSerializer(typeof(VideoOverlay));
                    serializer.Serialize(
                        writer,
                        videoOverlay);

                    var response = await client.PutAsync(
                        cameraToUpdate.UpdateOverlayTextUrl,
                        new StringContent(stringWriter.ToString()),
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
