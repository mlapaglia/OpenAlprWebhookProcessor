using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Cameras;
using System;
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
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var agent = await unitOfWork.Agents.GetFirstAgentAsync();

                var camerasToUpdate = await unitOfWork.Cameras.FindAsync(
                    x => x.UpdateDayNightModeEnabled,
                    _cancellationTokenSource.Token);

                foreach (var camera in camerasToUpdate)
                {
                    var latitude = camera.Latitude ?? agent?.Latitude;
                    var longitude = camera.Longitude ?? agent?.Longitude;

                    if (latitude.HasValue && longitude.HasValue)
                    {
                        _backgroundJobClient.Enqueue(() => ProcessSunriseSunsetJobAsync(
                            camera.Id,
                            CameraScheduling.IsSunUp(
                                latitude.Value,
                                longitude.Value) ? SunriseSunset.Sunrise : SunriseSunset.Sunset,
                            false));
                    }
                }
            }
        }

        public async Task DeleteSunriseSunsetAsync(Guid cameraId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var cameraToUpdate = await unitOfWork.Cameras.FirstOrDefaultAsync(
                    x => x.Id == cameraId,
                    _cancellationTokenSource.Token);

                if (cameraToUpdate == null)
                {
                    throw new ArgumentException("Camera not found");
                }

                if (!string.IsNullOrWhiteSpace(cameraToUpdate.NextDayNightScheduleId))
                {
                    _backgroundJobClient.Delete(cameraToUpdate.NextDayNightScheduleId);

                    cameraToUpdate.NextDayNightScheduleId = string.Empty;
                    await unitOfWork.SaveChangesAsync(_cancellationTokenSource.Token);
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
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                _logger.LogInformation("setting {sunriseSunset} for {cameraId}", sunriseSunset, cameraId);

                try
                {
                    var cameraToUpdate = await unitOfWork.Cameras.FirstOrDefaultAsync(
                        x => x.Id == cameraId,
                        _cancellationTokenSource.Token);

                    if (cameraToUpdate == null)
                    {
                        throw new ArgumentException("Camera not found");
                    }

                    if (!string.IsNullOrWhiteSpace(cameraToUpdate.NextDayNightScheduleId))
                    {
                        _backgroundJobClient.Delete(cameraToUpdate.NextDayNightScheduleId);
                        cameraToUpdate.NextDayNightScheduleId = string.Empty;
                    }

                    var camera = CameraFactory.Create(cameraToUpdate.Manufacturer, cameraToUpdate);

                    await camera.TriggerDayNightModeAsync(
                        sunriseSunset,
                        _cancellationTokenSource.Token);

                    var agent = await unitOfWork.Agents.GetFirstAgentAsync(_cancellationTokenSource.Token);

                    if (scheduleNextJob)
                    {
                        CameraScheduling.ScheduleDayNightTask(
                            this,
                            _backgroundJobClient,
                            agent,
                            cameraToUpdate);
                    }

                    await unitOfWork.SaveChangesAsync(_cancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing sunrise/sunset job for camera {cameraId}", cameraId);
                }
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _cancellationTokenSource.CancelAsync();
            _cancellationTokenSource.Dispose();

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
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                _logger.LogInformation("processing job for plate: {plateNumber}", cameraUpdateRequest.LicensePlate);

                try
                {
                    var cameraToUpdate = await unitOfWork.Cameras.FirstOrDefaultAsync(
                        x => x.Id == cameraUpdateRequest.Id,
                        _cancellationTokenSource.Token);

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

                    await unitOfWork.SaveChangesAsync(_cancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing job for camera {cameraId}", cameraUpdateRequest.Id);
                }
            }
        }

        public async Task ClearExpiredOverlayAsync(Guid cameraId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var cameraToUpdate = await unitOfWork.Cameras.FirstOrDefaultAsync(
                    x => x.Id == cameraId,
                    _cancellationTokenSource.Token);

                if (cameraToUpdate == null)
                {
                    _logger.LogError("Unable to find camera with Id: {cameraId}", cameraId);
                    return;
                }

                _logger.LogInformation("clearing expired overlay for: {cameraID}", cameraToUpdate.OpenAlprCameraId);

                try
                {
                    var camera = CameraFactory.Create(cameraToUpdate.Manufacturer, cameraToUpdate);

                    await camera.ClearCameraTextAsync(_cancellationTokenSource.Token);

                    cameraToUpdate.NextClearOverlayScheduleId = string.Empty;

                    await unitOfWork.SaveChangesAsync(_cancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error clearing overlay for camera {cameraId}", cameraId);
                }
            }
        }

        public async Task<ZoomFocus> GetZoomAndFocusAsync(
            Guid cameraId,
            CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var dbCamera = await unitOfWork.Cameras.FirstOrDefaultAsync(
                    x => x.Id == cameraId,
                    cancellationToken);

                if (dbCamera == null)
                {
                    throw new ArgumentException("Camera not found");
                }

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
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var dbCamera = await unitOfWork.Cameras.FirstOrDefaultAsync(
                    x => x.Id == cameraId,
                    cancellationToken);

                if (dbCamera == null)
                {
                    throw new ArgumentException("Camera not found");
                }

                var camera = CameraFactory.Create(
                    dbCamera.Manufacturer,
                    dbCamera);

                await camera.SetZoomAndFocusAsync(zoomAndFocus, cancellationToken);
            }
        }

        public async Task<bool> TriggerAutofocusAsync(
            Guid cameraId,
            CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var dbCamera = await unitOfWork.Cameras.FirstOrDefaultAsync(
                    x => x.Id == cameraId,
                    cancellationToken);

                if (dbCamera == null)
                {
                    throw new ArgumentException("Camera not found");
                }

                var camera = CameraFactory.Create(
                    dbCamera.Manufacturer,
                    dbCamera);

                return await camera.TriggerAutoFocusAsync(cancellationToken);
            }
        }

        public async Task ForceClearOverlaysAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var camerasToUpdate = await unitOfWork.Cameras.GetAllAsync(_cancellationTokenSource.Token);

                foreach (var cameraToUpdate in camerasToUpdate)
                {
                    try
                    {
                        var camera = CameraFactory.Create(cameraToUpdate.Manufacturer, cameraToUpdate);

                        await camera.ClearCameraTextAsync(_cancellationTokenSource.Token);

                        cameraToUpdate.NextClearOverlayScheduleId = string.Empty;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error clearing overlay for camera {cameraId}", cameraToUpdate.Id);
                    }
                }

                await unitOfWork.SaveChangesAsync(_cancellationTokenSource.Token);
            }
        }
    }
}
