using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebsocket;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.WebSockets.Queries.GetWebSocket
{
    public class GetWebSocketQueryHandler : IRequestHandler<GetWebSocketQuery>
    {
        private readonly ILogger<GetWebSocketQueryHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebsocketClientOrganizer _websocketClientOrganizer;
        private readonly IHubContext<ProcessorHub.ProcessorHub, ProcessorHub.IProcessorHub> _processorHub;

        public GetWebSocketQueryHandler(
            ILogger<GetWebSocketQueryHandler> logger,
            IUnitOfWork unitOfWork,
            IWebsocketClientOrganizer websocketClientOrganizer,
            IHubContext<ProcessorHub.ProcessorHub, ProcessorHub.IProcessorHub> processorHub)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _websocketClientOrganizer = websocketClientOrganizer;
            _processorHub = processorHub;
        }

        public async Task Handle(GetWebSocketQuery request, CancellationToken cancellationToken = default)
        {
            if (request.HttpContext.WebSockets.IsWebSocketRequest)
            {
                _logger.LogInformation("Websocket connection received.");

                var agents = await _unitOfWork.Agents.GetAllAsync(cancellationToken);
                var agent = agents.FirstOrDefault();

                if (agent == null)
                {
                    _logger.LogError("No agent found");
                    request.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    return;
                }

                var webSocket = await request.HttpContext.WebSockets.AcceptWebSocketAsync();

                var webSocketClient = new OpenAlprWebsocketClient(
                    _logger,
                    agent.Uid,
                    webSocket);

                var addResult = await _websocketClientOrganizer.AddAgentAsync(
                    agent.Uid,
                    webSocketClient,
                    cancellationToken);

                if (!addResult.WasAdded)
                {
                    _logger.LogError("Unable to disconnect client: {AgentId}", agent.Uid);
                    return;
                }

                if (addResult.WasUpdated)
                {
                    _logger.LogWarning("Multiple websocket connections for the same agent, previous agent disconnected: {AgentId}.", agent.Uid);
                }

                await _processorHub.Clients.All.OpenAlprAgentConnected(agent.Uid, request.HttpContext.Connection.RemoteIpAddress.ToString());

                try
                {
                    await webSocketClient.ConsumeMessagesAsync(cancellationToken);

                    await _websocketClientOrganizer.RemoveAgentAsync(agent.Uid, cancellationToken);

                    _logger.LogInformation("Websocket connection closed: {AgentId}", agent.Uid);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Websocket connection closed ungracefully.");

                    await _websocketClientOrganizer.RemoveAgentAsync(agent.Uid, cancellationToken);
                    await _processorHub.Clients.All.OpenAlprAgentDisconnected(agent.Uid, request.HttpContext.Connection.RemoteIpAddress.ToString());
                }
            }
            else
            {
                _logger.LogInformation("Non websocket connection received.");
                request.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
    }
} 