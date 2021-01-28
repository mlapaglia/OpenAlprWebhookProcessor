using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Cameras;
using OpenAlprWebhookProcessor.Cameras.Configuration;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.CameraUpdateService
{
    public class CameraUpdateService : IHostedService
    {
        private readonly BlockingCollection<CameraUpdateRequest> _webhooksToProcess;

        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly ConcurrentDictionary<Guid, DateTime> _camerasWithActiveOverlays;

        private readonly IServiceScopeFactory _scopeFactory;

        private readonly ILogger _logger;

        public CameraUpdateService(
            IServiceScopeFactory scopeFactory,
            ILogger<CameraUpdateService> logger)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _webhooksToProcess = new BlockingCollection<CameraUpdateRequest>();
            _cancellationTokenSource = new CancellationTokenSource();
            _camerasWithActiveOverlays = new ConcurrentDictionary<Guid, DateTime>();
        }

        public void AddJob(CameraUpdateRequest request)
        {
            _logger.LogInformation("adding job for plate: " + request.LicensePlate);
            _webhooksToProcess.Add(request);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
                await ProcessJobsAsync(),
                cancellationToken);

            Task.Run(async () =>
                await ClearExpiredOverlaysAsync(),
                cancellationToken);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _webhooksToProcess.CompleteAdding();
            _webhooksToProcess.Dispose();
            _cancellationTokenSource.Cancel();

            await ForceClearOverlaysAsync();
        }

        private async Task ProcessJobsAsync()
        {
            foreach (var job in _webhooksToProcess.GetConsumingEnumerable(_cancellationTokenSource.Token))
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                    _logger.LogInformation("processing job for plate: " + job.LicensePlate);

                    try
                    {
                        var cameraToUpdate = await processorContext.Cameras.FirstOrDefaultAsync(x => x.Id == job.Id);

                        if (cameraToUpdate == null)
                        {
                            _logger.LogError($"Unable to find camera with OpenAlprId: {job.Id}, check your configuration.");
                            continue;
                        }

                        if (job.IsAlert)
                        {
                            var alertService = scope.ServiceProvider.GetRequiredService<AlertService.AlertService>();
                            alertService.AddJob(new AlertService.AlertUpdateRequest()
                            {
                                CameraId = job.Id,
                                Description = job.AlertDescription,
                                OpenAlprGroupUuid = job.LicensePlateImageUuid,
                            });
                        }

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
                            job.Id,
                            DateTime.UtcNow,
                            (oldkey, oldvalue) => DateTime.UtcNow);

                        cameraToUpdate.PlatesSeen++;
                        cameraToUpdate.LatestProcessedPlateUuid = job.LicensePlateImageUuid;

                        await processorContext.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                    }
                }
            }
        }

        private async Task ClearExpiredOverlaysAsync()
        {
            await ForceClearOverlaysAsync();

            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                if (!_camerasWithActiveOverlays.IsEmpty)
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                        foreach (var openAlprId in _camerasWithActiveOverlays)
                        {
                            var cameraToUpdate = await processorContext.Cameras.FirstOrDefaultAsync(x => x.Id == openAlprId.Key);

                            if ((DateTime.UtcNow - openAlprId.Value) > TimeSpan.FromSeconds(5))
                            {
                                _logger.LogInformation("clearing expired overlay for: " + cameraToUpdate.OpenAlprCameraId);

                                await ClearCameraOverlayAsync(cameraToUpdate);

                                _camerasWithActiveOverlays.TryRemove(openAlprId.Key, out var value);
                            }
                        }
                    }
                }

                await Task.Delay(1000);
            }
        }

        private async Task ForceClearOverlaysAsync()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                foreach (var cameraToUpdate in await processorContext.Cameras.ToListAsync())
                {
                    _logger.LogInformation("force clearing overlay for: " + cameraToUpdate.OpenAlprCameraId);

                    await ClearCameraOverlayAsync(cameraToUpdate);
                }
            }
        }

        private async Task ClearCameraOverlayAsync(Data.Camera cameraToUpdate)
        {
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
        }
    }
}
