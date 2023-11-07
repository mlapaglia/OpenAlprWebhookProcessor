using ImageMagick;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.ImageRelay
{
    public static class GetImageHandler
    {
        public async static Task<Stream> GetImageFromLocalAsync(
            ProcessorContext processorContext,
            string imageId,
            CancellationToken cancellationToken)
        {
            var plateGroup = await processorContext.PlateGroups
                .Include(x => x.VehicleImage)
                .Where(x => x.OpenAlprUuid == imageId)
                .FirstOrDefaultAsync(cancellationToken);

            var isImageCompressionEnabled = await processorContext.Agents
                .AsNoTracking()
                .Select(x => x.IsImageCompressionEnabled)
                .FirstOrDefaultAsync(cancellationToken);

            if (plateGroup == null)
            {
                throw new ArgumentException("No image found with that id.");
            }

            if (plateGroup.VehicleImage == null)
            {
                plateGroup.VehicleImage = new VehicleImage()
                {
                    Jpeg = await GetImageFromAgentAsync(
                        processorContext,
                        imageId,
                        cancellationToken),
                    IsCompressed = isImageCompressionEnabled,
                };

                await processorContext.SaveChangesAsync(cancellationToken);
            }

            return new MemoryStream(plateGroup.VehicleImage.Jpeg);
        }

        public async static Task<Stream> GetCropImageFromLocalAsync(
            ProcessorContext processorContext,
            string imageId,
            CancellationToken cancellationToken)
        {
            var plateGroup = await processorContext.PlateGroups
                .Include(x => x.PlateImage)
                .Where(x => x.OpenAlprUuid == imageId)
                .FirstOrDefaultAsync(cancellationToken);

            var isImageCompressionEnabled = await processorContext.Agents
                .AsNoTracking()
                .Select(x => x.IsImageCompressionEnabled)
                .FirstOrDefaultAsync(cancellationToken);

            if (plateGroup == null)
            {
                throw new ArgumentException("No image found with that id.");
            }

            if (plateGroup.PlateImage == null)
            {
                var image = await GetCropImageFromAgentAsync(
                    processorContext,
                    imageId + "?" + plateGroup.PlateCoordinates,
                    cancellationToken);

                plateGroup.PlateImage = new PlateImage()
                {
                    Jpeg = image,
                    IsCompressed = isImageCompressionEnabled,
                };

                await processorContext.SaveChangesAsync(cancellationToken);
            }

            return new MemoryStream(plateGroup.PlateImage.Jpeg);
        }

        public async static Task<byte[]> GetImageFromAgentAsync(
            ProcessorContext processorContext,
            string imageId,
            CancellationToken cancellationToken)
        {
            var agent = await processorContext.Agents
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (agent == null || string.IsNullOrWhiteSpace(agent.EndpointUrl))
            {
                throw new ArgumentException("agent not configured");
            }

            var httpClient = new HttpClient();

            var result = await httpClient.GetAsync(
                Flurl.Url.Combine(
                    agent.EndpointUrl,
                    "/img/",
                    imageId),
                cancellationToken);

            if (!result.IsSuccessStatusCode)
            {
                throw new ArgumentException("Image not found for that id.");
            }

            var imageBytes = await result.Content.ReadAsByteArrayAsync(cancellationToken);

            return agent.IsImageCompressionEnabled ? CompressImage(imageBytes) : imageBytes;
        }

        public async static Task<byte[]> GetCropImageFromAgentAsync(
            ProcessorContext processorContext,
            string imageId,
            CancellationToken cancellationToken)
        {
            var agent = await processorContext.Agents
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (agent == null || string.IsNullOrWhiteSpace(agent.EndpointUrl))
            {
                throw new ArgumentException("agent not configured");
            }

            var httpClient = new HttpClient();

            var result = await httpClient.GetAsync(
                Flurl.Url.Combine(
                    agent.EndpointUrl,
                    "/crop/",
                    imageId),
                cancellationToken);

            if (!result.IsSuccessStatusCode)
            {
                throw new ArgumentException("Image not found for that id.");
            }

            var imageBytes = await result.Content.ReadAsByteArrayAsync(cancellationToken);

            return agent.IsImageCompressionEnabled ? CompressImage(imageBytes) : imageBytes;
        }

        public static byte[] CompressImage(byte[] rawImage)
        {
            var optimizer = new ImageOptimizer();

            try
            {
                using (var stream = new MemoryStream(rawImage))
                {
                    optimizer.Compress(stream);

                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
