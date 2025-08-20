using Mediator;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Ignores.Queries.GetIgnores
{
    public class GetIgnoresQueryHandler : IQueryHandler<GetIgnoresQuery, List<IgnoreDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetIgnoresQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<List<IgnoreDto>> Handle(GetIgnoresQuery query, CancellationToken cancellationToken = default)
        {
            var dbIgnores = await _unitOfWork.Ignores.GetAllAsync(cancellationToken);
            var ignores = new List<IgnoreDto>();

            foreach (var dbIgnore in dbIgnores)
            {
                var ignore = new IgnoreDto()
                {
                    Id = dbIgnore.Id,
                    PlateNumber = dbIgnore.PlateNumber,
                    StrictMatch = dbIgnore.IsStrictMatch,
                    Description = dbIgnore.Description,
                };

                ignores.Add(ignore);
            }

            return ignores;
        }
    }
} 