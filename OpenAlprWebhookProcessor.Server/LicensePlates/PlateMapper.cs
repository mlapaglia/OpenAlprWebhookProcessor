using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OpenAlprWebhookProcessor.LicensePlates
{
    public static class PlateMapper
    {
        public static LicensePlate MapPlate(
            PlateGroup plate,
            List<string> platesToIgnore = null,
            List<string> platesToAlert = null)
        {
            return new LicensePlate()
            {
                AlertDescription = plate.AlertDescription,
                Direction = plate.Direction,
                ImageUrl = new Uri($"/api/images/{plate.OpenAlprUuid}", UriKind.Relative),
                CanBeEnriched = !plate.IsEnriched,
                CropImageUrl = new Uri($"/api/images/crop/{plate.OpenAlprUuid}", UriKind.Relative),
                Id = plate.Id,
                IsAlert = platesToAlert?.Contains(plate.BestNumber) ?? false,
                IsIgnore = platesToIgnore?.Contains(plate.BestNumber) ?? false,
                Notes = plate.Notes,
                OpenAlprCameraId = plate.OpenAlprCameraId,
                OpenAlprProcessingTimeMs = plate.OpenAlprProcessingTimeMs,
                PlateNumber = plate.BestNumber,
                PossiblePlateNumbers = string.Join(", ", plate.PossibleNumbers.Select(x => x.Number).ToList()),
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
