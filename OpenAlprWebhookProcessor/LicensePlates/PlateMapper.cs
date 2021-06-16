using OpenAlprWebhookProcessor.Data;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace OpenAlprWebhookProcessor.LicensePlates
{
    public static class PlateMapper
    {
        public static LicensePlate MapPlate(
            PlateGroup plate,
            List<string> platesToIgnore,
            List<string> platesToAlert)
        {
            return new LicensePlate()
            {
                AlertDescription = plate.AlertDescription,
                Direction = plate.Direction,
                ImageUrl = new Uri($"/images/{plate.OpenAlprUuid}.jpg", UriKind.Relative),
                CropImageUrl = new Uri($"/images/crop/{plate.OpenAlprUuid}?{plate.PlateCoordinates}", UriKind.Relative),
                Id = plate.Id,
                IsAlert = platesToAlert.Contains(plate.BestNumber),
                IsIgnore = platesToIgnore.Contains(plate.BestNumber),
                OpenAlprCameraId = plate.OpenAlprCameraId,
                OpenAlprProcessingTimeMs = plate.OpenAlprProcessingTimeMs,
                PlateNumber = plate.BestNumber,
                PossiblePlateNumbers = !string.IsNullOrWhiteSpace(plate.PossibleNumbers) ? plate.PossibleNumbers.Replace(",", ", ") : string.Empty,
                ProcessedPlateConfidence = plate.Confidence,
                ReceivedOn = DateTimeOffset.FromUnixTimeMilliseconds(plate.ReceivedOnEpoch),
                Region = TranslateRegion(plate.Region),
                VehicleDescription = plate.VehicleDescription,
            };
        }

        private static string TranslateRegion(string openAlprRegion)
        {
            var splitCode = openAlprRegion.Split('-');
            var countryCode = splitCode[0];
            var stateCode = splitCode[1];

            var regionInfo = new RegionInfo(countryCode);
            return regionInfo.DisplayName + " " + stateCode.ToUpper();
        }
    }
}
