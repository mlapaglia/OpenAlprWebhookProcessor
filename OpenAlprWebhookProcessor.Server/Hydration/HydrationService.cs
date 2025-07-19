using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprAgentScraper;
using System;
using Microsoft.Extensions.DependencyInjection;
using OpenAlprWebhookProcessor.ProcessorHub;
using Microsoft.AspNetCore.SignalR;
using OpenAlprWebhookProcessor.Data.Repositories;

namespace OpenAlprWebhookProcessor.Hydrator
{
    public class HydrationService : IHostedService, IHydrationService
    {
        private readonly BlockingCollection<string> _hydrationRequestsToProcess;

        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly IServiceProvider _serviceProvider;

        private readonly IHubContext<ProcessorHub.ProcessorHub, IProcessorHub> _processorHub;

        private Timer _scheduledScrapeTimer;

        private readonly Lock _timerLock = new Lock();

        public HydrationService(
            IServiceProvider serviceProvider,
            IHubContext<ProcessorHub.ProcessorHub, IProcessorHub> processorHub)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _processorHub = processorHub ?? throw new ArgumentNullException(nameof(processorHub));
            _cancellationTokenSource = new CancellationTokenSource();
            _hydrationRequestsToProcess = new BlockingCollection<string>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await ScheduleHydrationAsync(cancellationToken);

            _ = Task.Run(() => StartHydrationAsync(), cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }
            
            lock (_timerLock)
            {
                _scheduledScrapeTimer?.Dispose();
                _scheduledScrapeTimer = null;
            }
            
            _cancellationTokenSource.Dispose();

            return Task.CompletedTask;
        }

        public async Task ScheduleHydrationAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var agent = await unitOfWork.Agents.GetFirstAgentAsync(cancellationToken);

                if (string.IsNullOrWhiteSpace(agent.Uid))
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<HydrationService>>();
                    logger.LogWarning("Agent UID is not set. Cannot schedule hydration.");
                    return;
                }

                lock (_timerLock)
                {
                    _scheduledScrapeTimer?.DisposeAsync();
                    _scheduledScrapeTimer = null;

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
                                if (!_cancellationTokenSource.Token.IsCancellationRequested)
                                {
                                    StartHydration(agent.Uid);
                                    await UpdateNextScrapeTimeAsync(agent.Uid, agent.ScheduledScrapingIntervalMinutes.Value);
                                }
                            },
                            null,
                            TimeSpan.FromMinutes(agent.ScheduledScrapingIntervalMinutes.Value),
                            TimeSpan.FromMinutes(agent.ScheduledScrapingIntervalMinutes.Value)
                        );
                    }
                }

                unitOfWork.Agents.Update(agent);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        private async Task UpdateNextScrapeTimeAsync(
            string agentUid,
            int intervalMinutes)
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

        public void StartHydration(string name)
        {
            _hydrationRequestsToProcess.Add(name);
        }

        private async Task StartHydrationAsync()
        {
            foreach (var _ in _hydrationRequestsToProcess.GetConsumingEnumerable(_cancellationTokenSource.Token))
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var logger = scope.ServiceProvider.GetRequiredService<ILogger<HydrationService>>();
                        logger.LogInformation("Starting OpenALPR Agent scrape.");

                        try
                        {
                            var scraper = scope.ServiceProvider.GetRequiredService<IOpenAlprAgentScraper>();

                            await scraper.ScrapeAgentAsync(_cancellationTokenSource.Token);
                            await scraper.ScrapeAgentImagesAsync(_cancellationTokenSource.Token);

                            await _processorHub.Clients.All.ScrapeFinished();
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Failed to scrape Agent.");
                        }
                    }
                }
                finally
                {
                    await ScheduleHydrationAsync(default);
                }
            }
        }
    }
}