using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OpenAlprWebhookProcessor.Utilities
{
    public static class VehicleUtilities
    {
        private static readonly TextInfo _textInfo = CultureInfo.CurrentCulture.TextInfo;

        public static string FormatVehicleDescription(string vehicleMakeModel)
        {
            if (vehicleMakeModel == null)
            {
                return string.Empty;
            }

            return _textInfo
                .ToTitleCase(vehicleMakeModel.Replace('_', ' '));
        }

        public static string FormatLicensePlateImageCoordinates(
            List<int> xCoordinates,
            List<int> yCoordinates)
        {
            return $"x1={xCoordinates.Min()}&y1={yCoordinates.Min()}&x2={xCoordinates.Max()}&y2={yCoordinates.Max()}";
        }
    }
}
