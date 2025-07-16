using MediatR;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.ImageRelay.ImageCompression;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.ImageRelay.GetImage
{
    public class GetCropImageQueryHandler : IRequestHandler<GetCropImageQuery, Stream>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ImageCompressionService _imageCompressionService;

        public GetCropImageQueryHandler(IUnitOfWork unitOfWork, ImageCompressionService imageCompressionService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _imageCompressionService = imageCompressionService ?? throw new ArgumentNullException(nameof(imageCompressionService));
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
                var imageBytes = await _imageCompressionService.GetCropImageFromAgentAsync(agent, request.ImageId, fullPlateGroup.PlateCoordinates, cancellationToken);
                
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


    }
} 