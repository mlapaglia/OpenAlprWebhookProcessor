using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.ImageRelay.ImageCompression;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    public class ImageRetrieverService : IHostedService, IImageRetrieverService
    {
        private readonly BlockingCollection<string> _imageRequestsToProcess = new BlockingCollection<string>();

        private readonly HashSet<string> _imageRequestsToProcessList = new();

        private readonly object _imageRequestsToProcessGate = new();

        private readonly BlockingCollection<string> _imageCompressionRequestsToProcess = new();

        private readonly HashSet<string> _imageCompressionRequestsToProcessList = new();

        private readonly object _imageCompressionRequestsToGate = new();

        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly IServiceProvider _serviceProvider;

        public ImageRetrieverService(IServiceProvider serviceProvider)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _serviceProvider = serviceProvider;
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
            _cancellationTokenSource.Dispose();

            return Task.CompletedTask;
        }

        public void AddImageRetrievalJob(string uuid)
        {
            if (string.IsNullOrWhiteSpace(uuid))
                return;

            lock (_imageRequestsToProcessGate)
            {
                if (_imageRequestsToProcessList.Add(uuid))
                {
                    _imageRequestsToProcess.Add(uuid);
                }
            }
        }

        public void AddImageCompressionJob(string ignoreThisParameter)
        {
            if (_imageCompressionRequestsToProcessList.Add(ignoreThisParameter))
            {
                _imageCompressionRequestsToProcess.Add(ignoreThisParameter);
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

                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var plateGroups = await unitOfWork.PlateGroups.GetQueryable()
                        .Include(x => x.PlateImage)
                        .Include(x => x.VehicleImage)
                        .Where(x => x.OpenAlprUuid == job)
                        .ToListAsync(_cancellationTokenSource.Token);

                    var agent = await unitOfWork.Agents.GetFirstAgentAsync(_cancellationTokenSource.Token);

                    var isImageCompressionEnabled = agent?.IsImageCompressionEnabled ?? false;

                    foreach (var plateGroup in plateGroups)
                    {
                        if (plateGroup == null)
                        {
                            logger.LogError("Unable to find openalpr group id: {groupId}", job);
                            continue;
                        }

                        try
                        {
                            var imageCompressionService = scope.ServiceProvider.GetRequiredService<ImageCompressionService>();

                            var image = await imageCompressionService.GetImageFromAgentAsync(
                                agent,
                                job,
                                _cancellationTokenSource.Token);

                            var cropImage = await imageCompressionService.GetCropImageFromAgentAsync(
                                agent,
                                job + "?" + plateGroup.PlateCoordinates,
                                _cancellationTokenSource.Token);

                            plateGroup.PlateImage = new PlateImage()
                            {
                                Jpeg = cropImage,
                                IsCompressed = isImageCompressionEnabled,
                            };

                            plateGroup.VehicleImage = new VehicleImage()
                            {
                                Jpeg = image,
                                IsCompressed = isImageCompressionEnabled,
                            };
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Unable to retrieve image from Agent: {imageId}", job);
                        }

                        plateGroup.AgentImageScrapeOccurredOn = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        await unitOfWork.SaveChangesAsync(_cancellationTokenSource.Token);

                        lock (_imageRequestsToProcessGate)
                        {
                            _imageRequestsToProcessList.Remove(job);
                        }
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
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ImageRetrieverService>>();

                        var agent = await unitOfWork.Agents.GetFirstAgentAsync(_cancellationTokenSource.Token);
                        var isImageCompressionEnabled = agent?.IsImageCompressionEnabled ?? false;

                        if (!isImageCompressionEnabled)
                        {
                            logger.LogWarning("Image compression disabled, check agent settings.");
                            break;
                        }

                        var orderedGroups = await unitOfWork.PlateGroups.GetQueryable()
                            .Include(x => x.PlateImage)
                            .Include(x => x.VehicleImage)
                            .OrderBy(x => x.ReceivedOnEpoch)
                            .Where(x => x.ReceivedOnEpoch > lastReceivedOnEpoch)
                            .Where(x => !x.PlateImage.IsCompressed || !x.VehicleImage.IsCompressed)
                            .Where(x => x.PlateImage.Jpeg.Length > 0 || x.VehicleImage.Jpeg.Length > 0)
                            .Take(25)
                            .ToListAsync(_cancellationTokenSource.Token);

                        if (!orderedGroups.Any())
                        {
                            keepPaging = false;
                        }
                        else
                        {
                            lastReceivedOnEpoch = orderedGroups.First().ReceivedOnEpoch;
                        }

                        logger.LogInformation("Searching for images newer than {epoch}: {numberOfRequests} images queued for compression", lastReceivedOnEpoch, orderedGroups.Count);

                        foreach (var plateGroup in orderedGroups)
                        {
                            if (!plateGroup.VehicleImage.IsCompressed && plateGroup.VehicleImage.Jpeg != null)
                            {
                                plateGroup.VehicleImage.Jpeg = ImageCompressionService.CompressImage(plateGroup.VehicleImage.Jpeg);
                                plateGroup.VehicleImage.IsCompressed = true;
                            }

                            if (!plateGroup.PlateImage.IsCompressed && plateGroup.PlateImage.Jpeg != null)
                            {
                                plateGroup.PlateImage.Jpeg = ImageCompressionService.CompressImage(plateGroup.PlateImage.Jpeg);
                                plateGroup.PlateImage.IsCompressed = true;
                            }
                        }

                        await unitOfWork.SaveChangesAsync(_cancellationTokenSource.Token);
                    }
                }
            }
        }
    }
}