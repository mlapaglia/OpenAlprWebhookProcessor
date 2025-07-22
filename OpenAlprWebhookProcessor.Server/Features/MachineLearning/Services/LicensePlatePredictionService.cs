using Microsoft.Extensions.Logging;
using Microsoft.ML;
using OpenAlprWebhookProcessor.Features.MachineLearning.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenAlprWebhookProcessor.Data.Repositories;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Services
{
    /// <summary>
    /// Service for making license plate predictions using trained ML models.
    /// Provides both single predictions and batch predictions with confidence scores.
    /// </summary>
    public class LicensePlatePredictionService : ILicensePlatePredictionService
    {
        private readonly ILicensePlateMlTrainingService _trainingService;
        private readonly ILicensePlateFeatureExtractor _featureExtractor;
        private readonly ILogger<LicensePlatePredictionService> _logger;
        private readonly MLContext _mlContext;
        private readonly IServiceProvider _serviceProvider;

        public LicensePlatePredictionService(
            ILicensePlateMlTrainingService trainingService,
            ILicensePlateFeatureExtractor featureExtractor,
            ILogger<LicensePlatePredictionService> logger,
            IServiceProvider serviceProvider)
        {
            _trainingService = trainingService;
            _featureExtractor = featureExtractor;
            _logger = logger;
            _mlContext = new MLContext();
            _serviceProvider = serviceProvider;
        }

        public async Task<LicensePlatePredictionResult> PredictNextSeenAsync(
            LicensePlateInput input, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var model = _trainingService.GetCurrentModel();
                if (model == null)
                {
                    _logger.LogWarning("No trained model available for prediction");
                    return CreateFallbackPrediction(input);
                }

                var features = await _featureExtractor.ExtractFeaturesForPredictionAsync(input, cancellationToken);
                var predictionEngine = _mlContext.Model.CreatePredictionEngine<LicensePlateTrainingData, LicensePlatePrediction>(model);
                
                var prediction = predictionEngine.Predict(features);
                
                // Calculate confidence based on historical data quality
                var confidenceScore = CalculateConfidenceScore(features, prediction);
                
                _logger.LogDebug("Prediction for {LicensePlate}: {PredictedHours:F2} hours with {Confidence:P2} confidence",
                    input.LicensePlate, prediction.PredictedHours, confidenceScore);

                return new LicensePlatePredictionResult
                {
                    LicensePlate = input.LicensePlate,
                    PredictedNextSeen = prediction.PredictedNextSeen,
                    PredictedHours = prediction.PredictedHours,
                    ConfidenceScore = confidenceScore,
                    TotalHistoricalVisits = (int)features.TotalVisits,
                    AverageTimeBetweenVisits = features.AverageTimeBetweenVisits,
                    LastSeen = input.LastSeen,
                    ModelVersion = GetModelVersion(),
                    PredictionMadeAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making prediction for license plate {LicensePlate}", input.LicensePlate);
                return CreateFallbackPrediction(input);
            }
        }

        public async Task<List<LicensePlatePredictionResult>> PredictBatchAsync(
            List<LicensePlateInput> inputs, 
            CancellationToken cancellationToken = default)
        {
            var results = new List<LicensePlatePredictionResult>();
            
            try
            {
                var model = _trainingService.GetCurrentModel();
                if (model == null)
                {
                    _logger.LogWarning("No trained model available for batch prediction");
                    return inputs.Select(CreateFallbackPrediction).ToList();
                }

                var predictionEngine = _mlContext.Model.CreatePredictionEngine<LicensePlateTrainingData, LicensePlatePrediction>(model);

                foreach (var input in inputs)
                {
                    try
                    {
                        var features = await _featureExtractor.ExtractFeaturesForPredictionAsync(input, cancellationToken);
                        var prediction = predictionEngine.Predict(features);
                        var confidenceScore = CalculateConfidenceScore(features, prediction);

                        results.Add(new LicensePlatePredictionResult
                        {
                            LicensePlate = input.LicensePlate,
                            PredictedNextSeen = prediction.PredictedNextSeen,
                            PredictedHours = prediction.PredictedHours,
                            ConfidenceScore = confidenceScore,
                            TotalHistoricalVisits = (int)features.TotalVisits,
                            AverageTimeBetweenVisits = features.AverageTimeBetweenVisits,
                            LastSeen = input.LastSeen,
                            ModelVersion = GetModelVersion(),
                            PredictionMadeAt = DateTime.UtcNow
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error in batch prediction for license plate {LicensePlate}", input.LicensePlate);
                        results.Add(CreateFallbackPrediction(input));
                    }
                }

                _logger.LogDebug("Completed batch prediction for {Count} license plates", inputs.Count);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in batch prediction");
                return inputs.Select(CreateFallbackPrediction).ToList();
            }
        }

        public async Task<List<LicensePlatePredictionResult>> GetTopPredictionsAsync(
            int topCount = 10,
            TimeSpan? withinHours = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var cutoffHours = withinHours?.TotalHours ?? 168; // Default to within a week
                var predictions = new List<LicensePlatePredictionResult>();

                var model = _trainingService.GetCurrentModel();
                if (model == null)
                {
                    _logger.LogWarning("No trained model available for top predictions");
                    return predictions;
                }

                // Get the most recently seen license plates (within last 30 days)
                using var scope = _serviceProvider.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                
                // Calculate the cutoff timestamp outside the query
                var thirtyDaysAgo = DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeMilliseconds();
                
                var recentPlates = await unitOfWork.PlateGroups.GetQueryable()
                    .Where(pg => !string.IsNullOrEmpty(pg.BestNumber))
                    .Where(pg => pg.ReceivedOnEpoch > thirtyDaysAgo)
                    .GroupBy(pg => pg.BestNumber)
                    .Select(g => new {
                        LicensePlate = g.Key,
                        LastSeen = g.Max(pg => pg.ReceivedOnEpoch),
                        CameraId = g.OrderByDescending(pg => pg.ReceivedOnEpoch).FirstOrDefault().OpenAlprCameraId,
                        VehicleType = g.OrderByDescending(pg => pg.ReceivedOnEpoch).FirstOrDefault().VehicleType,
                        VehicleColor = g.OrderByDescending(pg => pg.ReceivedOnEpoch).FirstOrDefault().VehicleColor,
                        TotalVisits = g.Count()
                    })
                    .Where(p => p.TotalVisits >= 3) // Only include plates with at least 3 visits (more predictable)
                    .OrderByDescending(p => p.LastSeen)
                    .Take(50) // Get top 50 recent plates to make predictions for
                    .ToListAsync(cancellationToken);

                // Make predictions for each recent plate
                foreach (var plate in recentPlates)
                {
                    try
                    {
                        var input = new LicensePlateInput
                        {
                            LicensePlate = plate.LicensePlate,
                            CameraId = plate.CameraId,
                            LastSeen = DateTimeOffset.FromUnixTimeMilliseconds(plate.LastSeen).DateTime,
                            VehicleType = plate.VehicleType ?? "",
                            VehicleColor = plate.VehicleColor ?? ""
                        };

                        var prediction = await PredictNextSeenAsync(input, cancellationToken);
                        
                        // Only include predictions within the specified time window
                        if (prediction.PredictedNextSeen <= DateTime.UtcNow.AddHours(cutoffHours))
                        {
                            predictions.Add(prediction);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error making prediction for plate {Plate}", plate.LicensePlate);
                    }
                }

                // Return top predictions sorted by soonest predicted time
                var topPredictions = predictions
                    .OrderBy(p => p.PredictedNextSeen)
                    .Take(topCount)
                    .ToList();
                
                _logger.LogDebug("Retrieved top {Count} predictions within {Hours} hours from {Total} candidates", 
                    topPredictions.Count, cutoffHours, recentPlates.Count);
                
                return topPredictions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top predictions");
                return new List<LicensePlatePredictionResult>();
            }
        }

        private double CalculateConfidenceScore(LicensePlateTrainingData features, LicensePlatePrediction prediction)
        {
            // Base confidence on historical data quality
            var dataQualityScore = Math.Min(features.TotalVisits / 10.0, 1.0); // More visits = higher confidence
            var frequencyScore = Math.Min(features.HistoricalFrequency * 10, 1.0); // Regular visits = higher confidence
            var recencyScore = features.TimeSinceLastSeen < 168 ? 1.0 : 0.5; // Recent sightings = higher confidence

            // Penalize extreme predictions
            var predictionReasonableness = 1.0;
            if (prediction.PredictedHours < 0.5 || prediction.PredictedHours > 2160) // Less than 30 min or more than 3 months
            {
                predictionReasonableness = 0.3;
            }

            var confidence = (dataQualityScore + frequencyScore + recencyScore) / 3.0 * predictionReasonableness;
            return Math.Max(0.1, Math.Min(1.0, confidence));
        }

        private LicensePlatePredictionResult CreateFallbackPrediction(LicensePlateInput input)
        {
            // Fallback prediction based on simple heuristics when ML model is unavailable
            var timeSinceLastSeen = DateTime.UtcNow - input.LastSeen;
            var fallbackHours = timeSinceLastSeen.TotalDays switch
            {
                < 1 => 24, // Daily visitor - predict tomorrow
                < 7 => 168, // Weekly visitor - predict next week
                < 30 => 720, // Monthly visitor - predict next month
                _ => 2160 // Quarterly visitor - predict in 3 months
            };

            return new LicensePlatePredictionResult
            {
                LicensePlate = input.LicensePlate,
                PredictedNextSeen = DateTime.UtcNow.AddHours(fallbackHours),
                PredictedHours = (float)fallbackHours,
                ConfidenceScore = 0.1, // Low confidence for fallback
                TotalHistoricalVisits = 1,
                AverageTimeBetweenVisits = fallbackHours,
                LastSeen = input.LastSeen,
                ModelVersion = "Fallback-v1.0",
                PredictionMadeAt = DateTime.UtcNow
            };
        }

        private string GetModelVersion()
        {
            return $"ML.NET-v1.0-{DateTime.UtcNow:yyyy-MM}";
        }

        public bool IsModelAvailable()
        {
            return _trainingService.GetCurrentModel() != null;
        }

        public async Task<bool> TriggerTrainingAsync()
        {
            _logger.LogInformation("Manual training trigger requested");
            return await _trainingService.TrainModelAsync();
        }
    }
} 