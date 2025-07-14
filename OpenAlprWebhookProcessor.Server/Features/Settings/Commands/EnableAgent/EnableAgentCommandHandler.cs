using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Settings.Commands.EnableAgent
{
    public class EnableAgentCommandHandler : IRequestHandler<EnableAgentCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly WebsocketClientOrganizer _websocketClientOrganizer;

        public EnableAgentCommandHandler(
            IUnitOfWork unitOfWork,
            WebsocketClientOrganizer websocketClientOrganizer)
        {
            _unitOfWork = unitOfWork;
            _websocketClientOrganizer = websocketClientOrganizer;
        }

        public async Task<bool> Handle(EnableAgentCommand request, CancellationToken cancellationToken)
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