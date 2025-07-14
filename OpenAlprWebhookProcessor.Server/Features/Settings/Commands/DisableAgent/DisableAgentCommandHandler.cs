using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Settings.Commands.DisableAgent
{
    public class DisableAgentCommandHandler : IRequestHandler<DisableAgentCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly WebsocketClientOrganizer _websocketClientOrganizer;

        public DisableAgentCommandHandler(
            IUnitOfWork unitOfWork,
            WebsocketClientOrganizer websocketClientOrganizer)
        {
            _unitOfWork = unitOfWork;
            _websocketClientOrganizer = websocketClientOrganizer;
        }

        public async Task<bool> Handle(DisableAgentCommand request, CancellationToken cancellationToken)
        {
            var agent = await _unitOfWork.Agents.GetFirstAgentAsync(cancellationToken);

            if (agent == null)
            {
                return false;
            }

            await _websocketClientOrganizer.DisableEnableAgentAsync(
                agent.Uid,
                AgentStartStopType.Stop,
                cancellationToken);

            return true;
        }
    }
} 