using OpenAlprWebhookProcessor.Data;
using System;

namespace OpenAlprWebhookProcessor.LicensePlates
{
    public static class PlateMapper
    {
        public static LicensePlate MapPlate(
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
