using Mediator;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Settings.Queries.GetDebugPlates
{
    public class GetDebugPlatesQueryHandler : IQueryHandler<GetDebugPlatesQuery, string>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetDebugPlatesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<string> Handle(GetDebugPlatesQuery request, CancellationToken cancellationToken = default)
        {
            var query = _unitOfWork.RawPlateGroups.GetQueryable()
                .AsNoTracking();

            var stopEpoch = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeMilliseconds();

            query = query.Where(x => x.ReceivedOnEpoch > stopEpoch);

            if (request.OnlyFailedPlateGroups)
            {
                query = query.Where(x => !x.WasProcessedCorrectly);
            }

            var results = await query
                .Select(x => x.RawPlateGroup)
                .ToListAsync(cancellationToken);

            return "[" + String.Join(",", results.Take(10).ToList()) + "]";
        }
    }
} 