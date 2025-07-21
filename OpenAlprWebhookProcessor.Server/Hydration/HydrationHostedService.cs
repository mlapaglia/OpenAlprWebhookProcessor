using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.ProcessorHub;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprAgentScraper;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Hydrator
{
    public class HydrationHostedService : BackgroundService
    {
        private readonly IHydrationService _hydrationService;

        private readonly IServiceProvider _serviceProvider;

        private readonly IHubContext<ProcessorHub.ProcessorHub, IProcessorHub> _processorHub;

        private readonly ILogger<HydrationHostedService> _logger;

        public HydrationHostedService(
            IHydrationService hydrationService,
            IServiceProvider serviceProvider,
            IHubContext<ProcessorHub.ProcessorHub, IProcessorHub> processorHub,
            ILogger<HydrationHostedService> logger)
        {
            _hydrationService = hydrationService ?? throw new ArgumentNullException(nameof(hydrationService));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _processorHub = processorHub ?? throw new ArgumentNullException(nameof(processorHub));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("HydrationHostedService starting...");

            // Initial scheduling
            await _hydrationService.ScheduleHydrationAsync(stoppingToken);

            try
            {
                await ProcessHydrationRequestsAsync(stoppingToken);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogInformation(ex, "Hydration processing cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error in HydrationHostedService");
                throw;
            }

            _logger.LogInformation("HydrationHostedService stopped.");
        }

        private async Task ProcessHydrationRequestsAsync(CancellationToken cancellationToken)
        {
            foreach (var hydrationRequest in _hydrationService.GetConsumingHydrationRequests(cancellationToken))
            {
                try
                {
                    await ProcessSingleHydrationRequest(hydrationRequest, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing hydration request: {Request}", hydrationRequest);
                }
                finally
                {
                    // Always reschedule after processing, regardless of success or failure
                    try
                    {
                        await _hydrationService.ScheduleHydrationAsync(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error rescheduling hydration");
                    }
                }
            }
        }

        private async Task ProcessSingleHydrationRequest(
            string requestName,
            CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var scopedLogger = scope.ServiceProvider.GetRequiredService<ILogger<HydrationHostedService>>();

            scopedLogger.LogInformation("Starting OpenALPR Agent scrape. Request: {RequestName}, Pending: {Count}",
                requestName,
                _hydrationService.GetPendingHydrationCount());

            try
            {
                var scraper = scope.ServiceProvider.GetRequiredService<IOpenAlprAgentScraper>();

                await scraper.ScrapeAgentAsync(cancellationToken);
                await scraper.ScrapeAgentImagesAsync(cancellationToken);

                await _processorHub.Clients.All.ScrapeFinished();

                scopedLogger.LogInformation("OpenALPR Agent scrape completed successfully");
            }
            catch (OperationCanceledException ex)
            {
                scopedLogger.LogInformation(ex, "Scraping operation was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                scopedLogger.LogError(ex, "Failed to scrape Agent");
                throw;
            }
        }
    }
}