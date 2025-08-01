using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.ProcessorHub
{
    [Authorize]
    public class ProcessorHub : Hub<IProcessorHub>
    {
        private readonly ILogger<ProcessorHub> _logger;
        private static readonly ConcurrentDictionary<string, SignalRConnectionInfo> _connections = new();

        public ProcessorHub(ILogger<ProcessorHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User.Identity.Name;
            var connectionInfo = new SignalRConnectionInfo
            {
                ConnectionId = Context.ConnectionId,
                UserId = userId,
                ConnectedAt = DateTime.UtcNow,
                Transport = GetTransportType(),
                UserAgent = Context.GetHttpContext()?.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown",
                IpAddress = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
            };

            _connections.TryAdd(Context.ConnectionId, connectionInfo);
            
            _logger.LogDebug("SignalR connection established for user: {UserId}, ConnectionId: {ConnectionId}, Transport: {Transport}", 
                userId, Context.ConnectionId, connectionInfo.Transport);
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            var userId = Context.User.Identity.Name;
            _connections.TryRemove(Context.ConnectionId, out var connectionInfo);
            
            _logger.LogDebug("SignalR connection closed for user: {UserId}, ConnectionId: {ConnectionId}", 
                userId, Context.ConnectionId);
            
            if (exception != null)
            {
                _logger.LogDebug(exception, "SignalR connection closed with exception for user: {UserId}", userId);
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        private string GetTransportType()
        {
            var httpContext = Context.GetHttpContext();
            var isWebSocket = httpContext?.WebSockets?.IsWebSocketRequest == true;
            
            if (isWebSocket)
                return "WebSockets";
            
            // Check for Server-Sent Events
            var accept = httpContext?.Request.Headers["Accept"].FirstOrDefault();
            if (accept?.Contains("text/event-stream") == true)
                return "Server-Sent Events";
            
            // Default to Long Polling for other transports
            return "Long Polling";
        }

        public static SignalRConnectionInfo[] GetAllConnections()
        {
            return _connections.Values.ToArray();
        }
    }
}
