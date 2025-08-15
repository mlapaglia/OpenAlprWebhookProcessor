using Microsoft.ML.Data;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Models
{
    /// <summary>
    /// Training data model for predicting when a license plate will next be seen.
    /// Contains features extracted from historical PlateGroup data.
    /// </summary>
    public class LicensePlateTrainingData
    {
        [LoadColumn(0)]
        public string LicensePlate { get; set; }

        [LoadColumn(1)]
        public float HourOfDay { get; set; }

        [LoadColumn(2)]
        public float DayOfWeek { get; set; }

        [LoadColumn(3)]
        public float DayOfMonth { get; set; }

        [LoadColumn(4)]
        public float MonthOfYear { get; set; }

        [LoadColumn(5)]
        public float CameraId { get; set; }

        [LoadColumn(6)]
        public float TimeSinceLastSeen { get; set; }

        [LoadColumn(7)]
        public float HistoricalFrequency { get; set; }

        [LoadColumn(8)]
        public float AverageTimeBetweenVisits { get; set; }

        [LoadColumn(9)]
        public float TotalVisits { get; set; }

        [LoadColumn(10)]
        public float IsWeekend { get; set; }

        [LoadColumn(11)]
        public float IsBusinessHour { get; set; }

        [LoadColumn(12)]
        public float SeasonalFactor { get; set; }

        [LoadColumn(13)]
        public float VehicleTypeCode { get; set; }

        [LoadColumn(14)]
        public float VehicleColorCode { get; set; }

        [LoadColumn(15)]
        [ColumnName("Label")]
        public float HoursUntilNextSeen { get; set; }
    }
} 