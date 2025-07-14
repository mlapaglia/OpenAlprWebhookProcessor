using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Settings.Commands.UpsertEnrichers
{
    public class UpsertEnrichersCommandHandler : IRequestHandler<UpsertEnrichersCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpsertEnrichersCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpsertEnrichersCommand request, CancellationToken cancellationToken)
        {
            var enricher = request.Enricher;
            var dbEnricher = await _unitOfWork.Enrichers.FirstOrDefaultAsync(x => x.Id == enricher.Id, cancellationToken);

            if (dbEnricher == null)
            {
                dbEnricher = new Data.Enricher()
                {
                    ApiKey = enricher.ApiKey,
                    EnricherType = enricher.EnricherType,
                    IsEnabled = enricher.IsEnabled,
                    EnrichmentType = enricher.EnrichmentType,
                };

                await _unitOfWork.Enrichers.AddAsync(dbEnricher, cancellationToken);
            }
            else
            {
                dbEnricher.ApiKey = enricher.ApiKey;
                dbEnricher.EnricherType = enricher.EnricherType;
                dbEnricher.IsEnabled = enricher.IsEnabled;
                dbEnricher.EnrichmentType = enricher.EnrichmentType;

                _unitOfWork.Enrichers.Update(dbEnricher);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
} 