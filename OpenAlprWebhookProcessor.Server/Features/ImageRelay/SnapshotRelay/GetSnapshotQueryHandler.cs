using Mediator;
using Microsoft.Extensions.Options;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Cameras;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.ImageRelay.SnapshotRelay
{
    public class GetSnapshotQueryHandler : IQueryHandler<GetSnapshotQuery, Stream>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ICameraFactory _cameraFactory;

        private readonly TimeSpan _timeout;

        public GetSnapshotQueryHandler(
            IUnitOfWork unitOfWork,
            ICameraFactory cameraFactory,
            IOptions<SnapshotRelayConfiguration> configuration)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _cameraFactory = cameraFactory ?? throw new ArgumentNullException(nameof(cameraFactory));
            _timeout = TimeSpan.FromMilliseconds(configuration.Value.TimeoutMilliseconds);
        }

        public async ValueTask<Stream> Handle(GetSnapshotQuery request, CancellationToken cancellationToken)
        {
            var dbCamera = await _unitOfWork.Cameras.GetByIdAsync(
                request.CameraId,
                cancellationToken);

            if (dbCamera == null)
            {
                throw new ArgumentException("Camera not found.");
            }

            var camera = _cameraFactory.Create(
                dbCamera.Manufacturer,
                dbCamera);

            var task = camera.GetSnapshotAsync(cancellationToken);
            
            if (await Task.WhenAny(task, Task.Delay(_timeout, cancellationToken)) == task)
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