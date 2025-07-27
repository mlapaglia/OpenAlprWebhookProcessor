using MediatR;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebsocket;

namespace OpenAlprWebhookProcessor.Features.WebSockets.Queries.GetAccountInfo
{
    public class GetAccountInfoQuery : IRequest<AccountInfoResponse>
    {
    }
} 