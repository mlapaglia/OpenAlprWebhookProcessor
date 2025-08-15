using Mediator;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetAgent;

namespace OpenAlprWebhookProcessor.Features.Settings.Commands.UpsertAgent
{
    public class UpsertAgentCommand : ICommand
    {
        public AgentDto Agent { get; set; }

        public UpsertAgentCommand(AgentDto agent)
        {
            Agent = agent;
        }
    }
} 