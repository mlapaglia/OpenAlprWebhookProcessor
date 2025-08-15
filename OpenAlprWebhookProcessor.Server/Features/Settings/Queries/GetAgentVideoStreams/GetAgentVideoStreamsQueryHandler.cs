using Mediator;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebsocket;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Settings.Queries.GetAgentVideoStreams
{
    public class GetAgentVideoStreamsQueryHandler : IQueryHandler<GetAgentVideoStreamsQuery, AgentVideoStreamsDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebsocketClientOrganizer _websocketClientOrganizer;

        public GetAgentVideoStreamsQueryHandler(
            IUnitOfWork unitOfWork,
            IWebsocketClientOrganizer websocketClientOrganizer)
        {
            _unitOfWork = unitOfWork;
            _websocketClientOrganizer = websocketClientOrganizer;
        }

        public async ValueTask<AgentVideoStreamsDto> Handle(GetAgentVideoStreamsQuery request, CancellationToken cancellationToken = default)
        {
            var agent = await _unitOfWork.Agents.GetFirstAgentAsync(cancellationToken);

            if (agent == null || agent.Uid == null)
            {
                return new AgentVideoStreamsDto()
                {
                    IsConnected = false,
                };
            }

            var agentStatus = await _websocketClientOrganizer.GetAgentStatusAsync(agent.Uid, cancellationToken);

            if (agentStatus == null)
            {
                return new AgentVideoStreamsDto()
                {
                    IsConnected = false,
                };
            }

            return new AgentVideoStreamsDto()
            {
                IsConnected = true,
                VideoStreams = agentStatus.AgentStatus.VideoStreams?.Select(vs => new VideoStreamDto
                {
                    CameraId = vs.CameraId,
                    CameraName = vs.CameraName,
                    Fps = vs.Fps,
                    IsStreaming = vs.IsStreaming,
                    LastPlateRead = vs.LastPlateRead,
                    LastUpdate = vs.LastUpdate,
                    TotalPlateReads = vs.TotalPlateReads,
                    Url = vs.Url
                }).ToList() ?? new System.Collections.Generic.List<VideoStreamDto>()
            };
        }
    }
}