using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Data
{
    public class PlateGroup
    {
        public Guid Id { get; set; }

        public int OpenAlprCameraId { get; set; }

        public double OpenAlprProcessingTimeMs { get; set; }

        public bool IsAlert { get; set; }

        public string AlertDescription { get; set; }

        public DateTimeOffset ReceivedOn { get; set; }

        public string PlateNumber { get; set; }

        public string PlateJpeg { get; set; }

        public double PlateConfidence { get; set; }
    }
}
