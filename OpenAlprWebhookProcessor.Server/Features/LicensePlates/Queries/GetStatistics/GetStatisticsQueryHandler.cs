using Mediator;
using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetStatistics
{
    public class GetStatisticsQueryHandler : IQueryHandler<GetStatisticsQuery, PlateStatistics>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetStatisticsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<PlateStatistics> Handle(
            GetStatisticsQuery request,
            CancellationToken cancellationToken = default)
        {
            var endingEpoch = DateTimeOffset.UtcNow.AddDays(-90).ToUnixTimeMilliseconds();
            var plateNumber = request.PlateNumber;

            var aggregation = await _unitOfWork.PlateGroups.GetPlateStatisticsAggregationAsync(
                plateNumber, endingEpoch, cancellationToken);

            var plateStatistics = new PlateStatistics
            {
                TotalSeen = aggregation.TotalCount,
                Last90Days = aggregation.Last90DaysCount
            };

            if (aggregation.MinEpoch != 0)
            {
                plateStatistics.FirstSeen = DateTimeOffset.FromUnixTimeMilliseconds(aggregation.MinEpoch);
            }

            if (aggregation.MaxEpoch != 0)
            {
                plateStatistics.LastSeen = DateTimeOffset.FromUnixTimeMilliseconds(aggregation.MaxEpoch);
            }

            return plateStatistics;
        }
    }
} 