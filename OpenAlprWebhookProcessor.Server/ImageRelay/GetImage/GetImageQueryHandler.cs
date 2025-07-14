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
    public class GetImageQueryHandler : IRequestHandler<GetImageQuery, Stream>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _httpClientFactory;

        public GetImageQueryHandler(IUnitOfWork unitOfWork, IHttpClientFactory httpClientFactory)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<Stream> Handle(GetImageQuery request, CancellationToken cancellationToken)
        {
            var plateGroups = await _unitOfWork.PlateGroups.GetAllAsync(cancellationToken);
            var plateGroup = plateGroups
                .FirstOrDefault(x => x.OpenAlprUuid == request.ImageId);

            if (plateGroup == null)
            {
                throw new ArgumentException("No image found with that id.");
            }

            // Get the full plate group with vehicle image
            var fullPlateGroup = await _unitOfWork.PlateGroups.GetByIdWithDetailsAsync(plateGroup.Id, cancellationToken);
            
            var agents = await _unitOfWork.Agents.GetAllAsync(cancellationToken);
            var agent = agents.FirstOrDefault();

            if (fullPlateGroup?.VehicleImage == null)
            {
                var imageBytes = await GetImageFromAgentAsync(agent, request.ImageId, cancellationToken);
                
                var vehicleImage = new VehicleImage()
                {
                    Jpeg = imageBytes,
                    IsCompressed = agent?.IsImageCompressionEnabled ?? false,
                };

                fullPlateGroup.VehicleImage = vehicleImage;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return new MemoryStream(fullPlateGroup.VehicleImage.Jpeg);
        }

        private async Task<byte[]> GetImageFromAgentAsync(Agent agent, string imageId, CancellationToken cancellationToken)
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