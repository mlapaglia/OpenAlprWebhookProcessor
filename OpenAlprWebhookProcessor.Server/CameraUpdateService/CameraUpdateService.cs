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
    public class CameraUpdateService : IHostedService, ICameraUpdateService
    {
        private readonly IBackgroundJobService _backgroundJobService;

        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger _logger;

        public CameraUpdateService(
            IServiceProvider serviceProvider,
            ILogger<CameraUpdateService> logger,
            IBackgroundJobService backgroundJobService)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _backgroundJobService = backgroundJobService ?? throw new ArgumentNullException(nameof(backgroundJobService));
            _cancellationTokenSource = new CancellationTokenSource();
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
                    _backgroundJobService.DeleteJob(cameraToUpdate.NextDayNightScheduleId);

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
                var cameraFactory = scope.ServiceProvider.GetRequiredService<ICameraFactory>();

                _logger.LogInformation("setting {SunriseSunset} for {CameraId}", sunriseSunset, cameraId);

                var cameraToUpdate = await unitOfWork.Cameras.FirstOrDefaultAsync(
                    x => x.Id == cameraId,
                    _cancellationTokenSource.Token);

                if (cameraToUpdate == null)
                {
                    throw new ArgumentException("camera not found");
                }

                var updateRequest = new CameraUpdateRequest
                {
                    Id = cameraId,
                    LicensePlate = sunriseSunset == SunriseSunset.Sunrise ? "DAY" : "NIGHT",
                    IsTest = true,
                    IsSinglePlate = false,
                    IsPreviewGroup = false,
                    LicensePlateImageUuid = Guid.NewGuid().ToString()
                };

                var camera = cameraFactory.Create(cameraToUpdate.Manufacturer, cameraToUpdate);

                await camera.SetCameraTextAsync(
                    updateRequest,
                    _cancellationTokenSource.Token);

                await unitOfWork.SaveChangesAsync(_cancellationTokenSource.Token);

                if (scheduleNextJob)
                {
                    _logger.LogInformation("Scheduling additional sunrise/sunset tasks after completing task for {CameraId}", cameraId);
                    await CameraScheduling.ScheduleDayNightTasksAsync(unitOfWork, _backgroundJobService);
                }
            }
        }

        public async Task ClearExpiredOverlayAsync(Guid cameraId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var cameraFactory = scope.ServiceProvider.GetRequiredService<ICameraFactory>();

                _logger.LogInformation("clearing expired overlay for {CameraId}", cameraId);

                var cameraToUpdate = await unitOfWork.Cameras.FirstOrDefaultAsync(
                    x => x.Id == cameraId,
                    _cancellationTokenSource.Token);

                if (cameraToUpdate == null)
                {
                    throw new ArgumentException("Camera not found");
                }

                try
                {
                    var camera = cameraFactory.Create(cameraToUpdate.Manufacturer, cameraToUpdate);

                    await camera.ClearCameraTextAsync(_cancellationTokenSource.Token);

                    cameraToUpdate.NextClearOverlayScheduleId = string.Empty;
                    await unitOfWork.SaveChangesAsync(_cancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error clearing expired overlay for camera {CameraId}", cameraId);
                }
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await ForceClearOverlaysAsync(cancellationToken);
            
            await _cancellationTokenSource.CancelAsync();
            _cancellationTokenSource.Dispose();
        }

        public async Task ScheduleDayNightTaskAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                await CameraScheduling.ScheduleDayNightTasksAsync(unitOfWork, _backgroundJobService);
            }
        }

        public void EnqueueDayNight(
            Guid cameraId,
            SunriseSunset sunriseSunset)
        {
            CameraScheduling.ExecuteSingleDayNightTaskAsync(
                sunriseSunset,
                cameraId,
                _backgroundJobService);
        }

        public void ScheduleOverlayRequest(CameraUpdateRequest cameraUpdateRequest)
        {
            _backgroundJobService.EnqueueProcessJobAsync(cameraUpdateRequest);
        }

        public async Task ProcessJobAsync(CameraUpdateRequest cameraUpdateRequest)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var cameraFactory = scope.ServiceProvider.GetRequiredService<ICameraFactory>();

                _logger.LogInformation("processing job for plate: {PlateNumber}", cameraUpdateRequest.LicensePlate);

                try
                {
                    var cameraToUpdate = await unitOfWork.Cameras.FirstOrDefaultAsync(
                        x => x.Id == cameraUpdateRequest.Id,
                        _cancellationTokenSource.Token);

                    if (cameraToUpdate == null)
                    {
                        _logger.LogError("Unable to find camera with OpenAlprId: {CameraId}, check your configuration.", cameraUpdateRequest.Id);
                        throw new ArgumentException($"unknown camera Id: {cameraUpdateRequest.Id}");
                    }

                    if (!string.IsNullOrWhiteSpace(cameraToUpdate.NextClearOverlayScheduleId))
                    {
                        _logger.LogInformation("cancelling redundant clear overlay job: {JobId}", cameraToUpdate.NextClearOverlayScheduleId);
                        _backgroundJobService.DeleteJob(cameraToUpdate.NextClearOverlayScheduleId);
                    }

                    var camera = cameraFactory.Create(cameraToUpdate.Manufacturer, cameraToUpdate);

                    await camera.SetCameraTextAsync(
                        cameraUpdateRequest,
                        _cancellationTokenSource.Token);

                    cameraToUpdate.NextClearOverlayScheduleId = _backgroundJobService.ScheduleClearOverlayJob(
                       cameraUpdateRequest.Id,
                       TimeSpan.FromSeconds(5));

                    if (!cameraUpdateRequest.IsTest
                        && !cameraUpdateRequest.IsPreviewGroup
                        && !cameraUpdateRequest.IsSinglePlate)
                    {
                        cameraToUpdate.PlatesSeen++;
                        cameraToUpdate.LatestProcessedPlateUuid = cameraUpdateRequest.LicensePlateImageUuid;
                    }

                    await unitOfWork.SaveChangesAsync(_cancellationTokenSource.Token);
                    
                    _logger.LogInformation("completed processing job for plate: {PlateNumber}", cameraUpdateRequest.LicensePlate);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing job for plate: {PlateNumber}", cameraUpdateRequest.LicensePlate);
                    throw;
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
                var cameraFactory = scope.ServiceProvider.GetRequiredService<ICameraFactory>();

                var dbCamera = await unitOfWork.Cameras.FirstOrDefaultAsync(
                    x => x.Id == cameraId,
                    cancellationToken);

                if (dbCamera == null)
                {
                    throw new ArgumentException("Camera not found");
                }

                var camera = cameraFactory.Create(
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
                var cameraFactory = scope.ServiceProvider.GetRequiredService<ICameraFactory>();

                var dbCamera = await unitOfWork.Cameras.FirstOrDefaultAsync(
                    x => x.Id == cameraId,
                    cancellationToken);

                if (dbCamera == null)
                {
                    throw new ArgumentException("Camera not found");
                }

                var camera = cameraFactory.Create(
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
                var cameraFactory = scope.ServiceProvider.GetRequiredService<ICameraFactory>();

                var dbCamera = await unitOfWork.Cameras.FirstOrDefaultAsync(
                    x => x.Id == cameraId,
                    cancellationToken);

                if (dbCamera == null)
                {
                    throw new ArgumentException("Camera not found");
                }

                var camera = cameraFactory.Create(
                    dbCamera.Manufacturer,
                    dbCamera);

                return await camera.TriggerAutoFocusAsync(cancellationToken);
            }
        }

        public async Task ForceClearOverlaysAsync(CancellationToken cancellationToken = default)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var cameraFactory = scope.ServiceProvider.GetRequiredService<ICameraFactory>();

                var camerasToUpdate = await unitOfWork.Cameras.GetAllAsync(cancellationToken);

                foreach (var cameraToUpdate in camerasToUpdate)
                {
                    try
                    {
                        var camera = cameraFactory.Create(cameraToUpdate.Manufacturer, cameraToUpdate);

                        await camera.ClearCameraTextAsync(cancellationToken);

                        cameraToUpdate.NextClearOverlayScheduleId = string.Empty;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error clearing overlay for camera {CameraId}", cameraToUpdate.Id);
                    }
                }

                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
