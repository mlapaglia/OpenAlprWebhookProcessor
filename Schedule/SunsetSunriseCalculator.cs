using CoordinateSharp;
using System;

namespace OpenAlprWebhookProcessor.Schedule
{
    public static class SunsetSunriseCalculator
    {
        public static DateTime? CalculateSunsetTime(
            double longitude,
            double latitude)
        {
            var coordinate = new Coordinate(
                latitude,
                longitude,
                DateTime.UtcNow);

            return coordinate.CelestialInfo.SunRise;
        }
    }
}
