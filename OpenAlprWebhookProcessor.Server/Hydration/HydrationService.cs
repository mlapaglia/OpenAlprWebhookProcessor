using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Hydrator
{
    public class HydrationService : IHydrationService, IDisposable
    {
        private readonly Channel<string> _hydrationRequestsChannel;
        private readonly ChannelWriter<string> _writer;
        private readonly ChannelReader<string> _reader;

        private readonly IServiceProvider _serviceProvider;

        private Timer _scheduledScrapeTimer;

        private readonly Lock _timerLock = new Lock();

        // Track current configuration to avoid unnecessary timer recreation
        private int? _currentIntervalMinutes;

        private string _currentAgentUid;

        private bool _disposed = false;

        public HydrationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            // Create an unbounded channel for hydration requests
            _hydrationRequestsChannel = Channel.CreateUnbounded<string>();
            _writer = _hydrationRequestsChannel.Writer;
            _reader = _hydrationRequestsChannel.Reader;
        }

        public void StartHydration(string name)
        {
            if (_disposed)
                return;

            // TryWrite returns false if the channel is closed
            if (!_writer.TryWrite(name))
            {
                using var scope = _serviceProvider.CreateScope();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<HydrationService>>();
                logger.LogWarning("Failed to queue hydration request for {Name} - channel may be closed", name);
            }
        }

        public int GetPendingHydrationCount()
        {
            return _reader.Count;
        }

        public async IAsyncEnumerable<string> GetConsumingHydrationRequestsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var request in _reader.ReadAllAsync(cancellationToken))
            {
                yield return request;
            }
        }

        public async Task ScheduleHydrationAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<HydrationService>>();

            var agent = await unitOfWork.Agents.GetFirstAgentAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(agent?.Uid))
            {
                logger.LogWarning("Agent UID is not set. Cannot schedule hydration.");
                DisposeTimer();
                return;
            }

            lock (_timerLock)
            {
                var configurationChanged = _currentIntervalMinutes != agent.ScheduledScrapingIntervalMinutes
                    || _currentAgentUid != agent.Uid;

                if (configurationChanged)
                {
                    _scheduledScrapeTimer?.Dispose();
                    _scheduledScrapeTimer = null;

                    _currentIntervalMinutes = agent.ScheduledScrapingIntervalMinutes;
                    _currentAgentUid = agent.Uid;

                    if (agent.ScheduledScrapingIntervalMinutes == null)
                    {
                        agent.NextScrapeEpochMs = null;
                    }
                    else
                    {
                        var nextExecution = DateTime.UtcNow.AddMinutes(agent.ScheduledScrapingIntervalMinutes.Value);
                        agent.NextScrapeEpochMs = new DateTimeOffset(nextExecution).ToUnixTimeMilliseconds();

                        _scheduledScrapeTimer = new Timer(
                            async _ =>
                            {
                                StartHydration(agent.Uid);
                                await UpdateNextScrapeTimeAsync(agent.Uid, agent.ScheduledScrapingIntervalMinutes.Value);
                            },
                            null,
                            TimeSpan.FromMinutes(agent.ScheduledScrapingIntervalMinutes.Value),
                            TimeSpan.FromMinutes(agent.ScheduledScrapingIntervalMinutes.Value)
                        );
                    }
                }
                else if (_scheduledScrapeTimer != null && agent.ScheduledScrapingIntervalMinutes.HasValue)
                {
                    var nextExecution = DateTime.UtcNow.AddMinutes(agent.ScheduledScrapingIntervalMinutes.Value);
                    agent.NextScrapeEpochMs = new DateTimeOffset(nextExecution).ToUnixTimeMilliseconds();
                }
            }

            unitOfWork.Agents.Update(agent);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public void DisposeTimer()
        {
            lock (_timerLock)
            {
                _scheduledScrapeTimer?.Dispose();
                _scheduledScrapeTimer = null;
                _currentIntervalMinutes = null;
                _currentAgentUid = null;
            }
        }

        /// <summary>
        /// Complete the channel to signal that no more items will be written.
        /// Call this when shutting down the service.
        /// </summary>
        public void CompleteChannel()
        {
            _writer.TryComplete();
        }

        private async Task UpdateNextScrapeTimeAsync(string agentUid, int intervalMinutes)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var agent = await unitOfWork.Agents.GetFirstAgentAsync();
                if (agent != null && agent.Uid == agentUid)
                {
                    var nextExecution = DateTime.UtcNow.AddMinutes(intervalMinutes);
                    agent.NextScrapeEpochMs = new DateTimeOffset(nextExecution).ToUnixTimeMilliseconds();

                    unitOfWork.Agents.Update(agent);
                    await unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                using var scope = _serviceProvider.CreateScope();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<HydrationService>>();
                logger.LogError(ex, "Error updating next scrape time for agent {AgentUid}", agentUid);
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            DisposeTimer();
            CompleteChannel();
            _disposed = true;
        }
    }

    // Extension method to convert IAsyncEnumerable to blocking IEnumerable (for backwards compatibility)
    public static class AsyncEnumerableExtensions
    {
        public static IEnumerable<T> ToBlockingEnumerable<T>(this IAsyncEnumerable<T> asyncEnumerable, CancellationToken cancellationToken = default)
        {
            var enumerator = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (true)
                {
                    var moveNextTask = enumerator.MoveNextAsync();
                    var hasNext = moveNextTask.IsCompletedSuccessfully
                        ? moveNextTask.Result
                        : moveNextTask.AsTask().GetAwaiter().GetResult();

                    if (!hasNext)
                        yield break;

                    yield return enumerator.Current;
                }
            }
            finally
            {
                var disposeTask = enumerator.DisposeAsync();
                if (!disposeTask.IsCompletedSuccessfully)
                    disposeTask.AsTask().GetAwaiter().GetResult();
            }
        }
    }
}