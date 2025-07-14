using MediatR;
using System;

namespace OpenAlprWebhookProcessor.Features.Settings.Commands.EnableAgent
{
    public class EnableAgentCommand : IRequest<bool>
    {
        public Guid AgentId { get; set; }

        public EnableAgentCommand(Guid agentId)
        {
            AgentId = agentId;
        }
    }
} 