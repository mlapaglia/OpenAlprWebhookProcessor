using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetMostSeenPlates
{
    public class GetMostSeenPlatesQueryHandler : IRequestHandler<GetMostSeenPlatesQuery, GetMostSeenPlatesResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetMostSeenPlatesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<GetMostSeenPlatesResponse> Handle(
            GetMostSeenPlatesQuery request,
            CancellationToken cancellationToken)
        {
            var startDate = request.StartDate ?? DateTimeOffset.UtcNow.AddDays(-30);
            var endDate = (request.EndDate ?? DateTimeOffset.UtcNow).AddDays(1).AddTicks(-1);

            var plateCounts = await _unitOfWork.PlateGroups.GetQueryable()
                .Where(x =>
                    x.ReceivedOnEpoch >= startDate.ToUnixTimeMilliseconds()
                    && x.ReceivedOnEpoch <= endDate.ToUnixTimeMilliseconds())
                .GroupBy(x => x.BestNumber)
                .Select(g => new MostSeenCount {
                    PlateNumber = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(request.Limit)
                .ToListAsync(cancellationToken);

            return new GetMostSeenPlatesResponse
            {
                Counts = plateCounts
            };
        }
    }
} 