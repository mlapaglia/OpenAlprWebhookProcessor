using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Hydrator
{
    public class HydrationService : IHydrationService
    {
        private readonly BlockingCollection<string> _hydrationRequestsToProcess = new BlockingCollection<string>();

        private readonly IServiceProvider _serviceProvider;

        private Timer _scheduledScrapeTimer;

        private readonly object _timerLock = new object();

        // Track current configuration to avoid unnecessary timer recreation
        private int? _currentIntervalMinutes;

        private string _currentAgentUid;

        public HydrationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public void StartHydration(string name)
        {
            _hydrationRequestsToProcess.Add(name);
        }

        public int GetPendingHydrationCount()
        {
            return _hydrationRequestsToProcess.Count;
        }

        public IEnumerable<string> GetConsumingHydrationRequests(CancellationToken cancellationToken)
        {
            return _hydrationRequestsToProcess.GetConsumingEnumerable(cancellationToken);
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
    }
}