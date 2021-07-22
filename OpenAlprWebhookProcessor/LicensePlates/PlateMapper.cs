using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Utilities;
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
                CanBeEnriched = !plate.IsEnriched,
                CropImageUrl = new Uri($"/images/crop/{plate.OpenAlprUuid}?{plate.PlateCoordinates}", UriKind.Relative),
                Id = plate.Id,
                IsAlert = platesToAlert.Contains(plate.BestNumber),
                IsIgnore = platesToIgnore.Contains(plate.BestNumber),
                Notes = plate.Notes,
                OpenAlprCameraId = plate.OpenAlprCameraId,
                OpenAlprProcessingTimeMs = plate.OpenAlprProcessingTimeMs,
                PlateNumber = plate.BestNumber,
                PossiblePlateNumbers = !string.IsNullOrWhiteSpace(plate.PossibleNumbers) ? plate.PossibleNumbers.Replace(",", ", ") : string.Empty,
                ProcessedPlateConfidence = plate.Confidence,
                ReceivedOn = DateTimeOffset.FromUnixTimeMilliseconds(plate.ReceivedOnEpoch),
                Region = TryTranslateRegion(plate.VehicleRegion),
                VehicleDescription = VehicleUtilities.FormatVehicleDescription(plate.VehicleYear + " " + plate.VehicleMakeModel),
            };
        }

        private static string TryTranslateRegion(string openAlprRegion)
        {
            if(openAlprRegion == null)
            {
                return string.Empty;
            }

            try
            {
                var splitCode = openAlprRegion.Split('-');
                var countryCode = splitCode[0];
                var stateCode = splitCode[1];

                var regionInfo = new RegionInfo(countryCode);
                return regionInfo.DisplayName + " " + stateCode.ToUpper();
            }
            catch
            {
                return openAlprRegion;
            }
        }
    }
}
