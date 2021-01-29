using OpenAlprWebhookProcessor.Cameras.Configuration;
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.Settings
{
    public class Camera
    {
        public Guid Id { get; set; }

        public int PlatesSeen { get; set; }

        public string SampleImageUrl { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [DisplayName("Manufacturer")]
        public CameraManufacturer Manufacturer { get; set; }

        public string ModelNumber { get; set; }

        public string OpenAlprName { get; set; }

        public long OpenAlprCameraId { get; set; }

        public string CameraPassword { get; set; }

        public string CameraUsername { get; set; }

        public string UpdateOverlayTextUrl { get; set; }
    }
}
