using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Cameras;
using OpenAlprWebhookProcessor.Cameras.ZoomAndFocus;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.CameraUpdateService
{
    public class CameraUpdateService : IHostedService
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

        public async Task ForceSunriseSunsetAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                var agent = await processorContext.Agents.FirstOrDefaultAsync();

                var camerasToUpdate = await processorContext.Cameras
                    .Where(x => x.UpdateDayNightModeEnabled)
                    .ToListAsync();

                foreach (var camera in camerasToUpdate)
                {
                    _backgroundJobClient.Enqueue(() => ProcessSunriseSunsetJobAsync(
                        camera.Id,
                        CameraScheduling.IsSunUp(
                            agent.Latitude,
                            agent.Longitude) ? SunriseSunset.Sunrise : SunriseSunset.Sunset,
                        false));
                }
            }
        }

        public async Task DeleteSunriseSunsetAsync(Guid cameraId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                var cameraToUpdate = await processorContext.Cameras
                    .Where(x => x.Id == cameraId)
                    .FirstOrDefaultAsync();

                if (cameraToUpdate == null)
                {
                    throw new ArgumentException("Camera not found");
                }

                if (!string.IsNullOrWhiteSpace(cameraToUpdate.NextDayNightScheduleId))
                {
                    _backgroundJobClient.Delete(cameraToUpdate.NextDayNightScheduleId);

                    cameraToUpdate.NextDayNightScheduleId = string.Empty;
                    await processorContext.SaveChangesAsync();
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
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                _logger.LogInformation("setting {sunriseSunset} for {cameraId}", sunriseSunset, cameraId);

                try
                {
                    var cameraToUpdate = await processorContext.Cameras.FirstOrDefaultAsync(x => x.Id == cameraId);

                    if (cameraToUpdate == null)
                    {
                        _logger.LogError("Unable to find camera with OpenAlprId: {cameraId}, check your configuration.", cameraId);
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
                        _logger.LogInformation("Scheduling next job for {cameraId}", cameraId);

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
                    _logger.LogError(ex.Message);
                }
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                await ForceSunriseSunsetAsync();

                await CameraScheduling.ScheduleDayNightTasksAsync(
                    this,
                    _serviceProvider,
                    _backgroundJobClient);

                await ForceClearOverlaysAsync();
            }, cancellationToken);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();

            await ForceClearOverlaysAsync();
        }

        public async Task ScheduleDayNightTaskAsync()
        {
            await CameraScheduling.ScheduleDayNightTasksAsync(
                this,
                _serviceProvider,
                _backgroundJobClient);
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

                _logger.LogInformation("processing job for plate: {plateNumber}", cameraUpdateRequest.LicensePlate);

                try
                {
                    var cameraToUpdate = await processorContext.Cameras.FirstOrDefaultAsync(x => x.Id == cameraUpdateRequest.Id);

                    if (cameraToUpdate == null)
                    {
                        _logger.LogError("Unable to find camera with OpenAlprId: {cameraId}, check your configuration.", cameraUpdateRequest.Id);
                        throw new ArgumentException($"unknown camera Id: {cameraUpdateRequest.Id}");
                    }

                    if (!string.IsNullOrWhiteSpace(cameraToUpdate.NextClearOverlayScheduleId))
                    {
                        _logger.LogInformation("cancelling redundant clear overlay job: {jobId}", cameraToUpdate.NextClearOverlayScheduleId);
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
                    _logger.LogError(ex.Message);
                }
            }
        }

        public async Task ClearExpiredOverlayAsync(Guid cameraId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                var cameraToUpdate = await processorContext.Cameras.FirstOrDefaultAsync(x => x.Id == cameraId);

                _logger.LogInformation("clearing expired overlay for: {cameraID}", cameraToUpdate.OpenAlprCameraId);

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

        private async Task ForceClearOverlaysAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                foreach (var cameraToUpdate in await processorContext.Cameras.ToListAsync())
                {
                    _logger.LogInformation("force clearing overlay for: {cameraId}", cameraToUpdate.OpenAlprCameraId);

                    var camera = CameraFactory.Create(cameraToUpdate.Manufacturer, cameraToUpdate);

                    await camera.ClearCameraTextAsync(
                        _cancellationTokenSource.Token);

                    cameraToUpdate.NextClearOverlayScheduleId = string.Empty;
                }

                await processorContext.SaveChangesAsync();
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
