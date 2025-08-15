using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Features.Settings.Queries.GetAgentVideoStreams
{
    public class AgentVideoStreamsDto
    {
        public bool IsConnected { get; set; }

        public List<VideoStreamDto> VideoStreams { get; set; } = new List<VideoStreamDto>();
    }
}