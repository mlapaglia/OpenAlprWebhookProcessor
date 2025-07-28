using Mediator;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Settings.Queries.GetEnrichers
{
    public class GetEnrichersQueryHandler : IQueryHandler<GetEnrichersQuery, EnricherDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetEnrichersQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<EnricherDto> Handle(GetEnrichersQuery request, CancellationToken cancellationToken = default)
        {
            var enricher = await _unitOfWork.Enrichers.GetFirstAsync(cancellationToken);

            if (enricher == null)
            {
                return new EnricherDto();
            }

            return new EnricherDto()
            {
                ApiKey = enricher.ApiKey,
                EnricherType = enricher.EnricherType,
                IsEnabled = enricher.IsEnabled,
                Id = enricher.Id,
                EnrichmentType = enricher.EnrichmentType,
            };
        }
    }
} 