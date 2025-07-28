using Mediator;
using OpenAlprWebhookProcessor.Alerts.WebPush;

namespace OpenAlprWebhookProcessor.Features.Alerts.Commands.UpsertWebPush
{
    public class UpsertWebPushCommand : ICommand
    {
        public WebPushRequest Request { get; set; }

        public UpsertWebPushCommand(WebPushRequest request)
        {
            Request = request;
        }
    }
} 