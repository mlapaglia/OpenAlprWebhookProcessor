using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetStatistics
{
    public class GetStatisticsQueryHandler : IRequestHandler<GetStatisticsQuery, PlateStatistics>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetStatisticsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PlateStatistics> Handle(
            GetStatisticsQuery request,
            CancellationToken cancellationToken = default)
        {
            var endingEpoch = DateTimeOffset.UtcNow.AddDays(-90).ToUnixTimeMilliseconds();
            var plateNumber = request.PlateNumber;

            var (seenPlates, seenPossiblePlates) = await _unitOfWork.PlateGroups.GetPlateStatisticsEpochsAsync(
                plateNumber, cancellationToken);

            seenPlates.AddRange(seenPossiblePlates);
            seenPlates = seenPlates.OrderBy(x => x).ToList();

            var plateStatistics = new PlateStatistics
            {
                TotalSeen = seenPlates.Count,
                Last90Days = seenPlates.Count(x => x > endingEpoch)
            };

            var firstSeenEpoch = seenPlates.FirstOrDefault();
            if (firstSeenEpoch != 0)
            {
                plateStatistics.FirstSeen = DateTimeOffset.FromUnixTimeMilliseconds(firstSeenEpoch);
            }

            var lastSeenEpoch = seenPlates.LastOrDefault();
            if (lastSeenEpoch != 0)
            {
                plateStatistics.LastSeen = DateTimeOffset.FromUnixTimeMilliseconds(lastSeenEpoch);
            }

            return plateStatistics;
        }
    }
} 