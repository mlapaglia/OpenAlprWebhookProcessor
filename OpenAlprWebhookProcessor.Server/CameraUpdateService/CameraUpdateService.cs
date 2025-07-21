using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Cameras;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.CameraUpdateService
{
    public class CameraUpdateService : ICameraUpdateService
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger<CameraUpdateService> _logger;

        private readonly IBackgroundJobService _backgroundJobService;

        public CameraUpdateService(
            IServiceProvider serviceProvider,
            ILogger<CameraUpdateService> logger,
            IBackgroundJobService backgroundJobService)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _backgroundJobService = backgroundJobService ?? throw new ArgumentNullException(nameof(backgroundJobService));
        }

        public async Task DeleteSunriseSunsetAsync(Guid cameraId)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var cameraToUpdate = await unitOfWork.Cameras.FirstOrDefaultAsync(
                x => x.Id == cameraId);

            if (cameraToUpdate == null)
            {
                throw new ArgumentException($"Camera not found: {cameraId}", nameof(cameraId));
            }

            if (!string.IsNullOrWhiteSpace(cameraToUpdate.NextDayNightScheduleId))
            {
                _backgroundJobService.DeleteJob(cameraToUpdate.NextDayNightScheduleId);

                cameraToUpdate.NextDayNightScheduleId = string.Empty;
                await unitOfWork.SaveChangesAsync();
            }
        }

        public async Task ProcessSunriseSunsetJobAsync(
            Guid cameraId,
            SunriseSunset sunriseSunset,
            bool scheduleNextJob)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var cameraFactory = scope.ServiceProvider.GetRequiredService<ICameraFactory>();

            _logger.LogInformation("Setting {SunriseSunset} for camera {CameraId}", sunriseSunset, cameraId);

            var cameraToUpdate = await unitOfWork.Cameras.FirstOrDefaultAsync(
                x => x.Id == cameraId);

            if (cameraToUpdate == null)
            {
                throw new ArgumentException($"Camera not found: {cameraId}", nameof(cameraId));
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

            await camera.SetCameraTextAsync(updateRequest, default);

            await unitOfWork.SaveChangesAsync();

            if (scheduleNextJob)
            {
                _logger.LogInformation("Scheduling additional sunrise/sunset tasks after completing task for {CameraId}", cameraId);
                await CameraScheduling.ScheduleDayNightTasksAsync(unitOfWork, _backgroundJobService);
            }
        }

        public async Task ClearExpiredOverlayAsync(Guid cameraId)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var cameraFactory = scope.ServiceProvider.GetRequiredService<ICameraFactory>();

            _logger.LogInformation("Clearing expired overlay for camera {CameraId}", cameraId);

            var cameraToUpdate = await unitOfWork.Cameras.FirstOrDefaultAsync(
                x => x.Id == cameraId);

            if (cameraToUpdate == null)
            {
                throw new ArgumentException($"Camera not found: {cameraId}", nameof(cameraId));
            }

            try
            {
                var camera = cameraFactory.Create(cameraToUpdate.Manufacturer, cameraToUpdate);

                await camera.ClearCameraTextAsync(default);

                cameraToUpdate.NextClearOverlayScheduleId = string.Empty;
                await unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing expired overlay for camera {CameraId}", cameraId);
                throw;
            }
        }

        public async Task ScheduleDayNightTaskAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await CameraScheduling.ScheduleDayNightTasksAsync(unitOfWork, _backgroundJobService);
        }

        public async Task EnqueueDayNightAsync(
            Guid cameraId,
            SunriseSunset sunriseSunset)
        {
            await CameraScheduling.ExecuteSingleDayNightTaskAsync(
                sunriseSunset,
                cameraId,
                _backgroundJobService);
        }

        public async Task ScheduleOverlayRequestAsync(CameraUpdateRequest cameraUpdateRequest)
        {
            await _backgroundJobService.EnqueueProcessJobAsync(cameraUpdateRequest);
        }

        public async Task ProcessJobAsync(CameraUpdateRequest cameraUpdateRequest)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var cameraFactory = scope.ServiceProvider.GetRequiredService<ICameraFactory>();

            _logger.LogInformation("Processing job for plate: {PlateNumber}", cameraUpdateRequest.LicensePlate);

            try
            {
                var cameraToUpdate = await unitOfWork.Cameras.FirstOrDefaultAsync(
                    x => x.Id == cameraUpdateRequest.Id);

                if (cameraToUpdate == null)
                {
                    _logger.LogError("Unable to find camera with ID: {CameraId}, check your configuration.", cameraUpdateRequest.Id);
                    throw new ArgumentException($"Unknown camera ID: {cameraUpdateRequest.Id}", nameof(cameraUpdateRequest));
                }

                if (!string.IsNullOrWhiteSpace(cameraToUpdate.NextClearOverlayScheduleId))
                {
                    _logger.LogInformation("Cancelling redundant clear overlay job: {JobId}", cameraToUpdate.NextClearOverlayScheduleId);
                    _backgroundJobService.DeleteJob(cameraToUpdate.NextClearOverlayScheduleId);
                }

                var camera = cameraFactory.Create(cameraToUpdate.Manufacturer, cameraToUpdate);

                await camera.SetCameraTextAsync(cameraUpdateRequest, default);

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

                await unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Completed processing job for plate: {PlateNumber}", cameraUpdateRequest.LicensePlate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing job for plate: {PlateNumber}", cameraUpdateRequest.LicensePlate);
                throw;
            }
        }

        public async Task<ZoomFocus> GetZoomAndFocusAsync(
            Guid cameraId,
            CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var cameraFactory = scope.ServiceProvider.GetRequiredService<ICameraFactory>();

            var dbCamera = await unitOfWork.Cameras.FirstOrDefaultAsync(
                x => x.Id == cameraId,
                cancellationToken);

            if (dbCamera == null)
            {
                throw new ArgumentException($"Camera not found: {cameraId}", nameof(cameraId));
            }

            var camera = cameraFactory.Create(
                dbCamera.Manufacturer,
                dbCamera);

            return await camera.GetZoomAndFocusAsync(cancellationToken);
        }

        public async Task SetZoomAndFocusAsync(
            Guid cameraId,
            ZoomFocus zoomAndFocus,
            CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var cameraFactory = scope.ServiceProvider.GetRequiredService<ICameraFactory>();

            var dbCamera = await unitOfWork.Cameras.FirstOrDefaultAsync(
                x => x.Id == cameraId,
                cancellationToken);

            if (dbCamera == null)
            {
                throw new ArgumentException($"Camera not found: {cameraId}", nameof(cameraId));
            }

            var camera = cameraFactory.Create(
                dbCamera.Manufacturer,
                dbCamera);

            await camera.SetZoomAndFocusAsync(zoomAndFocus, cancellationToken);
        }

        public async Task<bool> TriggerAutofocusAsync(
            Guid cameraId,
            CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var cameraFactory = scope.ServiceProvider.GetRequiredService<ICameraFactory>();

            var dbCamera = await unitOfWork.Cameras.FirstOrDefaultAsync(
                x => x.Id == cameraId,
                cancellationToken);

            if (dbCamera == null)
            {
                throw new ArgumentException($"Camera not found: {cameraId}", nameof(cameraId));
            }

            var camera = cameraFactory.Create(
                dbCamera.Manufacturer,
                dbCamera);

            return await camera.TriggerAutoFocusAsync(cancellationToken);
        }

        public async Task ForceClearOverlaysAsync(CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
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
                    // Continue processing other cameras
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}