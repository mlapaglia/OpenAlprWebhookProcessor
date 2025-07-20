using Microsoft.ML.Data;
using System;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Models
{
    /// <summary>
    /// Result of a license plate prediction containing the predicted time until next sighting.
    /// </summary>
    public class LicensePlatePrediction
    {
        [ColumnName("Score")]
        public float PredictedHours { get; set; }

        public DateTime PredictedNextSeen => DateTime.UtcNow.AddHours(PredictedHours);

        public double ConfidenceScore { get; set; }
    }

    /// <summary>
    /// Input model for making license plate predictions.
    /// Contains current features for a specific license plate.
    /// </summary>
    public class LicensePlateInput
    {
        public string LicensePlate { get; set; }
        public int CameraId { get; set; }
        public DateTime LastSeen { get; set; }
        public string VehicleType { get; set; }
        public string VehicleColor { get; set; }
    }

    /// <summary>
    /// Comprehensive prediction result with additional metadata.
    /// </summary>
    public class LicensePlatePredictionResult
    {
        public string LicensePlate { get; set; }
        public DateTime PredictedNextSeen { get; set; }
        public float PredictedHours { get; set; }
        public double ConfidenceScore { get; set; }
        public int TotalHistoricalVisits { get; set; }
        public double AverageTimeBetweenVisits { get; set; }
        public DateTime LastSeen { get; set; }
        public string ModelVersion { get; set; }
        public DateTime PredictionMadeAt { get; set; }
    }
} 