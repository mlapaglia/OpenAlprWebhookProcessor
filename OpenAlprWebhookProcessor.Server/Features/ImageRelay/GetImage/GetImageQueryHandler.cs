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
    public class GetImageQueryHandler : IRequestHandler<GetImageQuery, Stream>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageCompressionService _imageCompressionService;

        public GetImageQueryHandler(IUnitOfWork unitOfWork, IImageCompressionService imageCompressionService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _imageCompressionService = imageCompressionService ?? throw new ArgumentNullException(nameof(imageCompressionService));
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
                var imageBytes = await _imageCompressionService.GetImageFromAgentAsync(agent, request.ImageId, cancellationToken);
                
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


    }
} 