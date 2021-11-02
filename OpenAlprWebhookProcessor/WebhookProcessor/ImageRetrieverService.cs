using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.ImageRelay;
using OpenAlprWebhookProcessor.ImageRelay.GetImage;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    public class ImageRetrieverService : IHostedService
    {
        private readonly BlockingCollection<string> _imageRequestsToProcess;

        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger _logger;

        public ImageRetrieverService(
            IServiceProvider serviceProvider,
            ILogger<ImageRetrieverService> logger)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _serviceProvider = serviceProvider;
            _logger = logger;
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
            _logger.LogInformation("adding job for image: {imageId}", openAlprImageId);
            _imageRequestsToProcess.Add(openAlprImageId);
        }

        private async Task ProcessImageRequestsAsync()
        {
            foreach (var job in _imageRequestsToProcess.GetConsumingEnumerable(_cancellationTokenSource.Token))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();

                    var plateGroup = await processorContext.PlateGroups.FirstOrDefaultAsync(x => x.OpenAlprUuid == job);

                    if (plateGroup == null)
                    {
                        _logger.LogError("Unable to find openalpr group id: {groupId}", job);
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

                        await processorContext.SaveChangesAsync(_cancellationTokenSource.Token);
                        _logger.LogInformation("finished job for image: {imageId}", job);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unable to retrieve image from Agent: {imageId}", job);
                        continue;
                    }
                }
            }
        }
    }
}
