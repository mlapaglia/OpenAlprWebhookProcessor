using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Cameras;
using OpenAlprWebhookProcessor.Cameras.Configuration;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.CameraUpdateService
{
    public class CameraUpdateService : IHostedService
    {
        private readonly BlockingCollection<CameraUpdateRequest> _webhooksToProcess;

        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly CameraConfiguration _cameraConfiguration;

        private readonly ConcurrentDictionary<int, DateTime> _camerasWithActiveOverlays;

        private readonly ILogger _logger;

        public CameraUpdateService(
            ILogger<CameraUpdateService> logger,
            CameraConfiguration cameraConfiguration)
        {
            _logger = logger;
            _cameraConfiguration = cameraConfiguration;
            _webhooksToProcess = new BlockingCollection<CameraUpdateRequest>();
            _cancellationTokenSource = new CancellationTokenSource();
            _camerasWithActiveOverlays = new ConcurrentDictionary<int, DateTime>();
        }

        public void AddJob(CameraUpdateRequest request)
        {
            _logger.LogInformation("adding job for plate: " + request.LicensePlate);
            _webhooksToProcess.Add(request);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () => await ProcessJobsAsync());
            Task.Run(async () => await ClearExpiredOverlaysAsync());

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            _webhooksToProcess.CompleteAdding();
            _webhooksToProcess.Dispose();

            return Task.CompletedTask;
        }

        private async Task ProcessJobsAsync()
        {
            foreach (var job in _webhooksToProcess.GetConsumingEnumerable(_cancellationTokenSource.Token))
            {
                _logger.LogInformation("processing job for plate: " + job.LicensePlate);

                try
                {
                    var cameraToUpdate = _cameraConfiguration.Cameras.First(x => x.OpenAlprCameraId == job.OpenAlprCameraId);

                    switch (cameraToUpdate.Manufacturer)
                    {
                        case CameraManufacturer.Dahua:
                            await DahuaCamera.SetCameraTextAsync(
                                cameraToUpdate,
                                job,
                                _cancellationTokenSource.Token);
                            break;
                        case CameraManufacturer.Hikvision:
                            await HikvisionCamera.SetCameraTextAsync(
                                cameraToUpdate,
                                job,
                                _cancellationTokenSource.Token);
                            break;
                        default:
                            break;
                    }

                    _camerasWithActiveOverlays.AddOrUpdate(
                        job.OpenAlprCameraId,
                        DateTime.UtcNow,
                        (oldkey, oldvalue) => DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }
        }

        private async Task ClearExpiredOverlaysAsync()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                foreach (var openAlprId in _camerasWithActiveOverlays)
                {
                    var cameraToUpdate = _cameraConfiguration.Cameras.First(x => x.OpenAlprCameraId == openAlprId.Key);

                    if ((DateTime.UtcNow - openAlprId.Value) > TimeSpan.FromSeconds(5))
                    {
                        _logger.LogInformation("clearing expired overlay for: " + cameraToUpdate.OpenAlprCameraId);

                        switch (cameraToUpdate.Manufacturer)
                        {
                            case CameraManufacturer.Dahua:
                                await DahuaCamera.ClearCameraTextAsync(
                                    cameraToUpdate,
                                    _cancellationTokenSource.Token);
                                break;
                            case CameraManufacturer.Hikvision:
                                await HikvisionCamera.ClearCameraTextAsync(
                                    cameraToUpdate,
                                    _cancellationTokenSource.Token);
                                break;
                            default:
                                break;
                        }

                        _camerasWithActiveOverlays.TryRemove(openAlprId.Key, out var value);
                    }
                }

                await Task.Delay(1000);
            }
        }
    }
}
