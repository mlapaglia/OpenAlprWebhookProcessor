using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.CameraUpdateService
{
    public class TimerBasedBackgroundJobService : IBackgroundJobService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger<TimerBasedBackgroundJobService> _logger;

        private readonly ConcurrentDictionary<string, Timer> _scheduledJobs = new();

        private readonly ConcurrentDictionary<string, CancellationTokenSource> _jobCancellationTokens = new();

        private bool _disposed = false;

        public TimerBasedBackgroundJobService(
            IServiceProvider serviceProvider,
            ILogger<TimerBasedBackgroundJobService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task EnqueueProcessJobAsync(CameraUpdateRequest request)
        {
            if (request == null) return;

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var cameraUpdateService = scope.ServiceProvider.GetRequiredService<ICameraUpdateService>();
                await cameraUpdateService.ProcessJobAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing camera update job for camera {CameraId}", request.Id);
            }
        }

        public async Task EnqueueProcessSunriseSunsetJobAsync(Guid cameraId, SunriseSunset sunriseSunset, bool scheduleNextJob)
        {
            if (cameraId == Guid.Empty) return;

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var cameraUpdateService = scope.ServiceProvider.GetRequiredService<ICameraUpdateService>();
                await cameraUpdateService.ProcessSunriseSunsetJobAsync(cameraId, sunriseSunset, scheduleNextJob);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing sunrise/sunset job for camera {CameraId}", cameraId);
            }
        }

        public string ScheduleClearOverlayJob(Guid cameraId, TimeSpan delay)
        {
            if (cameraId == Guid.Empty || delay < TimeSpan.Zero) return null;

            var jobId = Guid.NewGuid().ToString();
            var cancellationTokenSource = new CancellationTokenSource();
            _jobCancellationTokens[jobId] = cancellationTokenSource;

            var timer = new Timer(async _ =>
            {
                if (cancellationTokenSource.Token.IsCancellationRequested) return;

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var cameraUpdateService = scope.ServiceProvider.GetRequiredService<ICameraUpdateService>();
                    await cameraUpdateService.ClearExpiredOverlayAsync(cameraId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error clearing overlay for camera {CameraId}", cameraId);
                }
                finally
                {
                    if (_scheduledJobs.TryRemove(jobId, out var timerToDispose))
                    {
                        timerToDispose?.Dispose();
                    }
                    if (_jobCancellationTokens.TryRemove(jobId, out var tokenSource))
                    {
                        tokenSource?.Dispose();
                    }
                }
            }, null, delay, Timeout.InfiniteTimeSpan);

            _scheduledJobs[jobId] = timer;
            return jobId;
        }

        public async Task<string> ScheduleProcessSunriseSunsetJobAsync(
            Guid cameraId,
            SunriseSunset sunriseSunset,
            bool scheduleNextJob,
            DateTimeOffset scheduleAt)
        {
            if (cameraId == Guid.Empty) return null;

            var delay = scheduleAt - DateTimeOffset.Now;
            if (delay <= TimeSpan.Zero)
            {
                await EnqueueProcessSunriseSunsetJobAsync(cameraId, sunriseSunset, scheduleNextJob);
                return null;
            }

            var jobId = Guid.NewGuid().ToString();
            var cancellationTokenSource = new CancellationTokenSource();
            _jobCancellationTokens[jobId] = cancellationTokenSource;

            var timer = new Timer(async _ =>
            {
                if (cancellationTokenSource.Token.IsCancellationRequested) return;

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var cameraUpdateService = scope.ServiceProvider.GetRequiredService<ICameraUpdateService>();
                    await cameraUpdateService.ProcessSunriseSunsetJobAsync(cameraId, sunriseSunset, scheduleNextJob);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing scheduled sunrise/sunset job for camera {CameraId}", cameraId);
                }
                finally
                {
                    if (_scheduledJobs.TryRemove(jobId, out var timerToDispose))
                    {
                        timerToDispose?.Dispose();
                    }
                    if (_jobCancellationTokens.TryRemove(jobId, out var tokenSource))
                    {
                        tokenSource?.Dispose();
                    }
                }
            }, null, delay, Timeout.InfiniteTimeSpan);

            _scheduledJobs[jobId] = timer;
            return jobId;
        }

        public void DeleteJob(string jobId)
        {
            if (string.IsNullOrEmpty(jobId)) return;

            if (_jobCancellationTokens.TryRemove(jobId, out var cancellationTokenSource))
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            }

            if (_scheduledJobs.TryRemove(jobId, out var timer))
            {
                timer?.Dispose();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            foreach (var kvp in _jobCancellationTokens)
            {
                kvp.Value?.Cancel();
                kvp.Value?.Dispose();
            }
            _jobCancellationTokens.Clear();

            foreach (var kvp in _scheduledJobs)
            {
                kvp.Value?.Dispose();
            }
            _scheduledJobs.Clear();

            _disposed = true;
        }
    }
} 