using ImageMagick;
using OpenAlprWebhookProcessor.Data;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.ImageRelay.ImageCompression
{
    public interface IImageCompressionService
    {
        Task<byte[]> GetImageFromAgentAsync(Agent agent, string imageId, CancellationToken cancellationToken);
        Task<byte[]> GetCropImageFromAgentAsync(Agent agent, string imageId, CancellationToken cancellationToken);
        Task<byte[]> GetCropImageFromAgentAsync(Agent agent, string imageId, string plateCoordinates, CancellationToken cancellationToken);
    }

    public class ImageCompressionService : IImageCompressionService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ImageCompressionService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<byte[]> GetImageFromAgentAsync(Agent agent, string imageId, CancellationToken cancellationToken)
        {
            if (agent == null || string.IsNullOrWhiteSpace(agent.EndpointUrl))
            {
                throw new ArgumentException("Agent not configured");
            }

            using var httpClient = _httpClientFactory.CreateClient();

            var imageUrl = Flurl.Url.Combine(agent.EndpointUrl, "/img/", imageId);
            using var response = await httpClient.GetAsync(imageUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new ArgumentException("Image not found for that id.");
            }

            var imageBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            return agent.IsImageCompressionEnabled ? CompressImage(imageBytes) : imageBytes;
        }

        public async Task<byte[]> GetCropImageFromAgentAsync(Agent agent, string imageId, CancellationToken cancellationToken)
        {
            return await GetCropImageFromAgentAsync(agent, imageId, null, cancellationToken);
        }

        public async Task<byte[]> GetCropImageFromAgentAsync(Agent agent, string imageId, string plateCoordinates, CancellationToken cancellationToken)
        {
            if (agent == null || string.IsNullOrWhiteSpace(agent.EndpointUrl))
            {
                throw new ArgumentException("Agent not configured");
            }

            using var httpClient = _httpClientFactory.CreateClient();

            var imageUrl = Flurl.Url.Combine(agent.EndpointUrl, "/crop/", imageId);
            if (!string.IsNullOrWhiteSpace(plateCoordinates))
            {
                imageUrl += "?" + plateCoordinates;
            }
            
            using var response = await httpClient.GetAsync(imageUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new ArgumentException("Image not found for that id.");
            }

            var imageBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            return agent.IsImageCompressionEnabled ? CompressImage(imageBytes) : imageBytes;
        }

        public static byte[] CompressImage(byte[] rawImage)
        {
            var optimizer = new ImageOptimizer();

            try
            {
                using var stream = new MemoryStream(rawImage);
                optimizer.Compress(stream);
                return stream.ToArray();
            }
            catch
            {
                return rawImage; // Return original if compression fails
            }
        }
    }
} 