using Mediator;
using Microsoft.EntityFrameworkCore;
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
    public class GetImageQueryHandler : IQueryHandler<GetImageQuery, Stream>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IImageCompressionService _imageCompressionService;

        public GetImageQueryHandler(IUnitOfWork unitOfWork, IImageCompressionService imageCompressionService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _imageCompressionService = imageCompressionService ?? throw new ArgumentNullException(nameof(imageCompressionService));
        }

        public async ValueTask<Stream> Handle(
            GetImageQuery request,
            CancellationToken cancellationToken = default)
        {
            var plateGroup = await _unitOfWork.PlateGroups.GetQueryable()
                .Where(x => x.OpenAlprUuid == request.ImageId)
                .FirstOrDefaultAsync(cancellationToken);

            if (plateGroup == null)
            {
                throw new ArgumentException("No image found with that id.");
            }

            var fullPlateGroup = await _unitOfWork.PlateGroups.GetByIdWithDetailsAsync(
                plateGroup.Id,
                cancellationToken);
            
            if (fullPlateGroup == null)
            {
                throw new ArgumentException("No plate group found with that id.");
            }

            var agent = await _unitOfWork.Agents.GetFirstAgentAsync(cancellationToken);

            if (fullPlateGroup.VehicleImage == null)
            {
                var imageBytes = await _imageCompressionService.GetImageFromAgentAsync(
                    agent,
                    request.ImageId,
                    cancellationToken);
                
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