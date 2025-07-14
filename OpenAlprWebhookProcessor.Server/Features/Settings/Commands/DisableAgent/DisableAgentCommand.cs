using MediatR;
using System;

namespace OpenAlprWebhookProcessor.Features.Settings.Commands.DisableAgent
{
    public class DisableAgentCommand : IRequest<bool>
    {
        public Guid AgentId { get; set; }

        public DisableAgentCommand(Guid agentId)
        {
            AgentId = agentId;
        }
    }
} 