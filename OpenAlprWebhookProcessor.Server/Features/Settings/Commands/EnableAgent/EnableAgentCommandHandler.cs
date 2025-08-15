using Mediator;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebsocket;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Settings.Commands.EnableAgent
{
    public class EnableAgentCommandHandler : IQueryHandler<EnableAgentCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebsocketClientOrganizer _websocketClientOrganizer;

        public EnableAgentCommandHandler(
            IUnitOfWork unitOfWork,
            IWebsocketClientOrganizer websocketClientOrganizer)
        {
            _unitOfWork = unitOfWork;
            _websocketClientOrganizer = websocketClientOrganizer;
        }

        public async ValueTask<bool> Handle(EnableAgentCommand request, CancellationToken cancellationToken = default)
        {
            var agent = await _unitOfWork.Agents.GetFirstAgentAsync(cancellationToken);

            if (agent == null)
            {
                return false;
            }

            return await _websocketClientOrganizer.DisableEnableAgentAsync(
                agent.Uid,
                AgentStartStopType.Start,
                cancellationToken);
        }
    }
} 