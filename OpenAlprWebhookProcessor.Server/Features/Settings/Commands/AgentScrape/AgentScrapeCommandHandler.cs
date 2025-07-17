using MediatR;
using OpenAlprWebhookProcessor.Hydrator;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Settings.Commands.AgentScrape
{
    public class AgentScrapeCommandHandler : IRequestHandler<AgentScrapeCommand>
    {
        private readonly IHydrationService _hydrationService;

        public AgentScrapeCommandHandler(IHydrationService hydrationService)
        {
            _hydrationService = hydrationService;
        }

        public Task Handle(AgentScrapeCommand request, CancellationToken cancellationToken)
        {
            _hydrationService.StartHydration("hydration");
            return Task.CompletedTask;
        }
    }
} 