using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetLicensePlateCounts;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetStatistics;
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

        public async Task<IEnumerable<DayCount>> GetPlateCountsAsync(
            DateTimeOffset startDate, 
            DateTimeOffset endDate,
            CancellationToken cancellationToken = default)
        {
            var startFloor = new DateTimeOffset(startDate.Date, startDate.Offset);
            var endCeiling = new DateTimeOffset(endDate.Date, endDate.Offset).AddDays(1).AddTicks(-1);

            var startEpochMs = startFloor.ToUnixTimeMilliseconds();
            var endEpochMs = endCeiling.ToUnixTimeMilliseconds();

            var results = await _context.PlateGroups
                .AsNoTracking()
                .Where(x => x.ReceivedOnEpoch >= startEpochMs && x.ReceivedOnEpoch <= endEpochMs)
                .Select(y => y.ReceivedOnEpoch)
                .ToListAsync(cancellationToken);

            return GroupByDay(results, startDate.Offset);
        }

        public async Task<PlateGroup?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.PlateImage)
                .Include(x => x.VehicleImage)
                .Include(x => x.PossibleNumbers)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<List<long>> GetPlateStatisticsEpochsAsync(
            string plateNumber, 
            CancellationToken cancellationToken = default)
        {
            var seenPlates = await _dbSet
                .AsNoTracking()
                .Where(x => x.BestNumber == plateNumber || x.PossibleNumbers.Any(x => x.Number == plateNumber))
                .Select(x => x.ReceivedOnEpoch)
                .ToListAsync(cancellationToken);

            return seenPlates;
        }

        public async Task<PlateStatisticsAggregation> GetPlateStatisticsAggregationAsync(
            string plateNumber,
            long last90DaysEpoch,
            CancellationToken cancellationToken = default)
        {
            var bestNumberQuery = _dbSet
                .AsNoTracking()
                .Where(pg => pg.BestNumber == plateNumber)
                .Select(pg => pg.ReceivedOnEpoch);

            var possibleNumberQuery = _dbSet
                .AsNoTracking()
                .Join(_context.PlateGroupPossibleNumbers,
                    pg => pg.Id,
                    pn => pn.PlateGroupId,
                    (pg, pn) => new { pg, pn })
                .Where(x => x.pn.Number == plateNumber)
                .Select(x => x.pg.ReceivedOnEpoch);

            var allEpochsQuery = bestNumberQuery.Union(possibleNumberQuery);

            var result = await allEpochsQuery
                .GroupBy(e => 1)
                .Select(g => new PlateStatisticsAggregation
                {
                    TotalCount = g.Count(),
                    Last90DaysCount = g.Count(e => e > last90DaysEpoch),
                    MinEpoch = g.Min(),
                    MaxEpoch = g.Max()
                })
                .FirstOrDefaultAsync(cancellationToken);

            return result ?? new PlateStatisticsAggregation
            {
                TotalCount = 0,
                Last90DaysCount = 0,
                MinEpoch = 0,
                MaxEpoch = 0
            };
        }

        private static List<DayCount> GroupByDay(List<long> plateCounts, TimeSpan timeZoneOffset)
        {
            var groupedResults = plateCounts.GroupBy(x => 
            {
                var dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(x);

                var adjustedDateTime = dateTimeOffset.ToOffset(timeZoneOffset);
                return adjustedDateTime.Date;
            });
            
            var parsedResults = new List<DayCount>();

            foreach (var date in groupedResults)
            {
                parsedResults.Add(new DayCount()
                {
                    Count = date.Count(),
                    Date = new DateTimeOffset(date.Key, timeZoneOffset),
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