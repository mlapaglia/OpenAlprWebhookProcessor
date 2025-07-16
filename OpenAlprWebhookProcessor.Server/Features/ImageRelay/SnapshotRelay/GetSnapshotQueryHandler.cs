using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Cameras;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.ImageRelay.SnapshotRelay
{
    public class GetSnapshotQueryHandler : IRequestHandler<GetSnapshotQuery, Stream>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetSnapshotQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Stream> Handle(GetSnapshotQuery request, CancellationToken cancellationToken)
        {
            var cameras = await _unitOfWork.Cameras.GetAllAsync(cancellationToken);
            var dbCamera = cameras.FirstOrDefault(x => x.Id == request.CameraId);

            if (dbCamera == null)
            {
                throw new ArgumentException("Camera not found.");
            }

            var camera = CameraFactory.Create(dbCamera.Manufacturer, dbCamera);

            const int timeout = 5000;
            var task = camera.GetSnapshotAsync(cancellationToken);
            
            if (await Task.WhenAny(task, Task.Delay(timeout, cancellationToken)) == task)
            {
                return await task;
            }
            else
            {
                throw new TimeoutException("Unable to get image from camera");
            }
        }
    }
} 