﻿using OpenAlprWebhookProcessor.Cameras.Configuration;
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.Cameras
{
    public class Camera
    {
        public Guid Id { get; set; }

        public int PlatesSeen { get; set; }

        public string SampleImageUrl { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [DisplayName("Manufacturer")]
        public CameraManufacturer Manufacturer { get; set; }

        public string ModelNumber { get; set; }

        public string OpenAlprName { get; set; }

        public long OpenAlprCameraId { get; set; }

        public string CameraPassword { get; set; }

        public string CameraUsername { get; set; }

        public string UpdateOverlayTextUrl { get; set; }

        public bool UpdateOverlayEnabled { get; set; }

        public string DayNightModeUrl { get; set; }

        public bool DayNightModeEnabled { get; set; }

        public decimal? DayZoom { get; set; }

        public decimal? DayFocus { get; set; }

        public decimal? NightZoom { get; set; }

        public decimal? NightFocus { get; set; }

        public int? SunsetOffset { get; set; }

        public int? SunriseOffset { get; set; }

        public double? TimezoneOffset { get; set; }

        public DateTimeOffset? DayNightNextScheduledCommand { get; set; }

        public string IpAddress { get; set; }

        public bool OpenAlprEnabled { get; set; }
    }
}
