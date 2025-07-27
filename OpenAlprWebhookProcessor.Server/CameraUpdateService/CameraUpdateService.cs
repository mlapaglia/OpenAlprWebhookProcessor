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

        public async Task DeleteSunriseSunsetAsync(
            Guid cameraId,
            CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var cameraToUpdate = await unitOfWork.Cameras.FirstOrDefaultAsync(
                x => x.Id == cameraId,
                cancellationToken);

            if (cameraToUpdate == null)
            {
                throw new ArgumentException($"Camera not found: {cameraId}", nameof(cameraId));
            }

            if (!string.IsNullOrWhiteSpace(cameraToUpdate.NextDayNightScheduleId))
            {
                _backgroundJobService.DeleteJob(cameraToUpdate.NextDayNightScheduleId);

                cameraToUpdate.NextDayNightScheduleId = string.Empty;
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task ProcessSunriseSunsetJobAsync(
            Guid cameraId,
            SunriseSunset sunriseSunset,
            bool scheduleNextJob,
            CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var cameraFactory = scope.ServiceProvider.GetRequiredService<ICameraFactory>();

            _logger.LogInformation("Setting {SunriseSunset} for camera {CameraId}", sunriseSunset, cameraId);

            var cameraToUpdate = await unitOfWork.Cameras.FirstOrDefaultAsync(
                x => x.Id == cameraId,
                cancellationToken);

            if (cameraToUpdate == null)
            {
                throw new ArgumentException($"Camera not found: {cameraId}", nameof(cameraId));
            }

            var camera = cameraFactory.Create(
                cameraToUpdate.Manufacturer,
                cameraToUpdate);

            try
            {
                await camera.TriggerDayNightModeAsync(
                    sunriseSunset,
                    cancellationToken);

                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process day/night job: {Message}", ex.Message);
            }

            if (scheduleNextJob)
            {
                _logger.LogInformation("Scheduling additional sunrise/sunset tasks after completing task for {CameraId}", cameraId);
                await CameraScheduling.ScheduleDayNightTasksAsync(
                    unitOfWork,
                    _backgroundJobService,
                    cancellationToken);
            }
        }

        public async Task ClearExpiredOverlayAsync(
            Guid cameraId,
            CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var cameraFactory = scope.ServiceProvider.GetRequiredService<ICameraFactory>();

            _logger.LogInformation("Clearing expired overlay for camera {CameraId}", cameraId);

            var cameraToUpdate = await unitOfWork.Cameras.FirstOrDefaultAsync(
                x => x.Id == cameraId,
                cancellationToken);

            if (cameraToUpdate == null)
            {
                throw new ArgumentException($"Camera not found: {cameraId}", nameof(cameraId));
            }

            try
            {
                var camera = cameraFactory.Create(cameraToUpdate.Manufacturer, cameraToUpdate);

                await camera.ClearCameraTextAsync(cancellationToken);

                cameraToUpdate.NextClearOverlayScheduleId = string.Empty;
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing expired overlay for camera {CameraId}", cameraId);
                throw;
            }
        }

        public async Task ScheduleDayNightTaskAsync(CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await CameraScheduling.ScheduleDayNightTasksAsync(unitOfWork, _backgroundJobService, cancellationToken);
        }

        public async Task EnqueueDayNightAsync(
            Guid cameraId,
            SunriseSunset sunriseSunset,
            CancellationToken cancellationToken = default)
        {
            await CameraScheduling.ExecuteSingleDayNightTaskAsync(
                sunriseSunset,
                cameraId,
                _backgroundJobService,
                cancellationToken);
        }

        public async Task ScheduleOverlayRequestAsync(
            CameraUpdateRequest cameraUpdateRequest,
            CancellationToken cancellationToken = default)
        {
            await _backgroundJobService.EnqueueProcessJobAsync(
                cameraUpdateRequest,
                cancellationToken);
        }

        public async Task ProcessJobAsync(
            CameraUpdateRequest cameraUpdateRequest,
            CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var cameraFactory = scope.ServiceProvider.GetRequiredService<ICameraFactory>();

            _logger.LogInformation("Processing job for plate: {PlateNumber}", cameraUpdateRequest.LicensePlate);

            try
            {
                var cameraToUpdate = await unitOfWork.Cameras.FirstOrDefaultAsync(
                    x => x.Id == cameraUpdateRequest.Id,
                    cancellationToken);

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

                await camera.SetCameraTextAsync(cameraUpdateRequest, cancellationToken);

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

                await unitOfWork.SaveChangesAsync(cancellationToken);

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
            CancellationToken cancellationToken = default)
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
            CancellationToken cancellationToken = default)
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

            await camera.SetZoomAndFocusAsync(
                zoomAndFocus,
                cancellationToken);
        }

        public async Task<bool> TriggerAutofocusAsync(
            Guid cameraId,
            CancellationToken cancellationToken = default)
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
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}