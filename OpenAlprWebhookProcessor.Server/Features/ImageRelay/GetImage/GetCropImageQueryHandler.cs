using MediatR;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.ImageRelay.ImageCompression;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.ImageRelay.GetImage
{
    public class GetCropImageQueryHandler : IRequestHandler<GetCropImageQuery, Stream>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IImageCompressionService _imageCompressionService;

        public GetCropImageQueryHandler(IUnitOfWork unitOfWork, IImageCompressionService imageCompressionService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _imageCompressionService = imageCompressionService ?? throw new ArgumentNullException(nameof(imageCompressionService));
        }

        public async Task<Stream> Handle(GetCropImageQuery request, CancellationToken cancellationToken)
        {
            var plateGroup = await _unitOfWork.PlateGroups.FirstOrDefaultAsync(x => x.OpenAlprUuid == request.ImageId, cancellationToken);

            if (plateGroup == null)
            {
                throw new ArgumentException("No image found with that id.");
            }

            var fullPlateGroup = await _unitOfWork.PlateGroups.GetByIdWithDetailsAsync(plateGroup.Id, cancellationToken);

            if (fullPlateGroup == null)
            {
                throw new ArgumentException("No plate group found with that id.");
            }

            var agent = await _unitOfWork.Agents.GetFirstAgentAsync(cancellationToken);

            if (fullPlateGroup.PlateImage == null)
            {
                var imageBytes = await _imageCompressionService.GetCropImageFromAgentAsync(
                    agent,
                    request.ImageId,
                    fullPlateGroup.PlateCoordinates,
                    cancellationToken);

                fullPlateGroup.PlateImage = new PlateImage()
                {
                    Jpeg = imageBytes,
                    IsCompressed = agent?.IsImageCompressionEnabled ?? false,
                };

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return new MemoryStream(fullPlateGroup.PlateImage.Jpeg);
        }
    }
} 