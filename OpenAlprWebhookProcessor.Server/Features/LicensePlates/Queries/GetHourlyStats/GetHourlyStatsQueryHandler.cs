using Mediator;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetHourlyStats
{
    public class GetHourlyStatsQueryHandler : IQueryHandler<GetHourlyStatsQuery, GetHourlyStatsResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetHourlyStatsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<GetHourlyStatsResponse> Handle(GetHourlyStatsQuery request, CancellationToken cancellationToken)
        {
            var thirtyDaysAgoEpoch = DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeMilliseconds();

            // Get raw epoch times first
            var plateEpochTimes = await _unitOfWork.PlateGroups.GetQueryable()
                .Where(pg => pg.ReceivedOnEpoch >= thirtyDaysAgoEpoch)
                .Select(pg => pg.ReceivedOnEpoch)
                .ToListAsync(cancellationToken);

            // Convert to DateTimeOffset and group by hour in memory
            var hourlyStats = plateEpochTimes
                .Select(epoch => DateTimeOffset.FromUnixTimeMilliseconds(epoch).Hour)
                .GroupBy(hour => hour)
                .Select(g => new HourlyCount
                {
                    Hour = g.Key,
                    Count = g.Count()
                })
                .OrderBy(h => h.Hour)
                .ToList();

            // Fill in missing hours with 0 count
            var allHours = Enumerable.Range(0, 24)
                .Select(hour => new HourlyCount
                {
                    Hour = hour,
                    Count = hourlyStats.FirstOrDefault(h => h.Hour == hour)?.Count ?? 0
                })
                .ToList();

            return new GetHourlyStatsResponse
            {
                Counts = allHours
            };
        }
    }
} 