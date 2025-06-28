using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.SignalR;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.Storage;
using System.Linq.Expressions;
using OpenAlprWebhookProcessor.Server.WebhookProcessor.OpenAlprAgentScraper;
using OpenAlprWebhookProcessor.Server.Data;
using OpenAlprWebhookProcessor.Server.ProcessorHub;

namespace OpenAlprWebhookProcessor.Server.Hydration
{
    public class HydrationService : IHostedService
    {
        private readonly BlockingCollection<string> _hydrationRequestsToProcess;

        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly IServiceProvider _serviceProvider;

        private readonly IHubContext<ProcessorHub.ProcessorHub, IProcessorHub> _processorHub;

        private readonly JobStorage _jobStorage;

        public HydrationService(
            IServiceProvider serviceProvider,
            IHubContext<ProcessorHub.ProcessorHub, IProcessorHub> processorHub,
            JobStorage jobStorage)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _serviceProvider = serviceProvider;
            _processorHub = processorHub;
            _hydrationRequestsToProcess = new BlockingCollection<string>();
            _jobStorage = jobStorage;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();
                var agent = await processorContext.Agents
                    .FirstOrDefaultAsync(cancellationToken);

                await ScheduleHydrationAsync(cancellationToken);
            }
            _ = Task.Run(() => StartHydrationAsync(), cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            return Task.CompletedTask;
        }

        public async Task ScheduleHydrationAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                var agent = await processorContext.Agents.FirstOrDefaultAsync(cancellationToken);

                if (string.IsNullOrWhiteSpace(agent.Uid))
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<HydrationService>>();
                    logger.LogWarning("Agent UID is not set. Cannot schedule hydration.");
                    return;
                }

                if (agent.ScheduledScrapingIntervalMinutes == null)
                {
                    RecurringJob.RemoveIfExists(agent.Uid);
                    agent.NextScrapeEpochMs = null;
                }
                else
                {
                    RecurringJob
                        .AddOrUpdate(agent.Uid,
                            () => StartHydration(agent.Uid),
                        $"*/{agent.ScheduledScrapingIntervalMinutes} * * * *");

                    var nextScrape = _jobStorage
                        .GetConnection()
                        .GetRecurringJobs()
                        .Single(x => x.Id == agent.Uid);

                    agent.NextScrapeEpochMs = new DateTimeOffset(nextScrape.NextExecution.Value).ToUnixTimeMilliseconds();
                }

                await processorContext.SaveChangesAsync(cancellationToken);
            }
        }

        public void StartHydration(string request)
        {
            _hydrationRequestsToProcess.Add(request);
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
                            var scraper = scope.ServiceProvider.GetRequiredService<OpenAlprAgentScraper>();
                            using var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                            var plateGroupIds = await processorContext.PlateGroups
                                .AsNoTracking()
                                .Where(x => x.AgentImageScrapeOccurredOn == null)
                                .Select(x => x.OpenAlprUuid)
                                .ToListAsync(_cancellationTokenSource.Token);

                            logger.LogInformation("Found {count} plates to query the Agent for.", plateGroupIds.Count);

                            var agent = await processorContext.Agents.FirstOrDefaultAsync(_cancellationTokenSource.Token);

                            await scraper.ScrapeAgentAsync(
                                agent.LastSuccessfulScrapeEpoch,
                                agent.EndpointUrl,
                                _cancellationTokenSource.Token);

                            await scraper.ScrapeAgentImagesAsync(
                                plateGroupIds,
                                _cancellationTokenSource.Token);

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