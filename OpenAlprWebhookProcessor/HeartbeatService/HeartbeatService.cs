using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Cameras.Configuration;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.HeartbeatService
{
    public class HeartbeatService : IHostedService
    {
        private readonly ILogger _logger;

        private readonly IServiceScopeFactory _scopeFactory;

        private readonly bool _webRequestLoggingEnabled;

        private readonly AgentConfiguration _agentConfiguration;

        private readonly CancellationTokenSource _cancellationTokenSource;

        private string _companyId;

        public HeartbeatService(
            ILogger<HeartbeatService> logger,
            IConfiguration configuration,
            IServiceScopeFactory scopeFactory,
            AgentConfiguration agentConfiguration)
        {
            _logger = logger;
            _webRequestLoggingEnabled = configuration.GetValue("WebRequestLoggingEnabled", false);
            _agentConfiguration = agentConfiguration;
            _scopeFactory = scopeFactory;
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
            _companyId = await RegisterAgentAsync();

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

                if (_webRequestLoggingEnabled)
                {
                    _logger.LogInformation("hearbeat sent: {0}", serializedHeartbeat);
                }

                await httpClient.PostAsync(
                    $"{_agentConfiguration.OpenAlprWebServer.Endpoint}push",
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
                CompanyId = _companyId,
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

        private async Task<string> RegisterAgentAsync()
        {
            try
            {
                var companyId = await AgentRegistration.RegisterAgentAsync(
                    _agentConfiguration.OpenAlprWebServer.Endpoint,
                    _agentConfiguration.OpenAlprWebServer.Username,
                    _agentConfiguration.OpenAlprWebServer.Password,
                    _agentConfiguration.OpenAlprWebServer.IgnoreSslErrors);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                    var company = await processorContext.Companies.FirstOrDefaultAsync(x => x.CompanyId == companyId);

                    if (company == null)
                    {
                        company = new Data.Company()
                        {
                            CompanyId = companyId,
                            Username = _agentConfiguration.OpenAlprWebServer.Username,
                        };

                        processorContext.Companies.Add(company);

                        await processorContext.SaveChangesAsync();
                    }

                    _logger.LogInformation("Agent registered with server successfully");

                    return company.CompanyId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "failed to register Agent");
                throw;
            }
        }
    }
}
