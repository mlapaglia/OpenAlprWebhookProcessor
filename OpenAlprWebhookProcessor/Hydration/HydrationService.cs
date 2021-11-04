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

namespace OpenAlprWebhookProcessor.Hydrator
{
    public class HydrationService : IHostedService
    {
        private readonly BlockingCollection<string> _hydrationRequestToProcess;

        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly IServiceProvider _serviceProvider;

        private readonly IHubContext<ProcessorHub.ProcessorHub, IProcessorHub> _processorHub;

        public HydrationService(
            IServiceProvider serviceProvider,
            IHubContext<ProcessorHub.ProcessorHub, IProcessorHub> processorHub)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _serviceProvider = serviceProvider;
            _processorHub = processorHub;
            _hydrationRequestToProcess = new BlockingCollection<string>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() => StartHydrationAsync(), cancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }

        public void StartHydration(string request)
        {
            _hydrationRequestToProcess.Add(request);
        }

        private async Task StartHydrationAsync()
        {
            foreach (var _ in _hydrationRequestToProcess.GetConsumingEnumerable(_cancellationTokenSource.Token))
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
        }
    }
}