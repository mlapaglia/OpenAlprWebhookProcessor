using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetLicensePlateCounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Data.Repositories
{
    public class PlateGroupRepository : Repository<PlateGroup>, IPlateGroupRepository
    {
        public PlateGroupRepository(ProcessorContext context) : base(context)
        {
        }

        public async Task<IEnumerable<PlateGroup>> SearchPlatesAsync(
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
            CancellationToken cancellationToken = default)
        {
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var query = BuildSearchQuery(
                plateNumber, 
                strictMatch, 
                regexSearchEnabled, 
                startDate, 
                endDate, 
                platesToIgnore,
                vehicleColor,
                vehicleMake,
                vehicleModel,
                vehicleType,
                vehicleRegion,
                filterPlatesSeenLessThan);

            var results = await query
                .Include(x => x.PossibleNumbers)
                .OrderByDescending(x => x.ReceivedOnEpoch)
                .Skip(pageNumber * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return results;
        }

        public async Task<int> GetSearchResultsCountAsync(
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
            CancellationToken cancellationToken = default)
        {
            var query = BuildSearchQuery(
                plateNumber, 
                strictMatch, 
                regexSearchEnabled, 
                startDate, 
                endDate, 
                platesToIgnore,
                vehicleColor,
                vehicleMake,
                vehicleModel,
                vehicleType,
                vehicleRegion,
                filterPlatesSeenLessThan);

            return await query.CountAsync(cancellationToken);
        }

        public async Task<IEnumerable<PlateGroup>> GetMostSeenPlatesAsync(
            DateTimeOffset? startDate, 
            DateTimeOffset? endDate, 
            int limit,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();

            if (startDate.HasValue)
            {
                var startEpochMs = startDate.Value.ToUnixTimeMilliseconds();
                query = query.Where(x => x.ReceivedOnEpoch >= startEpochMs);
            }

            if (endDate.HasValue)
            {
                var endEpochMs = endDate.Value.ToUnixTimeMilliseconds();
                query = query.Where(x => x.ReceivedOnEpoch <= endEpochMs);
            }

            return await query
                .GroupBy(x => x.BestNumber)
                .Select(g => new { PlateNumber = g.Key, Count = g.Count(), Latest = g.OrderByDescending(x => x.ReceivedOnEpoch).First() })
                .OrderByDescending(x => x.Count)
                .Take(limit)
                .Select(x => x.Latest)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<DayCount>> GetPlateCountsAsync(
            DateTimeOffset startDate, 
            DateTimeOffset endDate,
            CancellationToken cancellationToken = default)
        {
            var startEpochMs = startDate.ToUnixTimeMilliseconds();
            var endEpochMs = endDate.ToUnixTimeMilliseconds();

            var results = await _context.PlateGroups
                .AsNoTracking()
                .Where(x => x.ReceivedOnEpoch >= startEpochMs && x.ReceivedOnEpoch <= endEpochMs)
                .Select(y => y.ReceivedOnEpoch)
                .ToListAsync(cancellationToken);

            return GroupByDay(results);
        }

        public async Task<PlateGroup?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.PlateImage)
                .Include(x => x.VehicleImage)
                .Include(x => x.PossibleNumbers)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<(List<long> seenPlates, List<long> seenPossiblePlates)> GetPlateStatisticsEpochsAsync(
            string plateNumber, 
            CancellationToken cancellationToken = default)
        {
            var seenPlates = await _dbSet
                .AsNoTracking()
                .Where(x => x.BestNumber == plateNumber)
                .Select(x => x.ReceivedOnEpoch)
                .ToListAsync(cancellationToken);

            var seenPossiblePlates = await _context.PlateGroupPossibleNumbers
                .AsNoTracking()
                .Where(x => x.Number == plateNumber)
                .Select(x => x.PlateGroup.ReceivedOnEpoch)
                .ToListAsync(cancellationToken);

            return (seenPlates, seenPossiblePlates);
        }

        private static List<DayCount> GroupByDay(List<long> plateCounts)
        {
            var groupedResults = plateCounts.GroupBy(x => DateTimeOffset.FromUnixTimeMilliseconds(x).Date);
            var parsedResults = new List<DayCount>();

            foreach (var date in groupedResults)
            {
                parsedResults.Add(new DayCount()
                {
                    Count = date.Count(),
                    Date = date.Key,
                });
            }

            return parsedResults;
        }

        private IQueryable<PlateGroup> BuildSearchQuery(
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
            int filterPlatesSeenLessThan)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(plateNumber))
            {
                plateNumber = plateNumber.Trim().ToUpper();

                if (strictMatch)
                {
                    query = query.Where(x => x.BestNumber == plateNumber);
                }
                else if (regexSearchEnabled)
                {
                    query = query.Where(x => Regex.IsMatch(x.BestNumber, plateNumber));
                }
                else
                {
                    query = query.Where(x => x.PossibleNumbers.Any(pn => pn.Number == plateNumber) || plateNumber == x.BestNumber);
                }
            }

            if (startDate.HasValue)
            {
                var startEpochMs = startDate.Value.ToUnixTimeMilliseconds();
                query = query.Where(x => x.ReceivedOnEpoch >= startEpochMs);
            }

            if (endDate.HasValue)
            {
                var endEpochMs = endDate.Value.ToUnixTimeMilliseconds();
                query = query.Where(x => x.ReceivedOnEpoch <= endEpochMs);
            }

            if (platesToIgnore.Any())
            {
                query = query.Where(x => !x.PossibleNumbers.Any(pn => platesToIgnore.Contains(pn.Number)) && !platesToIgnore.Contains(x.BestNumber));
            }

            if (!string.IsNullOrWhiteSpace(vehicleColor))
            {
                query = query.Where(x => x.VehicleColor.Contains(vehicleColor.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(vehicleMake))
            {
                query = query.Where(x => x.VehicleMakeModel.Contains(vehicleMake.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(vehicleModel))
            {
                query = query.Where(x => x.VehicleMakeModel.Contains(vehicleModel.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(vehicleType))
            {
                query = query.Where(x => x.VehicleType.Contains(vehicleType.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(vehicleRegion))
            {
                query = query.Where(x => x.VehicleRegion.Contains(vehicleRegion.ToLower()));
            }

            if (filterPlatesSeenLessThan > 0)
            {
                var platesSeen = _context.PlateGroups
                    .GroupBy(x => x.BestNumber)
                    .Where(x => x.Count() > filterPlatesSeenLessThan)
                    .Select(x => x.Key);

                query = query.Where(x => !platesSeen.Contains(x.BestNumber));
            }

            return query;
        }
    }
} 