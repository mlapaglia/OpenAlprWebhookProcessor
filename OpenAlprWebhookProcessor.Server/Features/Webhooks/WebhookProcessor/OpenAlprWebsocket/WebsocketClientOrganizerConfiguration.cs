using System.ComponentModel.DataAnnotations;

namespace OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebsocket
{
    public class WebsocketClientOrganizerConfiguration
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = $"{nameof(TimeoutMilliseconds)} must be greater than 0")]
        public int TimeoutMilliseconds { get; set; }
    }
}
