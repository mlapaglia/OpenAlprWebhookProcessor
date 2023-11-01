using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.ImageRelay;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    public class ImageRetrieverService : IHostedService
    {
        private readonly BlockingCollection<string> _imageRequestsToProcess;

        private readonly BlockingCollection<string> _imageCompressionRequestsToProcess;

        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly IServiceProvider _serviceProvider;

        public ImageRetrieverService(IServiceProvider serviceProvider)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _serviceProvider = serviceProvider;
            _imageRequestsToProcess = new BlockingCollection<string>();
            _imageCompressionRequestsToProcess = new BlockingCollection<string>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
                await ProcessImageRequestsAsync(),
                cancellationToken);

            Task.Run(async () =>
                await ProcessImageCompressionRequestsAsync(),
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

        public void AddImageCompressionJob()
        {
            _imageCompressionRequestsToProcess.Add("allImages");
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

                    var isImageCompressionEnabled = await processorContext.Agents
                        .AsNoTracking()
                        .Select(x => x.IsImageCompressionEnabled)
                        .FirstOrDefaultAsync();

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
                            plateGroup.isPlateJpegCompressed = isImageCompressionEnabled;
                            plateGroup.isVehicleJpegCompressed = isImageCompressionEnabled;
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

        private async Task ProcessImageCompressionRequestsAsync()
        {
            foreach (var job in _imageCompressionRequestsToProcess.GetConsumingEnumerable(_cancellationTokenSource.Token))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    bool keepPaging = true;
                    long lastReceivedOnEpoch = 0;

                    while (keepPaging)
                    {
                        var processorContext = scope.ServiceProvider.GetRequiredService<ProcessorContext>();
                        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ImageRetrieverService>>();

                        var isImageCompressionEnabled = await processorContext.Agents
                            .AsNoTracking()
                            .Select(x => x.IsImageCompressionEnabled)
                            .FirstOrDefaultAsync();

                        if (!isImageCompressionEnabled)
                        {
                            logger.LogWarning("Image compression disabled, check agent settings.");
                            break;
                        }

                        var plateGroups = await processorContext.PlateGroups
                            .OrderBy(x => x.ReceivedOnEpoch)
                            .Where(x => x.ReceivedOnEpoch > lastReceivedOnEpoch)
                            .Where(x => !x.isPlateJpegCompressed || !x.isVehicleJpegCompressed)
                            .Where(x => x.PlateJpeg.Length > 0 || x.VehicleJpeg.Length > 0)
                            .Take(25)
                            .ToListAsync(_cancellationTokenSource.Token);

                        if (!plateGroups.Any())
                        {
                            keepPaging = false;
                        }
                        else
                        {
                            lastReceivedOnEpoch = plateGroups.First().ReceivedOnEpoch;
                        }

                        logger.LogInformation("Searcing for images newer than {epoch}: {numberOfRequests} images queued for compression", lastReceivedOnEpoch, _imageRequestsToProcess.Count);

                        foreach (var plateGroup in plateGroups)
                        {
                            if (!plateGroup.isVehicleJpegCompressed && plateGroup.VehicleJpeg != null)
                            {
                                plateGroup.VehicleJpeg = GetImageHandler.CompressImage(plateGroup.VehicleJpeg);
                                plateGroup.isVehicleJpegCompressed = true;
                            }

                            if (!plateGroup.isPlateJpegCompressed && plateGroup.PlateJpeg != null)
                            {
                                plateGroup.PlateJpeg = GetImageHandler.CompressImage(plateGroup.PlateJpeg);
                                plateGroup.isPlateJpegCompressed = true;
                            }
                        }

                        await processorContext.SaveChangesAsync(_cancellationTokenSource.Token);
                    }
                }
            }
        }
    }
}