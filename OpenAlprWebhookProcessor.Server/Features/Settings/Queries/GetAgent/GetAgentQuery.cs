using MediatR;
using System.Threading;

namespace OpenAlprWebhookProcessor.Features.Settings.Queries.GetAgent
{
    public class GetAgentQuery : IRequest<AgentDto>
    {
    }
} 