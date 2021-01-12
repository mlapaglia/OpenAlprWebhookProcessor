using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Cameras.Configuration;
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

        private readonly AgentConfiguration _agentConfiguration;

        public GetLicensePlateHandler(
            ProcessorContext processorContext,
            AgentConfiguration agentConfiguration)
        {
            _processerContext = processorContext;
            _agentConfiguration = agentConfiguration;
        }

        public async Task<List<LicensePlate>> GetLicensePlatesAsync(
            string licensePlate,
            CancellationToken cancellationToken)
        {
            var dbPlates = await _processerContext.PlateGroups
                .Where(x => x.Number == licensePlate)
                .ToListAsync(cancellationToken);

            var licensePlates = new List<LicensePlate>();

            foreach (var plate in dbPlates)
            {
                licensePlates.Add(MapPlate(plate));
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
            var dbPlates = await _processerContext.PlateGroups
                .OrderByDescending(x => x.ReceivedOnEpoch)
                .Skip(pageNumber * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var licensePlates = new List<LicensePlate>();

            foreach (var plate in dbPlates)
            {
                licensePlates.Add(MapPlate(plate));
            }

            return licensePlates;
        }

        public LicensePlate MapPlate(PlateGroup plate)
        {
            return new LicensePlate()
            {
                AlertDescription = plate.AlertDescription,
                Direction = plate.Direction,
                ImageUrl = new Uri(Flurl.Url.Combine(_agentConfiguration.Endpoint.ToString(), $"/img/{plate.OpenAlprUuid}.jpg")),
                CropImageUrl = new Uri(Flurl.Url.Combine(_agentConfiguration.Endpoint.ToString(), $"/crop/{plate.OpenAlprUuid}?{plate.PlateCoordinates}")),
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
