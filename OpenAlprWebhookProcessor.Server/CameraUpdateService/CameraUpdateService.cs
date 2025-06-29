using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Server.Cameras;
using OpenAlprWebhookProcessor.Server.Cameras.ZoomAndFocus;
using OpenAlprWebhookProcessor.Server.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Server.CameraUpdateService
{
    public class CameraUpdateService : IHostedService, ICameraUpdateService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;

        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger _logger;

        public CameraUpdateService(
            IServiceProvider serviceProvider,
            ILogger<CameraUpdateService> logger,
            IBackgroundJobClient backgroundJobClient)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _cancellationTokenSource = new CancellationTokenSource();
            _backgroundJobClient = backgroundJobClient;
        }

        public async Task ForceSunriseSunsetAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                var agent = await processorContext.Agents.FirstOrDefaultAsync(cancellationToken);

                var camerasToUpdate = await processorContext.Cameras
                    .Where(x => x.UpdateDayNightModeEnabled)
                    .ToListAsync(cancellationToken);

                foreach (var camera in camerasToUpdate)
                {
                    var latitude = camera.Latitude ?? agent.Latitude;
                    var longitude = camera.Longitude ?? agent.Longitude;

                    _backgroundJobClient.Enqueue(() => ProcessSunriseSunsetJobAsync(
                        camera.Id,
                        CameraScheduling.IsSunUp(
                            latitude.Value,
                            longitude.Value) ? SunriseSunset.Sunrise : SunriseSunset.Sunset,
                        false));
                }
            }
        }

        public async Task DeleteSunriseSunsetAsync(
            Guid cameraId,
            CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                using (var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>())
                {
                    var cameraToUpdate = await processorContext.Cameras
                        .Where(x => x.Id == cameraId)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (cameraToUpdate == null)
                    {
                        throw new ArgumentException("Camera not found");
                    }

                    if (!string.IsNullOrWhiteSpace(cameraToUpdate.NextDayNightScheduleId))
                    {
                        _backgroundJobClient.Delete(cameraToUpdate.NextDayNightScheduleId);

                        cameraToUpdate.NextDayNightScheduleId = string.Empty;
                        await processorContext.SaveChangesAsync(cancellationToken);
                    }
                }
            }
        }

        public async Task ProcessSunriseSunsetJobAsync(
            Guid cameraId,
            SunriseSunset sunriseSunset,
            bool scheduleNextJob)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                using (var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>())
                {
                    _logger.LogInformation("Setting {SunriseSunset} for {CameraId}", sunriseSunset, cameraId);

                    try
                    {
                        var cameraToUpdate = await processorContext.Cameras.FirstOrDefaultAsync(x => x.Id == cameraId);

                        if (cameraToUpdate == null)
                        {
                            _logger.LogError("Unable to find camera with OpenAlprId: {CameraId}, check your configuration.", cameraId);
                            return;
                        }

                        var camera = CameraFactory.Create(cameraToUpdate.Manufacturer, cameraToUpdate);
                        await camera.TriggerDayNightModeAsync(
                            sunriseSunset,
                            _cancellationTokenSource.Token);

                        await TriggerZoomAndFocusAsync(
                            sunriseSunset,
                            cameraToUpdate,
                            camera);

                        if (scheduleNextJob)
                        {
                            _logger.LogInformation("Scheduling next job for {CameraId}", cameraId);

                            var agent = await processorContext.Agents.FirstOrDefaultAsync();

                            CameraScheduling.ScheduleDayNightTask(
                                this,
                                _backgroundJobClient,
                                agent,
                                cameraToUpdate);

                            await processorContext.SaveChangesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ex.Message);
                    }
                }
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                await ForceSunriseSunsetAsync(cancellationToken);

                await CameraScheduling.ScheduleDayNightTasksAsync(
                    this,
                    _serviceProvider,
                    _backgroundJobClient,
                    cancellationToken);

                await ForceClearOverlaysAsync();
            }, cancellationToken);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _cancellationTokenSource.CancelAsync();
            _cancellationTokenSource.Dispose();

            await ForceClearOverlaysAsync();
        }

        public async Task ScheduleDayNightTaskAsync(CancellationToken cancellationToken)
        {
            await CameraScheduling.ScheduleDayNightTasksAsync(
                this,
                _serviceProvider,
                _backgroundJobClient,
                cancellationToken);
        }

        public void EnqueueDayNight(
            Guid cameraId,
            SunriseSunset sunriseSunset)
        {
            CameraScheduling.ExecuteSingleDayNightTask(
                sunriseSunset,
                cameraId,
                this,
                _backgroundJobClient);
        }

        public void ScheduleOverlayRequest(CameraUpdateRequest cameraUpdateRequest)
        {
            _backgroundJobClient.Enqueue(() => ProcessJobAsync(cameraUpdateRequest));
        }

        public async Task ProcessJobAsync(CameraUpdateRequest cameraUpdateRequest)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                _logger.LogInformation("Processing job for plate: {PlateNumber}", cameraUpdateRequest.LicensePlate);

                try
                {
                    var cameraToUpdate = await processorContext.Cameras.FirstOrDefaultAsync(x => x.Id == cameraUpdateRequest.Id);

                    if (cameraToUpdate == null)
                    {
                        _logger.LogError("Unable to find camera with OpenAlprId: {CameraId}, check your configuration.", cameraUpdateRequest.Id);
                        throw new ArgumentException($"unknown camera Id: {cameraUpdateRequest.Id}");
                    }

                    if (!string.IsNullOrWhiteSpace(cameraToUpdate.NextClearOverlayScheduleId))
                    {
                        _logger.LogInformation("cancelling redundant clear overlay job: {JobId}", cameraToUpdate.NextClearOverlayScheduleId);
                        _backgroundJobClient.Delete(cameraToUpdate.NextClearOverlayScheduleId);
                    }

                    var camera = CameraFactory.Create(cameraToUpdate.Manufacturer, cameraToUpdate);

                    await camera.SetCameraTextAsync(
                        cameraUpdateRequest,
                        _cancellationTokenSource.Token);

                    cameraToUpdate.NextClearOverlayScheduleId = _backgroundJobClient.Schedule(
                       () => ClearExpiredOverlayAsync(cameraUpdateRequest.Id),
                       TimeSpan.FromSeconds(5));

                    if (!cameraUpdateRequest.IsTest
                        && !cameraUpdateRequest.IsPreviewGroup
                        && !cameraUpdateRequest.IsSinglePlate)
                    {
                        cameraToUpdate.PlatesSeen++;
                        cameraToUpdate.LatestProcessedPlateUuid = cameraUpdateRequest.LicensePlateImageUuid;
                    }

                    await processorContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }
        }

        public async Task ClearExpiredOverlayAsync(Guid cameraId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                var cameraToUpdate = await processorContext.Cameras.FirstOrDefaultAsync(x => x.Id == cameraId);

                _logger.LogInformation("clearing expired overlay for: {CameraID}", cameraToUpdate.OpenAlprCameraId);

                var camera = CameraFactory.Create(cameraToUpdate.Manufacturer, cameraToUpdate);

                await camera.ClearCameraTextAsync(
                    _cancellationTokenSource.Token);

                cameraToUpdate.NextClearOverlayScheduleId = string.Empty;

                await processorContext.SaveChangesAsync();
            }
        }

        public async Task<ZoomFocus> GetZoomAndFocusAsync(
            Guid cameraId,
            CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                var dbCamera = await processorContext.Cameras.FirstOrDefaultAsync(
                    x => x.Id == cameraId,
                    cancellationToken);

                var camera = CameraFactory.Create(
                    dbCamera.Manufacturer,
                    dbCamera);

                return await camera.GetZoomAndFocusAsync(cancellationToken);
            }
        }

        public async Task SetZoomAndFocusAsync(
            Guid cameraId,
            ZoomFocus zoomAndFocus,
            CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                var dbCamera = await processorContext.Cameras.FirstOrDefaultAsync(
                    x => x.Id == cameraId,
                    cancellationToken);

                var camera = CameraFactory.Create(
                    dbCamera.Manufacturer,
                    dbCamera);

                await camera.SetZoomAndFocusAsync(
                    zoomAndFocus,
                    cancellationToken);
            }
        }

        public async Task<bool> TriggerAutofocusAsync(
            Guid cameraId,
            CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                var dbCamera = await processorContext.Cameras.FirstOrDefaultAsync(
                    x => x.Id == cameraId,
                    cancellationToken);

                var camera = CameraFactory.Create(
                    dbCamera.Manufacturer,
                    dbCamera);

                return await camera.TriggerAutoFocusAsync(cancellationToken);
            }
        }

        private async Task ForceClearOverlaysAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                foreach (var cameraToUpdate in await processorContext.Cameras.ToListAsync(_cancellationTokenSource.Token))
                {
                    _logger.LogInformation("force clearing overlay for: {CameraId}", cameraToUpdate.OpenAlprCameraId);

                    var camera = CameraFactory.Create(
                        cameraToUpdate.Manufacturer,
                        cameraToUpdate);

                    await camera.ClearCameraTextAsync(
                        _cancellationTokenSource.Token);

                    cameraToUpdate.NextClearOverlayScheduleId = string.Empty;
                }

                await processorContext.SaveChangesAsync(_cancellationTokenSource.Token);
            }
        }

        private static async Task TriggerZoomAndFocusAsync(
            SunriseSunset sunriseSunset,
            Data.Camera cameraToUpdate,
            ICamera camera)
        {
            ZoomFocus zoomFocus = null;

            if (sunriseSunset == SunriseSunset.Sunrise
                && cameraToUpdate.DayFocus.HasValue
                && cameraToUpdate.DayZoom.HasValue)
            {
                zoomFocus = new ZoomFocus()
                {
                    Focus = cameraToUpdate.DayFocus.Value,
                    Zoom = cameraToUpdate.DayZoom.Value,
                };
            }
            else if (sunriseSunset == SunriseSunset.Sunset
                && cameraToUpdate.NightFocus.HasValue
                && cameraToUpdate.NightZoom.HasValue)
            {
                zoomFocus = new ZoomFocus()
                {
                    Focus = cameraToUpdate.NightFocus.Value,
                    Zoom = cameraToUpdate.NightZoom.Value,
                };
            }

            if (zoomFocus != null)
            {
                await camera.SetZoomAndFocusAsync(zoomFocus, default);
            }
        }
    }
}
