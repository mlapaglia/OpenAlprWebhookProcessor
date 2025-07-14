using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Settings.Queries.GetIgnores
{
    public class GetIgnoresQueryHandler : IRequestHandler<GetIgnoresQuery, List<IgnoreDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetIgnoresQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<IgnoreDto>> Handle(GetIgnoresQuery request, CancellationToken cancellationToken)
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