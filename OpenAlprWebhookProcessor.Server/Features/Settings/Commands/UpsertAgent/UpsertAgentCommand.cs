using MediatR;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetAgent;

namespace OpenAlprWebhookProcessor.Features.Settings.Commands.UpsertAgent
{
    public class UpsertAgentCommand : IRequest
    {
        public AgentDto Agent { get; set; }

        public UpsertAgentCommand(AgentDto agent)
        {
            Agent = agent;
        }
    }
} 