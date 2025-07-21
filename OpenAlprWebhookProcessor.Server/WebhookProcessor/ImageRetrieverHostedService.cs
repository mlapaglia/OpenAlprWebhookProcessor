using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.ImageRelay.ImageCompression;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.WebhookProcessor
{
    public class ImageRetrieverHostedService : BackgroundService
    {
        private readonly IImageRetrieverService _imageRetrieverService;

        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger<ImageRetrieverHostedService> _logger;

        public ImageRetrieverHostedService(
            IImageRetrieverService imageRetrieverService,
            IServiceProvider serviceProvider,
            ILogger<ImageRetrieverHostedService> logger)
        {
            _imageRetrieverService = imageRetrieverService;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ImageRetrieverHostedService starting...");

            var imageProcessingTask = ProcessImageRequestsAsync(stoppingToken);
            var compressionProcessingTask = ProcessImageCompressionRequestsAsync(stoppingToken);

            await Task.WhenAll(imageProcessingTask, compressionProcessingTask);

            _logger.LogInformation("ImageRetrieverHostedService stopped.");
        }

        private async Task ProcessImageRequestsAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Image request processing task started");

            try
            {
                foreach (var job in _imageRetrieverService.GetConsumingImageRequests(cancellationToken))
                {
                    try
                    {
                        await ProcessSingleImageRequest(job, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing image request for job: {Job}", job);
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogInformation(ex, "Image request processing cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error in ProcessImageRequestsAsync");
            }
        }

        private async Task ProcessSingleImageRequest(string job, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            _logger.LogInformation("{NumberOfRequests} images queued for processing",
                _imageRetrieverService.GetImageRequestsCount());

            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var plateGroups = await unitOfWork.PlateGroups.GetQueryable()
                .Include(x => x.PlateImage)
                .Include(x => x.VehicleImage)
                .Where(x => x.OpenAlprUuid == job)
                .ToListAsync(cancellationToken);

            if (!plateGroups.Any())
            {
                _logger.LogWarning("No plate groups found for job: {Job}", job);
                _imageRetrieverService.RemoveImageRequest(job);
                return;
            }

            var agent = await unitOfWork.Agents.GetFirstAgentAsync(cancellationToken);
            var isImageCompressionEnabled = agent?.IsImageCompressionEnabled ?? false;

            foreach (var plateGroup in plateGroups)
            {
                try
                {
                    var imageCompressionService = scope.ServiceProvider.GetRequiredService<IImageCompressionService>();

                    var image = await imageCompressionService.GetImageFromAgentAsync(
                        agent,
                        job,
                        cancellationToken);

                    var cropImage = await imageCompressionService.GetCropImageFromAgentAsync(
                        agent,
                        job + "?" + plateGroup.PlateCoordinates,
                        cancellationToken);

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

                    plateGroup.AgentImageScrapeOccurredOn = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to retrieve image from Agent: {ImageId}", job);
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            _imageRetrieverService.RemoveImageRequest(job);

            _logger.LogInformation("Finished job for image: {ImageId}", job);
        }

        private async Task ProcessImageCompressionRequestsAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Image compression processing task started");

            try
            {
                foreach (var job in _imageRetrieverService.GetConsumingCompressionRequests(cancellationToken))
                {
                    try
                    {
                        await ProcessImageCompressionBatch(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing image compression job: {Job}", job);
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogInformation(ex, "Image compression processing cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error in ProcessImageCompressionRequestsAsync");
            }
        }

        private async Task ProcessImageCompressionBatch(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var agent = await unitOfWork.Agents.GetFirstAgentAsync(cancellationToken);
            var isImageCompressionEnabled = agent?.IsImageCompressionEnabled ?? false;

            if (!isImageCompressionEnabled)
            {
                _logger.LogWarning("Image compression disabled, check agent settings.");
                return;
            }

            bool keepPaging = true;
            long lastReceivedOnEpoch = 0;

            while (keepPaging && !cancellationToken.IsCancellationRequested)
            {
                var orderedGroups = await unitOfWork.PlateGroups.GetQueryable()
                    .Include(x => x.PlateImage)
                    .Include(x => x.VehicleImage)
                    .OrderBy(x => x.ReceivedOnEpoch)
                    .Where(x => x.ReceivedOnEpoch > lastReceivedOnEpoch)
                    .Where(x => !x.PlateImage.IsCompressed || !x.VehicleImage.IsCompressed)
                    .Where(x => x.PlateImage.Jpeg.Length > 0 || x.VehicleImage.Jpeg.Length > 0)
                    .Take(25)
                    .ToListAsync(cancellationToken);

                if (!orderedGroups.Any())
                {
                    keepPaging = false;
                    continue;
                }

                lastReceivedOnEpoch = orderedGroups.Last().ReceivedOnEpoch;

                _logger.LogInformation(
                    "Searching for images newer than {Epoch}: {NumberOfRequests} images queued for compression",
                    lastReceivedOnEpoch,
                    orderedGroups.Count);

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

                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
}