using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Cameras.UpsertMasks;
using OpenAlprWebhookProcessor.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Cameras.Queries.GetCameraMask
{
    public class GetCameraMaskQueryHandler : IRequestHandler<GetCameraMaskQuery, List<MaskCoordinate>>
    {
        private readonly ProcessorContext _processorContext;

        public GetCameraMaskQueryHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task<List<MaskCoordinate>> Handle(GetCameraMaskQuery request, CancellationToken cancellationToken)
        {
            var maskCoordinates = await _processorContext.CameraMasks
                .AsNoTracking()
                .Where(x => x.CameraId == request.CameraId)
                .Select(x => x.Coordinates)
                .FirstOrDefaultAsync(cancellationToken);

            if (maskCoordinates == null)
            {
                return new List<MaskCoordinate>();
            }
            else
            {
                return JsonSerializer.Deserialize<List<MaskCoordinate>>(maskCoordinates);
            }
        }
    }
} 