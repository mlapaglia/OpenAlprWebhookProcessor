using CoordinateSharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Cameras;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.CameraUpdateService
{
    public class SimpleCameraScheduler : ISimpleCameraScheduler, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger<SimpleCameraScheduler> _logger;

        private readonly ConcurrentDictionary<Guid, Timer> _cameraTimers = new();

        private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _cancellationTokens = new();

        private readonly ConcurrentDictionary<Guid, DateTimeOffset> _nextExecutionTimes = new();

        private readonly ConcurrentDictionary<string, Timer> _overlayTimers = new();

        private readonly ConcurrentDictionary<string, CancellationTokenSource> _overlayCancellationTokens = new();

        private readonly ConcurrentDictionary<string, DateTimeOffset> _overlayExecutionTimes = new();

        private bool _disposed = false;

        public SimpleCameraScheduler(
            IServiceProvider serviceProvider,
            ILogger<SimpleCameraScheduler> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting simple camera scheduler");

            await ResetAllCamerasAsync(cancellationToken);
            await ScheduleAllCamerasAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Stopping simple camera scheduler");

            foreach (var kvp in _cancellationTokens)
            {
                kvp.Value.Cancel();
            }

            foreach (var kvp in _overlayCancellationTokens)
            {
                kvp.Value.Cancel();
            }

            foreach (var kvp in _cameraTimers)
            {
                kvp.Value?.Dispose();
            }

            foreach (var kvp in _overlayTimers)
            {
                kvp.Value?.Dispose();
            }

            _cameraTimers.Clear();
            _cancellationTokens.Clear();
            _overlayTimers.Clear();
            _overlayCancellationTokens.Clear();
            _overlayExecutionTimes.Clear();
        }

        public async Task ResetAllCamerasAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Resetting all cameras to correct day/night mode");

            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var cameraFactory = scope.ServiceProvider.GetRequiredService<ICameraFactory>();

            var cameras = await unitOfWork.Cameras.FindAsync(x => x.UpdateDayNightModeEnabled, cancellationToken);
            var agent = await unitOfWork.Agents.GetFirstAgentAsync(cancellationToken);

            using var semaphore = new SemaphoreSlim(5, 5);
            var tasks = cameras.Select(async cameraData =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var currentMode = DetermineCurrentDayNightMode(agent, cameraData);
                    var camera = cameraFactory.Create(cameraData.Manufacturer, cameraData);

                    var modeDescription = currentMode == SunriseSunset.Sunrise ? "Day" : "Night";
                    _logger.LogInformation("Setting camera {CameraId} to {Mode} mode on startup", 
                        cameraData.Id, modeDescription);

                    await camera.TriggerDayNightModeAsync(currentMode, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to reset camera {CameraId} on startup: {Message}", cameraData.Id, ex.Message);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
        }

        public async Task ScheduleCameraAsync(
            Guid cameraId,
            CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var cameraData = await unitOfWork.Cameras.FirstOrDefaultAsync(x => x.Id == cameraId, cancellationToken);
            if (cameraData == null || !cameraData.UpdateDayNightModeEnabled)
            {
                _logger.LogWarning("Camera {CameraId} not found or day/night mode disabled", cameraId);
                return;
            }

            var agent = await unitOfWork.Agents.GetFirstAgentAsync(cancellationToken);
            ScheduleCameraInternal(agent, cameraData);
        }

        public Task RemoveCameraScheduleAsync(
            Guid cameraId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Removing camera {CameraId} from scheduling", cameraId);
            ClearCameraTimer(cameraId);
            return Task.CompletedTask;
        }

        public async Task RescheduleAllCamerasAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Rescheduling all cameras due to agent settings change");

            foreach (var kvp in _cameraTimers.ToList())
            {
                ClearCameraTimer(kvp.Key);
            }

            await ScheduleAllCamerasAsync(cancellationToken);
        }

        private async Task ScheduleAllCamerasAsync(CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var cameras = await unitOfWork.Cameras.FindAsync(x => x.UpdateDayNightModeEnabled, cancellationToken);
            var agent = await unitOfWork.Agents.GetFirstAgentAsync(cancellationToken);

            foreach (var camera in cameras)
            {
                ScheduleCameraInternal(
                    agent,
                    camera);
            }
        }

        private void ScheduleCameraInternal(
            Data.Agent agent,
            Data.Camera camera)
        {
            try
            {
                var timeZoneOffset = camera.TimezoneOffset ?? agent.TimeZoneOffset;
                var latitude = camera.Latitude ?? agent.Latitude;
                var longitude = camera.Longitude ?? agent.Longitude;

                if (!latitude.HasValue || !longitude.HasValue)
                {
                    _logger.LogWarning("Camera {CameraId} has no location data, skipping scheduling", camera.Id);
                    return;
                }

                var nextToggleTime = CalculateNextToggleTime(
                    latitude.Value,
                    longitude.Value,
                    timeZoneOffset,
                    camera.SunriseOffset ?? agent.SunriseOffset,
                    camera.SunsetOffset ?? agent.SunsetOffset);

                if (nextToggleTime.HasValue)
                {
                    ScheduleCameraTimer(camera.Id, nextToggleTime.Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to schedule camera {CameraId}", camera.Id);
            }
        }

        private void ScheduleCameraTimer(
            Guid cameraId,
            DateTimeOffset toggleTime)
        {
            ClearCameraTimer(cameraId);

            var delay = toggleTime - DateTimeOffset.Now;
            if (delay <= TimeSpan.Zero)
            {
                _logger.LogWarning("Next toggle time for camera {CameraId} is in the past (calculated: {ToggleTime}, current: {CurrentTime}, delay: {Delay}), skipping", 
                    cameraId, toggleTime, DateTimeOffset.Now, delay);
                return;
            }

            _logger.LogInformation("Scheduling camera {CameraId} to toggle at {ToggleTime} (in {Delay})", 
                cameraId, toggleTime, delay);

            _nextExecutionTimes[cameraId] = toggleTime;
            var cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokens[cameraId] = cancellationTokenSource;

            var timer = new Timer(async _ =>
            {
                if (cancellationTokenSource.Token.IsCancellationRequested) return;

                try
                {
                    _nextExecutionTimes.TryRemove(cameraId, out var whatever);
                    await ExecuteCameraToggleAsync(cameraId, cancellationTokenSource.Token);
                    await RescheduleCameraAsync(cameraId, cancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during camera {CameraId} toggle", cameraId);
                }
            }, null, delay, Timeout.InfiniteTimeSpan);

            _cameraTimers[cameraId] = timer;
        }

        private async Task ExecuteCameraToggleAsync(
            Guid cameraId,
            CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var cameraFactory = scope.ServiceProvider.GetRequiredService<ICameraFactory>();

            var cameraData = await unitOfWork.Cameras.FirstOrDefaultAsync(x => x.Id == cameraId, cancellationToken);
            if (cameraData == null || !cameraData.UpdateDayNightModeEnabled)
            {
                _logger.LogWarning("Camera {CameraId} not found or day/night mode disabled", cameraId);
                return;
            }

            var agent = await unitOfWork.Agents.GetFirstAgentAsync(cancellationToken);
            var currentMode = DetermineCurrentDayNightMode(agent, cameraData);
            var camera = cameraFactory.Create(cameraData.Manufacturer, cameraData);

            var modeDescription = currentMode == SunriseSunset.Sunrise ? "Day" : "Night";
            _logger.LogInformation("Toggling camera {CameraId} to {Mode} mode", cameraId, modeDescription);

            await camera.TriggerDayNightModeAsync(currentMode, cancellationToken);
        }

        private async Task RescheduleCameraAsync(
            Guid cameraId,
            CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var cameraData = await unitOfWork.Cameras.FirstOrDefaultAsync(x => x.Id == cameraId, cancellationToken);
            if (cameraData == null || !cameraData.UpdateDayNightModeEnabled)
            {
                return;
            }

            var agent = await unitOfWork.Agents.GetFirstAgentAsync(cancellationToken);
            ScheduleCameraInternal(agent, cameraData);
        }

        private static SunriseSunset DetermineCurrentDayNightMode(
            Data.Agent agent,
            Data.Camera camera)
        {
            var timeZoneOffset = camera.TimezoneOffset ?? agent.TimeZoneOffset;
            var latitude = camera.Latitude ?? agent.Latitude;
            var longitude = camera.Longitude ?? agent.Longitude;

            if (!latitude.HasValue || !longitude.HasValue)
            {
                return SunriseSunset.Sunrise;
            }

            var currentTimeInTimezone = DateTimeOffset.UtcNow.AddHours(timeZoneOffset).DateTime;

            var celestialTimes = Celestial.CalculateCelestialTimes(
                latitude.Value,
                longitude.Value,
                currentTimeInTimezone,
                timeZoneOffset);

            return celestialTimes.IsSunUp ? SunriseSunset.Sunrise : SunriseSunset.Sunset;
        }

        private static DateTimeOffset? CalculateNextToggleTime(
            double latitude,
            double longitude,
            double timeZoneOffset,
            int sunriseOffset,
            int sunsetOffset)
        {
            var currentTimeInTimezone = DateTimeOffset.UtcNow.AddHours(timeZoneOffset).DateTime;
            
            // Add a small buffer to avoid race conditions at transition times
            var lookAheadTime = currentTimeInTimezone.AddMinutes(1);

            var celestialTimes = Celestial.CalculateCelestialTimes(
                latitude,
                longitude,
                lookAheadTime,
                timeZoneOffset);

            DateTime nextToggleTime;
            if (celestialTimes.IsSunUp)
            {
                var nextSunset = Celestial.Get_Next_SunSet(
                    latitude,
                    longitude,
                    lookAheadTime,
                    timeZoneOffset);
                nextToggleTime = nextSunset.AddMinutes(sunsetOffset);
            }
            else
            {
                var nextSunrise = Celestial.Get_Next_SunRise(
                    latitude,
                    longitude,
                    lookAheadTime,
                    timeZoneOffset);
                nextToggleTime = nextSunrise.AddMinutes(sunriseOffset);
            }

            var calculatedTime = new DateTimeOffset(nextToggleTime, TimeSpan.FromHours(timeZoneOffset));
            
            // Ensure the calculated time is at least 30 seconds in the future to avoid immediate past times
            var minimumFutureTime = DateTimeOffset.Now.AddSeconds(30);
            if (calculatedTime <= minimumFutureTime)
            {
                // If calculated time is too close or in the past, try calculating for tomorrow
                var tomorrowTime = lookAheadTime.AddDays(1);
                var tomorrowCelestialTimes = Celestial.CalculateCelestialTimes(
                    latitude,
                    longitude,
                    tomorrowTime,
                    timeZoneOffset);

                if (tomorrowCelestialTimes.IsSunUp)
                {
                    var nextSunset = Celestial.Get_Next_SunSet(
                        latitude,
                        longitude,
                        tomorrowTime,
                        timeZoneOffset);
                    nextToggleTime = nextSunset.AddMinutes(sunsetOffset);
                }
                else
                {
                    var nextSunrise = Celestial.Get_Next_SunRise(
                        latitude,
                        longitude,
                        tomorrowTime,
                        timeZoneOffset);
                    nextToggleTime = nextSunrise.AddMinutes(sunriseOffset);
                }
                
                calculatedTime = new DateTimeOffset(nextToggleTime, TimeSpan.FromHours(timeZoneOffset));
            }

            return calculatedTime;
        }

        public DateTimeOffset? GetNextScheduledExecutionTime(Guid cameraId)
        {
            return _nextExecutionTimes.TryGetValue(cameraId, out var executionTime) ? executionTime : null;
        }

        public List<ScheduledJobInfo> GetAllScheduledJobs()
        {
            var jobs = new List<ScheduledJobInfo>();

            foreach (var kvp in _nextExecutionTimes)
            {
                var cameraId = kvp.Key;
                var executionTime = kvp.Value;

                jobs.Add(new ScheduledJobInfo
                {
                    JobId = cameraId.ToString(),
                    ScheduledExecutionTime = executionTime,
                    JobType = ScheduledJobType.SunriseSunset,
                    CameraId = cameraId,
                    SunriseSunsetType = DetermineNextSunriseSunsetType(cameraId)
                });
            }

            foreach (var kvp in _overlayExecutionTimes)
            {
                var jobId = kvp.Key;
                var executionTime = kvp.Value;
                var cameraId = ExtractCameraIdFromOverlayJobId(jobId);

                jobs.Add(new ScheduledJobInfo
                {
                    JobId = jobId,
                    ScheduledExecutionTime = executionTime,
                    JobType = ScheduledJobType.ClearOverlay,
                    CameraId = cameraId
                });
            }

            return jobs.OrderBy(j => j.ScheduledExecutionTime).ToList();
        }

        public async Task ScheduleOverlayAsync(
            CameraUpdateRequest cameraUpdateRequest,
            CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var cameraFactory = scope.ServiceProvider.GetRequiredService<ICameraFactory>();

            _logger.LogInformation("Processing overlay job for plate: {PlateNumber}", cameraUpdateRequest.LicensePlate);

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

                ClearExistingOverlayJob(cameraToUpdate.NextClearOverlayScheduleId);

                var camera = cameraFactory.Create(cameraToUpdate.Manufacturer, cameraToUpdate);
                await camera.SetCameraTextAsync(cameraUpdateRequest, cancellationToken);

                var jobId = ScheduleOverlayClearJob(cameraUpdateRequest.Id, TimeSpan.FromSeconds(5));
                cameraToUpdate.NextClearOverlayScheduleId = jobId;

                if (!cameraUpdateRequest.IsTest
                    && !cameraUpdateRequest.IsPreviewGroup
                    && !cameraUpdateRequest.IsSinglePlate)
                {
                    cameraToUpdate.PlatesSeen++;
                    cameraToUpdate.LatestProcessedPlateUuid = cameraUpdateRequest.LicensePlateImageUuid;
                }

                await unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Completed processing overlay job for plate: {PlateNumber}", cameraUpdateRequest.LicensePlate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing overlay job for plate: {PlateNumber}", cameraUpdateRequest.LicensePlate);
                throw;
            }
        }

        public async Task ClearOverlayAsync(
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

        private string ScheduleOverlayClearJob(
            Guid cameraId,
            TimeSpan delay)
        {
            if (cameraId == Guid.Empty || delay < TimeSpan.Zero) return null;

            var jobId = $"overlay-{cameraId}-{Guid.NewGuid()}";
            var cancellationTokenSource = new CancellationTokenSource();
            _overlayCancellationTokens[jobId] = cancellationTokenSource;
            var executionTime = DateTimeOffset.Now.Add(delay);
            _overlayExecutionTimes[jobId] = executionTime;

            var timer = new Timer(async _ =>
            {
                if (cancellationTokenSource.Token.IsCancellationRequested) return;

                try
                {
                    await ClearOverlayAsync(cameraId, cancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error clearing overlay for camera {CameraId}", cameraId);
                }
                finally
                {
                    ClearOverlayJob(jobId);
                }
            }, null, delay, Timeout.InfiniteTimeSpan);

            _overlayTimers[jobId] = timer;
            return jobId;
        }

        private void ClearExistingOverlayJob(string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId)) return;

            _logger.LogInformation("Cancelling redundant clear overlay job: {JobId}", jobId);
            ClearOverlayJob(jobId);
        }

        private void ClearOverlayJob(string jobId)
        {
            if (_overlayCancellationTokens.TryRemove(jobId, out var cancellationTokenSource))
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            }

            if (_overlayTimers.TryRemove(jobId, out var timer))
            {
                timer?.Dispose();
            }

            _overlayExecutionTimes.TryRemove(jobId, out _);
        }

        private static Guid? ExtractCameraIdFromOverlayJobId(string jobId)
        {
            if (string.IsNullOrEmpty(jobId) || !jobId.StartsWith("overlay-")) return null;

            var prefixLength = "overlay-".Length;
            var remainingPart = jobId.Substring(prefixLength);

            if (remainingPart.Length >= 36)
            {
                var potentialCameraId = remainingPart.Substring(0, 36);
                if (Guid.TryParse(potentialCameraId, out var cameraId))
                {
                    return cameraId;
                }
            }

            return null;
        }

        public async Task ExecuteDayNightModeAsync(
            Guid cameraId,
            SunriseSunset sunriseSunset,
            CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var cameraFactory = scope.ServiceProvider.GetRequiredService<ICameraFactory>();

            var modeDescription = sunriseSunset == SunriseSunset.Sunrise ? "Day" : "Night";
            _logger.LogInformation("Setting {Mode} mode for camera {CameraId}", modeDescription, cameraId);

            var cameraToUpdate = await unitOfWork.Cameras.FirstOrDefaultAsync(
                x => x.Id == cameraId,
                cancellationToken);

            if (cameraToUpdate == null)
            {
                throw new ArgumentException($"Camera not found: {cameraId}", nameof(cameraId));
            }

            var camera = cameraFactory.Create(cameraToUpdate.Manufacturer, cameraToUpdate);
            await camera.TriggerDayNightModeAsync(sunriseSunset, cancellationToken);
        }

        public async Task SetZoomAndFocusAsync(
            Guid cameraId,
            ZoomFocus zoomAndFocus,
            CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var cameraFactory = scope.ServiceProvider.GetRequiredService<ICameraFactory>();

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

            await camera.SetZoomAndFocusAsync(
                zoomAndFocus,
                cancellationToken);
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

            var camera = cameraFactory.Create(dbCamera.Manufacturer, dbCamera);
            return await camera.TriggerAutoFocusAsync(cancellationToken);
        }

        private SunriseSunset? DetermineNextSunriseSunsetType(Guid cameraId)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var cameraData = unitOfWork.Cameras.FirstOrDefaultAsync(x => x.Id == cameraId).Result;
                if (cameraData == null) return null;

                var agent = unitOfWork.Agents.GetFirstAgentAsync().Result;
                if (agent == null) return null;

                var timeZoneOffset = cameraData.TimezoneOffset ?? agent.TimeZoneOffset;
                var latitude = cameraData.Latitude ?? agent.Latitude;
                var longitude = cameraData.Longitude ?? agent.Longitude;

                if (!latitude.HasValue || !longitude.HasValue) return null;

                var currentTimeInTimezone = DateTimeOffset.UtcNow.AddHours(timeZoneOffset).DateTime;
                var celestialTimes = Celestial.CalculateCelestialTimes(
                    latitude.Value,
                    longitude.Value,
                    currentTimeInTimezone,
                    timeZoneOffset);

                return celestialTimes.IsSunUp ? SunriseSunset.Sunset : SunriseSunset.Sunrise;
            }
            catch
            {
                return null;
            }
        }

        private void ClearCameraTimer(Guid cameraId)
        {
            if (_cancellationTokens.TryRemove(cameraId, out var cancellationTokenSource))
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            }

            if (_cameraTimers.TryRemove(cameraId, out var timer))
            {
                timer?.Dispose();
            }

            _nextExecutionTimes.TryRemove(cameraId, out _);
        }

        public void Dispose()
        {
            if (_disposed) return;

            foreach (var kvp in _cancellationTokens)
            {
                kvp.Value?.Cancel();
                kvp.Value?.Dispose();
            }

            foreach (var kvp in _overlayCancellationTokens)
            {
                kvp.Value?.Cancel();
                kvp.Value?.Dispose();
            }

            foreach (var kvp in _cameraTimers)
            {
                kvp.Value?.Dispose();
            }

            foreach (var kvp in _overlayTimers)
            {
                kvp.Value?.Dispose();
            }

            _cameraTimers.Clear();
            _cancellationTokens.Clear();
            _nextExecutionTimes.Clear();
            _overlayTimers.Clear();
            _overlayCancellationTokens.Clear();
            _overlayExecutionTimes.Clear();

            _disposed = true;
        }
    }
}
