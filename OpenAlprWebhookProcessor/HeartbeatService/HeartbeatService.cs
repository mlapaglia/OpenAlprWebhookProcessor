using Microsoft.Extensions.Hosting;
using OpenAlprWebhookProcessor.Cameras.Configuration;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.HeartbeatService
{
    public class HeartbeatService : IHostedService
    {
        private readonly AgentConfiguration _agentConfiguration;

        private readonly CancellationTokenSource _cancellationTokenSource;

        public HeartbeatService(AgentConfiguration agentConfiguration)
        {
            _agentConfiguration = agentConfiguration;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () => await SendHeartbeatsAsync());

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();

            return Task.CompletedTask;
        }

        private async Task SendHeartbeatsAsync()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                var clientHandler = new HttpClientHandler();

                if (_agentConfiguration.OpenAlprWebServer.IgnoreSslErrors)
                {
                    clientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                }

                var httpClient = new HttpClient(clientHandler);

                var heartbeat = BuildOpenAlprHeartbeat();

                var serializedHeartbeat = JsonSerializer.Serialize(heartbeat);

                await httpClient.PostAsync(
                    _agentConfiguration.OpenAlprWebServer.Endpoint,
                    new StringContent(serializedHeartbeat),
                    _cancellationTokenSource.Token);

                await Task.Delay(
                    60 * 1000,
                    _cancellationTokenSource.Token);
            }
        }

        private OpenAlprHeatbeat BuildOpenAlprHeartbeat()
        {
            var heartbeat = new OpenAlprHeatbeat()
            {
                AgentHostname = _agentConfiguration.Hostname,
                AgentType = "alprd",
                AgentUid = _agentConfiguration.Uid,
                AgentVersion = _agentConfiguration.Version,
                BeanstalkQueueSize = 0,
                CompanyId = _agentConfiguration.CompanyId,
                CpuCores = 1,
                CpuLastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                CpuUsagePercent = 50.00,
                DaemonUptimeSeconds = 100,
                DataType = "heartbeat",
                DiskDriveFreeBytes = 7102922559488,
                DiskDriveTotalBytes = 49993471488000,
                DiskQuotaConsumedBytes = 1572864000,
                DiskQuotaEarliestResult = DateTimeOffset.UtcNow.AddMonths(-1).ToUnixTimeMilliseconds(),
                DiskQuotaTotalBytes = 49993471488000,
                LicenseKey = string.Empty,
                LicenseValid = true,
                LicenseSystemId = "11122233344455566677",
                MemoryConsumedBytes = 67464105984,
                MemoryLastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                MemorySwapTotalBytes = 0,
                MemorySwapUsedBytes = 0,
                MemoryTotalBytes = 67464237056,
                OpenAlprVersion = _agentConfiguration.Version,
                Os = "linux",
                ProcessingThreadsActive = 2,
                ProcessingThreadsConfigured = 2,
                RecordingEnabled = false,
                SystemUptimeSeconds = DateTimeOffset.UtcNow.AddMonths(-1).ToUnixTimeMilliseconds(),
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            };

            heartbeat.VideoStreams = new System.Collections.Generic.List<VideoStream>();

            foreach (var camera in _agentConfiguration.Cameras)
            {
                heartbeat.VideoStreams.Add(new VideoStream()
                {
                    CameraId = camera.OpenAlprCameraId,
                    CameraName = camera.Name,
                    Fps = 15,
                    IsStreaming = true,
                    Url = camera.UpdateOverlayTextUrl.ToString(),
                    GpsLatitude = camera.Latitude,
                    GpsLongitude = camera.Longitude,
                    LastPlateRead = DateTimeOffset.UtcNow.AddMinutes(-1).ToUnixTimeMilliseconds(),
                    LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    TotalPlatesRead = 100,
                });
            }

            return heartbeat;
        }
    }
}
