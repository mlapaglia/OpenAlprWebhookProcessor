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
            var dbPlates = await _processerContext.PlateGroups
                .Where(x => x.PlateNumber == licensePlate)
                .ToListAsync(cancellationToken);

            var licensePlates = new List<LicensePlate>();

            foreach (var plate in dbPlates)
            {
                licensePlates.Add(new LicensePlate()
                {
                    PlateNumber = plate.PlateNumber,
                    LicensePlateJpegBase64 = plate.PlateJpeg,
                    OpenAlprCameraId = plate.OpenAlprCameraId,
                    OpenAlprProcessingTimeMs = plate.OpenAlprProcessingTimeMs,
                    ProcessedPlateConfidence = plate.PlateConfidence,
                    IsAlert = plate.IsAlert,
                    AlertDescription = plate.AlertDescription,
                });
            }

            return licensePlates;
        }
    }
}
