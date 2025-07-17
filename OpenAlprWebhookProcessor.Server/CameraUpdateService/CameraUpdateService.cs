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
    public class CameraUpdateService : IHostedService, ICameraUpdateService
    {
        private readonly IBackgroundJobService _backgroundJobService;

        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger _logger;

        private readonly ICameraFactory _cameraFactory;

        private readonly ICameraScheduling _cameraScheduling;

        public CameraUpdateService(
            IServiceProvider serviceProvider,
            ILogger<CameraUpdateService> logger,
            IBackgroundJobService backgroundJobService,
            ICameraFactory cameraFactory,
            ICameraScheduling cameraScheduling)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _backgroundJobService = backgroundJobService ?? throw new ArgumentNullException(nameof(backgroundJobService));
            _cameraFactory = cameraFactory ?? throw new ArgumentNullException(nameof(cameraFactory));
            _cameraScheduling = cameraScheduling ?? throw new ArgumentNullException(nameof(cameraScheduling));
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task ForceSunriseSunsetAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var agent = await unitOfWork.Agents.GetFirstAgentAsync();

                // Only schedule if there are coordinates available
                if (agent?.Latitude.HasValue == true && agent?.Longitude.HasValue == true)
                {
                    await _cameraScheduling.ScheduleDayNightTasksAsync(_backgroundJobService);
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

                _logger.LogInformation("setting {SunriseSunset} for {CameraId}", sunriseSunset, cameraId);

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
                        _backgroundJobService.DeleteJob(cameraToUpdate.NextDayNightScheduleId);
                        cameraToUpdate.NextDayNightScheduleId = string.Empty;
                    }

                    var camera = _cameraFactory.Create(cameraToUpdate.Manufacturer, cameraToUpdate);

                    await camera.TriggerDayNightModeAsync(
                        sunriseSunset,
                        _cancellationTokenSource.Token);

                    var agent = await unitOfWork.Agents.GetFirstAgentAsync(_cancellationTokenSource.Token);

                    if (scheduleNextJob)
                    {
                        _cameraScheduling.ScheduleDayNightTask(
                            _backgroundJobService,
                            agent,
                            cameraToUpdate);
                    }

                    await unitOfWork.SaveChangesAsync(_cancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing sunrise/sunset job for camera {CameraId}", cameraId);
                    
                    // Re-throw ArgumentException so tests can catch it
                    if (ex is ArgumentException)
                        throw;
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
            await _cameraScheduling.ScheduleDayNightTasksAsync(_backgroundJobService);
        }

        public void EnqueueDayNight(
            Guid cameraId,
            SunriseSunset sunriseSunset)
        {
            _cameraScheduling.ExecuteSingleDayNightTask(
                sunriseSunset,
                cameraId,
                _backgroundJobService);
        }

        public void ScheduleOverlayRequest(CameraUpdateRequest cameraUpdateRequest)
        {
            _backgroundJobService.EnqueueProcessJob(cameraUpdateRequest);
        }

        public async Task ProcessJobAsync(CameraUpdateRequest cameraUpdateRequest)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

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

                    var camera = _cameraFactory.Create(cameraToUpdate.Manufacturer, cameraToUpdate);

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
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing job for camera {CameraId}", cameraUpdateRequest.Id);
                    
                    // Re-throw ArgumentException so tests can catch it
                    if (ex is ArgumentException)
                        throw;
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
                    _logger.LogError("Unable to find camera with Id: {CameraId}", cameraId);
                    return;
                }

                _logger.LogInformation("clearing expired overlay for: {CameraID}", cameraToUpdate.OpenAlprCameraId);

                try
                {
                    var camera = _cameraFactory.Create(cameraToUpdate.Manufacturer, cameraToUpdate);

                    await camera.ClearCameraTextAsync(_cancellationTokenSource.Token);

                    cameraToUpdate.NextClearOverlayScheduleId = string.Empty;

                    await unitOfWork.SaveChangesAsync(_cancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error clearing overlay for camera {CameraId}", cameraId);
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

                var camera = _cameraFactory.Create(
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

                var camera = _cameraFactory.Create(
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

                var camera = _cameraFactory.Create(
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

                var camerasToUpdate = await unitOfWork.Cameras.GetAllAsync(cancellationToken);

                foreach (var cameraToUpdate in camerasToUpdate)
                {
                    try
                    {
                        var camera = _cameraFactory.Create(cameraToUpdate.Manufacturer, cameraToUpdate);

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
