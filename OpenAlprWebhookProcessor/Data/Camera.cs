using OpenAlprWebhookProcessor.Cameras.Configuration;
using System;

namespace OpenAlprWebhookProcessor.Data
{
    public class Camera
    {
        public Guid Id { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public CameraManufacturer Manufacturer { get; set; }

        public string ModelNumber { get; set; }

        public string OpenAlprName { get; set; }

        public long OpenAlprCameraId { get; set; }

        public string CameraPassword { get; set; }

        public string CameraUsername { get; set; }

        public string UpdateOverlayTextUrl { get; set; }

        public bool UpdateOverlayEnabled { get; set; }

        public string UpdateDayNightModeUrl { get; set; }

        public string NextDayNightScheduleId { get; set; }

        public string NextClearOverlayScheduleId { get; set; }

        public bool UpdateDayNightModeEnabled { get; set; }

        public string LatestProcessedPlateUuid { get; set; }

        public int PlatesSeen { get; set; }

        public int? SunsetOffset { get; set; }

        public int? SunriseOffset { get; set; }

        public double? TimezoneOffset { get; set; }
    }
}
