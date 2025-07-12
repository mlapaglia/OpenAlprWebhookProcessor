using MediatR;
using OpenAlprWebhookProcessor.LicensePlates;
using System;

namespace OpenAlprWebhookProcessor.Features.LicensePlates.Commands.UpsertPlate
{
    public class UpsertPlateCommand : IRequest
    {
        public Guid Id { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsAlert { get; set; }
        public bool IsIgnore { get; set; }
        public string VehicleColor { get; set; } = string.Empty;
        public string VehicleMake { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public string VehicleRegion { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public long ReceivedOnEpoch { get; set; }
        public bool IsStrictMatch { get; set; }
        public bool IsPatternMatch { get; set; }
        public bool IsEnriched { get; set; }
        public string? EnrichedData { get; set; }
        public string? Notes { get; set; }
    }
} 