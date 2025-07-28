using Mediator;
using Microsoft.AspNetCore.Http;
using System.Threading;

namespace OpenAlprWebhookProcessor.Features.WebSockets.Queries.GetWebSocket
{
    public class GetWebSocketQuery : ICommand
    {
        public HttpContext HttpContext { get; set; }
        public CancellationToken CancellationToken { get; set; }

        public GetWebSocketQuery(HttpContext httpContext, CancellationToken cancellationToken = default)
        {
            HttpContext = httpContext;
            CancellationToken = cancellationToken;
        }
    }
} 