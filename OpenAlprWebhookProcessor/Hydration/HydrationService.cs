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
using OpenAlprWebhookProcessor.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.Storage;
using System.Linq.Expressions;

namespace OpenAlprWebhookProcessor.Hydrator
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
            return Task.CompletedTask;
        }

        public async Task ScheduleHydrationAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                var agent = await processorContext.Agents.FirstOrDefaultAsync(cancellationToken);

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