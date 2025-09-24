using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.MachineLearning.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Services
{
    /// <summary>
    /// Extracts features from historical license plate data for ML training and prediction.
    /// Transforms raw PlateGroup data into structured features for time-series prediction.
    /// </summary>
    public class LicensePlateFeatureExtractor : ILicensePlateFeatureExtractor
    {
        private readonly IUnitOfWork _unitOfWork;

        public LicensePlateFeatureExtractor(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<LicensePlateTrainingData>> ExtractTrainingDataAsync(
            int batchSize = 10000, 
            CancellationToken cancellationToken = default)
        {
            var trainingData = new List<LicensePlateTrainingData>();
            
            var plateGroups = await _unitOfWork.PlateGroups.GetQueryable()
                .Where(pg => !string.IsNullOrEmpty(pg.BestNumber))
                .OrderBy(pg => pg.ReceivedOnEpoch)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            var groupedByPlate = plateGroups
                .GroupBy(pg => pg.BestNumber)
                .Where(g => g.Count() > 2) // Need at least 3 sightings for meaningful patterns
                .ToList();

            foreach (var plateGroup in groupedByPlate)
            {
                var sightings = plateGroup.OrderBy(pg => pg.ReceivedOnEpoch).ToList();
                
                for (int i = 0; i < sightings.Count - 1; i++)
                {
                    var current = sightings[i];
                    var next = sightings[i + 1];
                    
                    var currentTime = DateTimeOffset.FromUnixTimeMilliseconds(current.ReceivedOnEpoch).DateTime;
                    var nextTime = DateTimeOffset.FromUnixTimeMilliseconds(next.ReceivedOnEpoch).DateTime;
                    var hoursUntilNext = (float)(nextTime - currentTime).TotalHours;

                    if (hoursUntilNext > 0 && hoursUntilNext < 96) // Include intervals less than 4 days (96 hours)
                    {
                        var features = ExtractFeatures(current, sightings.Take(i + 1).ToList());
                        features.HoursUntilNextSeen = hoursUntilNext;
                        trainingData.Add(features);
                    }
                }
            }

            return trainingData;
        }

        public async Task<LicensePlateTrainingData> ExtractFeaturesForPredictionAsync(
            LicensePlateInput input, 
            CancellationToken cancellationToken = default)
        {
            var historicalData = await _unitOfWork.PlateGroups.GetQueryable()
                .Where(pg => pg.BestNumber == input.LicensePlate)
                .OrderBy(pg => pg.ReceivedOnEpoch)
                .ToListAsync(cancellationToken);

            if (!historicalData.Any())
            {
                return CreateDefaultFeatures(input);
            }

            var mostRecent = historicalData.Last();
            return ExtractFeatures(mostRecent, historicalData);
        }

        private static LicensePlateTrainingData ExtractFeatures(
            PlateGroup plateGroup,
            List<PlateGroup> historicalSightings)
        {
            var currentTime = DateTimeOffset.FromUnixTimeMilliseconds(plateGroup.ReceivedOnEpoch).DateTime;
            
            // Time-based features
            var hourOfDay = currentTime.Hour;
            var dayOfWeek = (int)currentTime.DayOfWeek;
            var dayOfMonth = currentTime.Day;
            var monthOfYear = currentTime.Month;

            // Historical pattern features
            var timeSinceLastSeen = CalculateTimeSinceLastSeen(historicalSightings, plateGroup);
            var historicalFrequency = CalculateHistoricalFrequency(historicalSightings);
            var averageTimeBetweenVisits = CalculateAverageTimeBetweenVisits(historicalSightings);
            var totalVisits = historicalSightings.Count;

            // Context features
            var isWeekend = currentTime.DayOfWeek == DayOfWeek.Saturday || currentTime.DayOfWeek == DayOfWeek.Sunday;
            var isBusinessHour = hourOfDay >= 8 && hourOfDay <= 18 && !isWeekend;
            var seasonalFactor = CalculateSeasonalFactor(currentTime);

            // Vehicle features
            var vehicleTypeCode = EncodeVehicleType(plateGroup.VehicleType);
            var vehicleColorCode = EncodeVehicleColor(plateGroup.VehicleColor);

            return new LicensePlateTrainingData
            {
                LicensePlate = plateGroup.BestNumber,
                HourOfDay = hourOfDay,
                DayOfWeek = dayOfWeek,
                DayOfMonth = dayOfMonth,
                MonthOfYear = monthOfYear,
                CameraId = plateGroup.OpenAlprCameraId,
                TimeSinceLastSeen = timeSinceLastSeen,
                HistoricalFrequency = historicalFrequency,
                AverageTimeBetweenVisits = averageTimeBetweenVisits,
                TotalVisits = totalVisits,
                IsWeekend = isWeekend ? 1 : 0,
                IsBusinessHour = isBusinessHour ? 1 : 0,
                SeasonalFactor = seasonalFactor,
                VehicleTypeCode = vehicleTypeCode,
                VehicleColorCode = vehicleColorCode
            };
        }

        private static LicensePlateTrainingData CreateDefaultFeatures(LicensePlateInput input)
        {
            var currentTime = DateTime.UtcNow;
            var isWeekend = currentTime.DayOfWeek == DayOfWeek.Saturday || currentTime.DayOfWeek == DayOfWeek.Sunday;
            var isBusinessHour = currentTime.Hour >= 8 && currentTime.Hour <= 18 && !isWeekend;

            return new LicensePlateTrainingData
            {
                LicensePlate = input.LicensePlate,
                HourOfDay = currentTime.Hour,
                DayOfWeek = (int)currentTime.DayOfWeek,
                DayOfMonth = currentTime.Day,
                MonthOfYear = currentTime.Month,
                CameraId = input.CameraId,
                TimeSinceLastSeen = (float)(currentTime - input.LastSeen).TotalHours,
                HistoricalFrequency = 0.1f, // Default low frequency for new plates
                AverageTimeBetweenVisits = 168f, // Default to weekly
                TotalVisits = 1,
                IsWeekend = isWeekend ? 1 : 0,
                IsBusinessHour = isBusinessHour ? 1 : 0,
                SeasonalFactor = CalculateSeasonalFactor(currentTime),
                VehicleTypeCode = EncodeVehicleType(input.VehicleType),
                VehicleColorCode = EncodeVehicleColor(input.VehicleColor)
            };
        }

        private static float CalculateTimeSinceLastSeen(List<PlateGroup> sightings, PlateGroup current)
        {
            if (sightings.Count <= 1) return 0;

            var previous = sightings[^2]; // Second to last
            var currentTime = DateTimeOffset.FromUnixTimeMilliseconds(current.ReceivedOnEpoch);
            var previousTime = DateTimeOffset.FromUnixTimeMilliseconds(previous.ReceivedOnEpoch);
            
            return (float)(currentTime - previousTime).TotalHours;
        }

        private static float CalculateHistoricalFrequency(List<PlateGroup> sightings)
        {
            if (sightings.Count <= 1) return 0;

            var firstSighting = DateTimeOffset.FromUnixTimeMilliseconds(sightings.First().ReceivedOnEpoch);
            var lastSighting = DateTimeOffset.FromUnixTimeMilliseconds(sightings.Last().ReceivedOnEpoch);
            var totalDays = (lastSighting - firstSighting).TotalDays;

            return totalDays > 0 ? (float)(sightings.Count / totalDays) : 0;
        }

        private static float CalculateAverageTimeBetweenVisits(List<PlateGroup> sightings)
        {
            if (sightings.Count <= 1) return 0;

            var intervals = new List<double>();
            for (int i = 1; i < sightings.Count; i++)
            {
                var current = DateTimeOffset.FromUnixTimeMilliseconds(sightings[i].ReceivedOnEpoch);
                var previous = DateTimeOffset.FromUnixTimeMilliseconds(sightings[i-1].ReceivedOnEpoch);
                intervals.Add((current - previous).TotalHours);
            }

            return intervals.Any() ? (float)intervals.Average() : 0;
        }

        private static float CalculateSeasonalFactor(DateTime dateTime)
        {
            var dayOfYear = dateTime.DayOfYear;
            // Simple seasonal calculation - peaks in summer/winter
            return (float)(Math.Sin(2 * Math.PI * dayOfYear / 365.25) * 0.5 + 0.5);
        }

        private static float EncodeVehicleType(string vehicleType)
        {
            return vehicleType?.ToLowerInvariant() switch
            {
                "car" => 1,
                "truck" => 2,
                "suv" => 3,
                "van" => 4,
                "motorcycle" => 5,
                "bus" => 6,
                _ => 0
            };
        }

        private static float EncodeVehicleColor(string vehicleColor)
        {
            return vehicleColor?.ToLowerInvariant() switch
            {
                "white" => 1,
                "black" => 2,
                "silver" => 3,
                "gray" => 4,
                "red" => 5,
                "blue" => 6,
                "green" => 7,
                "yellow" => 8,
                "brown" => 9,
                _ => 0
            };
        }
    }
} 