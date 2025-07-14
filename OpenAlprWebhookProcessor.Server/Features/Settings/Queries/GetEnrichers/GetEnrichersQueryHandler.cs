using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Settings.Queries.GetEnrichers
{
    public class GetEnrichersQueryHandler : IRequestHandler<GetEnrichersQuery, EnricherDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetEnrichersQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EnricherDto> Handle(GetEnrichersQuery request, CancellationToken cancellationToken)
        {
            var enrichers = await _unitOfWork.Enrichers.GetAllAsync(cancellationToken);
            var enricher = enrichers.FirstOrDefault();

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