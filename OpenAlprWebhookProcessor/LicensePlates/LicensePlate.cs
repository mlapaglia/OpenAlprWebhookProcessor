using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.LicensePlates
{
    public class LicensePlate
    {
        public int OpenAlprCameraId { get; set; }

        public string VehicleDescription { get; set; }

        public string PlateNumber { get; set; }

        public double OpenAlprProcessingTimeMs { get; set; }

        public double ProcessedPlateConfidence { get; set; }

        public string LicensePlateJpegBase64 { get; set; }

        public bool IsAlert { get; set; }

        public string AlertDescription { get; set; }
    }
}
