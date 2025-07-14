using MediatR;
using OpenAlprWebhookProcessor.Hydrator;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Settings.Commands.AgentScrape
{
    public class AgentScrapeCommandHandler : IRequestHandler<AgentScrapeCommand>
    {
        private readonly HydrationService _hydrationService;

        public AgentScrapeCommandHandler(HydrationService hydrationService)
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