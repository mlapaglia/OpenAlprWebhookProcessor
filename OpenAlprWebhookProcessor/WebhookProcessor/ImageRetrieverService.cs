using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.ImageRelay;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    public class ImageRetrieverService : IHostedService
    {
        private readonly BlockingCollection<string> _imageRequestsToProcess;

        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly IServiceProvider _serviceProvider;

        public ImageRetrieverService(IServiceProvider serviceProvider)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _serviceProvider = serviceProvider;
            _imageRequestsToProcess = new BlockingCollection<string>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
                await ProcessImageRequestsAsync(),
                cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }

        public void AddJob(string openAlprImageId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<ImageRetrieverService>>();
                logger.LogInformation("adding job for image: {imageId}", openAlprImageId);

                _imageRequestsToProcess.Add(openAlprImageId);
            }
        }

        private async Task ProcessImageRequestsAsync()
        {
            foreach (var job in _imageRequestsToProcess.GetConsumingEnumerable(_cancellationTokenSource.Token))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<ImageRetrieverService>>();
                    logger.LogInformation("{numberOfRequests} images queued for processing", _imageRequestsToProcess.Count);

                    var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                    var plateGroups = await processorContext.PlateGroups
                        .Where(x => x.OpenAlprUuid == job)
                        .ToListAsync(_cancellationTokenSource.Token);

                    foreach (var plateGroup in plateGroups)
                    {
                        if (plateGroup == null)
                        {
                            logger.LogError("Unable to find openalpr group id: {groupId}", job);
                            continue;
                        }

                        try
                        {
                            var image = await GetImageHandler.GetImageFromAgentAsync(
                                processorContext,
                                job,
                                _cancellationTokenSource.Token);

                            var cropImage = await GetImageHandler.GetCropImageFromAgentAsync(
                                processorContext,
                                job + "?" + plateGroup.PlateCoordinates,
                                _cancellationTokenSource.Token);

                            plateGroup.PlateJpeg = cropImage;
                            plateGroup.VehicleJpeg = image;
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Unable to retrieve image from Agent: {imageId}", job);
                        }

                        plateGroup.AgentImageScrapeOccurredOn = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        await processorContext.SaveChangesAsync(_cancellationTokenSource.Token);
                    }

                    logger.LogInformation("finished job for image: {imageId}", job);
                }
            }
        }
    }
}