using Mediator;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetQuickStats
{
    public class GetQuickStatsQueryHandler : IQueryHandler<GetQuickStatsQuery, GetQuickStatsResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetQuickStatsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<GetQuickStatsResponse> Handle(GetQuickStatsQuery request, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;
            var todayUtc = DateTime.UtcNow.Date;
            var todayStartEpoch = new DateTimeOffset(todayUtc, TimeSpan.Zero).ToUnixTimeMilliseconds();
            var tomorrowStartEpoch = new DateTimeOffset(todayUtc.AddDays(1), TimeSpan.Zero).ToUnixTimeMilliseconds();
            var weekAgoEpoch = now.AddDays(-7).ToUnixTimeMilliseconds();
            var monthAgoEpoch = now.AddDays(-30).ToUnixTimeMilliseconds();

            var plateGroups = _unitOfWork.PlateGroups.GetQueryable();

            var todayCount = await plateGroups
                .Where(pg => pg.ReceivedOnEpoch >= todayStartEpoch && pg.ReceivedOnEpoch < tomorrowStartEpoch)
                .CountAsync(cancellationToken);

            var weekCount = await plateGroups
                .Where(pg => pg.ReceivedOnEpoch >= weekAgoEpoch)
                .CountAsync(cancellationToken);

            var monthCount = await plateGroups
                .Where(pg => pg.ReceivedOnEpoch >= monthAgoEpoch)
                .CountAsync(cancellationToken);

            var uniquePlatesThisWeek = await plateGroups
                .Where(pg => pg.ReceivedOnEpoch >= weekAgoEpoch)
                .Select(pg => pg.BestNumber)
                .Distinct()
                .CountAsync(cancellationToken);

            var activeCameras = await plateGroups
                .Where(pg => pg.ReceivedOnEpoch >= weekAgoEpoch)
                .Where(pg => pg.OpenAlprCameraId > 0)
                .Select(pg => pg.OpenAlprCameraId)
                .Distinct()
                .CountAsync(cancellationToken);

            var last30DaysPlates = await plateGroups
                .Where(pg => pg.ReceivedOnEpoch >= monthAgoEpoch)
                .CountAsync(cancellationToken);

            var averageDailyPlates = last30DaysPlates > 0 ? (int)(last30DaysPlates / 30.0) : 0;

            return new GetQuickStatsResponse
            {
                TodayCount = todayCount,
                WeekCount = weekCount,
                MonthCount = monthCount,
                UniquePlatesThisWeek = uniquePlatesThisWeek,
                ActiveCameras = activeCameras,
                AverageDailyPlates = averageDailyPlates
            };
        }
    }
} 