using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprAgentScraper;
using System;
using Microsoft.Extensions.DependencyInjection;
using OpenAlprWebhookProcessor.Alerts;
using OpenAlprWebhookProcessor.ProcessorHub;
using Microsoft.AspNetCore.SignalR;

namespace OpenAlprWebhookProcessor.Hydrator
{
    public class HydrationService : IHostedService
    {
        private readonly BlockingCollection<string> _hydrationRequestToProcess;

        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly ILogger _logger;

        private readonly IServiceProvider _serviceProvider;

        private readonly IHubContext<ProcessorHub.ProcessorHub, IProcessorHub> _processorHub;

        public HydrationService(
            IServiceProvider serviceProvider,
            ILogger<HydrationService> logger,
            IHubContext<ProcessorHub.ProcessorHub, IProcessorHub> processorHub)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _serviceProvider = serviceProvider;
            _logger = logger;
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
                _logger.LogInformation("Starting OpenALPR Agent scrape.");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var scraper = scope.ServiceProvider.GetRequiredService<OpenAlprAgentScraper>();
                    await scraper.ScrapeAgentAsync(_cancellationTokenSource.Token);
                }

                await _processorHub.Clients.All.ScrapeFinished();

                _logger.LogInformation("Finished OpenALPR Agent scrape.");
            }
        }
    }
}