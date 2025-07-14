using ImageMagick;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.ImageRelay.GetImage
{
    public class GetCropImageQueryHandler : IRequestHandler<GetCropImageQuery, Stream>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _httpClientFactory;

        public GetCropImageQueryHandler(IUnitOfWork unitOfWork, IHttpClientFactory httpClientFactory)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<Stream> Handle(GetCropImageQuery request, CancellationToken cancellationToken)
        {
            var plateGroups = await _unitOfWork.PlateGroups.GetAllAsync(cancellationToken);
            var plateGroup = plateGroups
                .FirstOrDefault(x => x.OpenAlprUuid == request.ImageId);

            if (plateGroup == null)
            {
                throw new ArgumentException("No image found with that id.");
            }

            // Get the full plate group with plate image
            var fullPlateGroup = await _unitOfWork.PlateGroups.GetByIdWithDetailsAsync(plateGroup.Id, cancellationToken);
            
            var agents = await _unitOfWork.Agents.GetAllAsync(cancellationToken);
            var agent = agents.FirstOrDefault();

            if (fullPlateGroup?.PlateImage == null)
            {
                var imageBytes = await GetCropImageFromAgentAsync(agent, request.ImageId, fullPlateGroup.PlateCoordinates, cancellationToken);
                
                var plateImage = new PlateImage()
                {
                    Jpeg = imageBytes,
                    IsCompressed = agent?.IsImageCompressionEnabled ?? false,
                };

                fullPlateGroup.PlateImage = plateImage;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return new MemoryStream(fullPlateGroup.PlateImage.Jpeg);
        }

        private async Task<byte[]> GetCropImageFromAgentAsync(Agent agent, string imageId, string plateCoordinates, CancellationToken cancellationToken)
        {
            if (agent == null || string.IsNullOrWhiteSpace(agent.EndpointUrl))
            {
                throw new ArgumentException("Agent not configured");
            }

            using var httpClient = _httpClientFactory.CreateClient();

            var imageUrl = Flurl.Url.Combine(agent.EndpointUrl, "/crop/", imageId + "?" + plateCoordinates);
            using var response = await httpClient.GetAsync(imageUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new ArgumentException("Image not found for that id.");
            }

            var imageBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            return agent.IsImageCompressionEnabled ? CompressImage(imageBytes) : imageBytes;
        }

        private static byte[] CompressImage(byte[] rawImage)
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