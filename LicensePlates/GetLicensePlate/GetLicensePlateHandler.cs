using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System;
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
                licensePlates.Add(MapPlate(plate, agent));
            }

            return licensePlates;
        }

        public async Task<int> GetTotalNumberOfPlatesAsync(CancellationToken cancellationToken)
        {
            return await _processerContext.PlateGroups.CountAsync(cancellationToken);
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
                licensePlates.Add(MapPlate(plate, agent));
            }

            return licensePlates;
        }

        public LicensePlate MapPlate(
            PlateGroup plate,
            Agent agent)
        {
            return new LicensePlate()
            {
                AlertDescription = plate.AlertDescription,
                Direction = plate.Direction,
                ImageUrl = new Uri(Flurl.Url.Combine(
                    agent.EndpointUrl,
                    $"/img/{plate.OpenAlprUuid}.jpg")),
                CropImageUrl = new Uri(Flurl.Url.Combine(
                    agent.EndpointUrl,
                    $"/crop/{plate.OpenAlprUuid}?{plate.PlateCoordinates}")),
                IsAlert = plate.IsAlert,
                LicensePlateJpegBase64 = plate.Jpeg,
                OpenAlprCameraId = plate.OpenAlprCameraId,
                OpenAlprProcessingTimeMs = plate.OpenAlprProcessingTimeMs,
                PlateNumber = plate.Number,
                ProcessedPlateConfidence = plate.Confidence,
                ReceivedOn = DateTimeOffset.FromUnixTimeMilliseconds(plate.ReceivedOnEpoch),
                VehicleDescription = plate.VehicleDescription,
            };
        }
    }
}
