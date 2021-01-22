using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.LicensePlates.GetLicensePlate
{
    public class GetLicensePlateHandler
    {
        private readonly ProcessorContext _processerContext;

        public GetLicensePlateHandler(
            ProcessorContext processorContext)
        {
            _processerContext = processorContext;
        }

        public async Task<List<LicensePlate>> GetLicensePlatesAsync(
            string licensePlate,
            CancellationToken cancellationToken)
        {
            var agent = await _processerContext.Agents.FirstOrDefaultAsync();

            var dbPlates = await _processerContext.PlateGroups
                .Where(x => x.Number == licensePlate)
                .ToListAsync(cancellationToken);

            var licensePlates = new List<LicensePlate>();

            foreach (var plate in dbPlates)
            {
                licensePlates.Add(PlateMapper.MapPlate(plate, agent));
            }

            return licensePlates;
        }

        public async Task<int> GetTotalNumberOfPlatesAsync(CancellationToken cancellationToken)
        {
            var query = _processerContext.PlateGroups.AsQueryable();

            var ignores = (await _processerContext.Ignores.ToListAsync())
                .Select(x => x.PlateNumber)
                .ToList();

            if (ignores.Count > 0)
            {
                query = query.Where(x => !ignores.Contains(x.Number));
            }

            return await query.CountAsync(cancellationToken);
        }

        public async Task<List<LicensePlate>> GetRecentPlatesAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var agent = await _processerContext.Agents.FirstOrDefaultAsync();

            var platesToIgnore = (await _processerContext.Ignores.ToListAsync(cancellationToken))
                .Select(x => x.PlateNumber)
                .ToList();

            var dbPlatesQuery = _processerContext.PlateGroups.AsQueryable();

            if (platesToIgnore.Count > 0)
            {
                dbPlatesQuery = dbPlatesQuery.Where(x => !platesToIgnore.Contains(x.Number));
            }

            var dbPlates = await dbPlatesQuery
                .OrderByDescending(x => x.ReceivedOnEpoch)
                .Skip(pageNumber * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var licensePlates = new List<LicensePlate>();

            foreach (var plate in dbPlates)
            {
                licensePlates.Add(PlateMapper.MapPlate(plate, agent));
            }

            return licensePlates;
        }
    }
}
