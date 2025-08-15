using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket;

namespace OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor
{
    public class AddAgentResult
    {
        public OpenAlprWebsocketClient WebSocketClient { get; set; }
        
        public bool WasAdded { get; set; }

        public bool WasUpdated { get; set; }

        public bool UpdateWasCleanDisconnect { get; set; }
    }
}
