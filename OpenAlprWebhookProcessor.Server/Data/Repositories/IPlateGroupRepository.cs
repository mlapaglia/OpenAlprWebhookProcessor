using OpenAlprWebhookProcessor.Server.Features.LicensePlates.Queries.GetLicensePlateCounts;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Data.Repositories
{
    public interface IPlateGroupRepository : IRepository<PlateGroup>
    {
        Task<IEnumerable<PlateGroup>> SearchPlatesAsync(
            string? plateNumber, 
            bool strictMatch, 
            bool regexSearchEnabled, 
            DateTimeOffset? startDate, 
            DateTimeOffset? endDate, 
            List<string> platesToIgnore,
            string? vehicleColor,
            string? vehicleMake,
            string? vehicleModel,
            string? vehicleType,
            string? vehicleRegion,
            int filterPlatesSeenLessThan,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<int> GetSearchResultsCountAsync(
            string? plateNumber, 
            bool strictMatch, 
            bool regexSearchEnabled, 
            DateTimeOffset? startDate, 
            DateTimeOffset? endDate, 
            List<string> platesToIgnore,
            string? vehicleColor,
            string? vehicleMake,
            string? vehicleModel,
            string? vehicleType,
            string? vehicleRegion,
            int filterPlatesSeenLessThan,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<PlateGroup>> GetMostSeenPlatesAsync(
            DateTimeOffset? startDate, 
            DateTimeOffset? endDate, 
            int limit,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<DayCount>> GetPlateCountsAsync(
            DateTimeOffset startDate, 
            DateTimeOffset endDate,
            CancellationToken cancellationToken = default);

        Task<PlateGroup?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

        Task<(List<long> seenPlates, List<long> seenPossiblePlates)> GetPlateStatisticsEpochsAsync(
            string plateNumber, 
            CancellationToken cancellationToken = default);
    }
} 