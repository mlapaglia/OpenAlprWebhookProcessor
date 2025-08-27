using Mediator;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebsocket;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.ImageRelay.WebsocketSnapshotRelay
{
    public class GetWebsocketSnapshotQueryHandler : IQueryHandler<GetWebsocketSnapshotQuery, Stream>
    {
        private readonly IWebsocketClientOrganizer _websocketClientOrganizer;

        public GetWebsocketSnapshotQueryHandler(IWebsocketClientOrganizer websocketClientOrganizer)
        {
            _websocketClientOrganizer = websocketClientOrganizer ?? throw new ArgumentNullException(nameof(websocketClientOrganizer));
        }

        public async ValueTask<Stream> Handle(GetWebsocketSnapshotQuery request, CancellationToken cancellationToken)
        {
            var response = await _websocketClientOrganizer.GetCameraSnapshotAsync(
                request.AgentId,
                request.CameraId,
                cancellationToken);

            if (response == null)
            {
                throw new InvalidOperationException("Failed to get snapshot from agent");
            }

            if (string.IsNullOrWhiteSpace(response.Image))
            {
                throw new InvalidOperationException("Agent returned empty image data");
            }

            try
            {
                var imageBytes = Convert.FromBase64String(response.Image);
                return new MemoryStream(imageBytes);
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException("Invalid image data received from agent", ex);
            }
        }
    }
}
