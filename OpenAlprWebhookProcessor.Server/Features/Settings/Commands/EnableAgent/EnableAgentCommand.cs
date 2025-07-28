using Mediator;
using System;

namespace OpenAlprWebhookProcessor.Features.Settings.Commands.EnableAgent
{
    public class EnableAgentCommand : IQuery<bool>
    {
        public Guid AgentId { get; set; }

        public EnableAgentCommand(Guid agentId)
        {
            AgentId = agentId;
        }
    }
} 