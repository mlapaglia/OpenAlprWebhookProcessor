using Mediator;
using OpenAlprWebhookProcessor.Hydrator;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Settings.Commands.AgentScrape
{
    public class AgentScrapeCommandHandler : ICommandHandler<AgentScrapeCommand>
    {
        private readonly IHydrationService _hydrationService;

        public AgentScrapeCommandHandler(IHydrationService hydrationService)
        {
            _hydrationService = hydrationService;
        }

        public async ValueTask<Unit> Handle(AgentScrapeCommand request, CancellationToken cancellationToken)
        {
            _hydrationService.StartHydration("hydration");
            return await Task.FromResult(Unit.Value);
        }
    }
} 