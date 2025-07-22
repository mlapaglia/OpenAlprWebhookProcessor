using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Features.MachineLearning.Commands.UpsertConfiguration;
using OpenAlprWebhookProcessor.Features.MachineLearning.Models;
using OpenAlprWebhookProcessor.Features.MachineLearning.Queries.GetConfiguration;
using OpenAlprWebhookProcessor.Features.MachineLearning.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.MachineLearning
{
    /// <summary>
    /// API controller for machine learning predictions on license plate patterns.
    /// Provides endpoints for single predictions, batch predictions, and model management.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MachineLearningController : ControllerBase
    {
        private readonly ILicensePlatePredictionService _predictionService;
        private readonly ILicensePlateMlTrainingService _trainingService;
        private readonly ILogger<MachineLearningController> _logger;
        private readonly IMediator _mediator;

        public MachineLearningController(
            IMediator mediator,
            ILicensePlatePredictionService predictionService,
            ILicensePlateMlTrainingService trainingService,
            ILogger<MachineLearningController> logger)
        {
            _predictionService = predictionService;
            _trainingService = trainingService;
            _logger = logger;
            _mediator = mediator;
        }

        [HttpPost("predict")]
        public async Task<ActionResult<LicensePlatePredictionResult>> PredictNextSeen([FromBody] LicensePlateInput input)
        {
            if (string.IsNullOrEmpty(input?.LicensePlate))
            {
                return BadRequest("License plate is required");
            }

            try
            {
                var prediction = await _predictionService.PredictNextSeenAsync(input);
                return Ok(prediction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error predicting for license plate {LicensePlate}", input.LicensePlate);
                return StatusCode(500, "Error generating prediction");
            }
        }

        [HttpPost("predict/batch")]
        public async Task<ActionResult<List<LicensePlatePredictionResult>>> PredictBatch([FromBody] List<LicensePlateInput> inputs)
        {
            if (inputs == null || inputs.Count == 0)
            {
                return BadRequest("At least one license plate input is required");
            }

            if (inputs.Count > 100)
            {
                return BadRequest("Maximum 100 predictions per batch");
            }

            try
            {
                var predictions = await _predictionService.PredictBatchAsync(inputs);
                return Ok(predictions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in batch prediction for {Count} license plates", inputs.Count);
                return StatusCode(500, "Error generating batch predictions");
            }
        }

        [HttpGet("predict/top")]
        public async Task<ActionResult<List<LicensePlatePredictionResult>>> GetTopPredictions(
            [FromQuery] int count = 10, 
            [FromQuery] int withinHours = 168)
        {
            if (count <= 0 || count > 50)
            {
                return BadRequest("Count must be between 1 and 50");
            }

            if (withinHours <= 0 || withinHours > 8760) // Max 1 year
            {
                return BadRequest("WithinHours must be between 1 and 8760");
            }

            try
            {
                var predictions = await _predictionService.GetTopPredictionsAsync(
                    count, 
                    TimeSpan.FromHours(withinHours));
                
                return Ok(predictions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top predictions");
                return StatusCode(500, "Error retrieving top predictions");
            }
        }

        [HttpGet("model/status")]
        public IActionResult GetModelStatus()
        {
            try
            {
                var isAvailable = _predictionService.IsModelAvailable();
                return Ok(new
                {
                    ModelAvailable = isAvailable,
                    Status = isAvailable ? "Ready" : "Training or Not Available",
                    LastChecked = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking model status");
                return StatusCode(500, "Error checking model status");
            }
        }

        [HttpPost("model/retrain")]
        public async Task<IActionResult> TriggerTraining()
        {
            try
            {
                _logger.LogInformation("Manual model training requested by user");
                var success = await _trainingService.TrainModelAsync();
                
                if (success)
                {
                    return Ok(new { Message = "Model training completed successfully", Timestamp = DateTime.UtcNow });
                }
                else
                {
                    return BadRequest(new { Message = "Model training failed or insufficient data", Timestamp = DateTime.UtcNow });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering model training");
                return StatusCode(500, "Error triggering model training");
            }
        }

        [HttpGet("model/info")]
        public IActionResult GetModelInfo()
        {
            try
            {
                var isAvailable = _predictionService.IsModelAvailable();
                
                return Ok(new
                {
                    ModelAvailable = isAvailable,
                    ModelType = "FastTree Regression",
                    Features = new[]
                    {
                        "HourOfDay", "DayOfWeek", "DayOfMonth", "MonthOfYear",
                        "CameraId", "TimeSinceLastSeen", "HistoricalFrequency",
                        "AverageTimeBetweenVisits", "TotalVisits", "IsWeekend",
                        "IsBusinessHour", "SeasonalFactor", "VehicleType", "VehicleColor"
                    },
                    Description = "Predicts when a license plate will next be seen based on historical patterns",
                    TrainingSchedule = "Every 6 hours",
                    LastUpdated = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting model info");
                return StatusCode(500, "Error retrieving model information");
            }
        }

        [HttpGet("training/status")]
        public async Task<IActionResult> GetTrainingStatusAsync(CancellationToken cancellationToken)
        {
            try
            {
                var status = _trainingService.GetTrainingStatus();
                
                return Ok(new
                {
                    status.IsTraining,
                    status.LastTrainingStarted,
                    status.LastTrainingCompleted,
                    status.LastTrainingSuccessful,
                    status.LastError,
                    status.TrainingDataCount,
                    ModelMetrics = status.RSquared.HasValue ? new
                    {
                        RSquared = status.RSquared.Value,
                        MeanAbsoluteError = status.MeanAbsoluteError.Value,
                        RootMeanSquaredError = status.RootMeanSquaredError.Value
                    } : null,
                    ModelFile = status.ModelLastSaved.HasValue ? new
                    {
                        LastSaved = status.ModelLastSaved.Value,
                        FileSizeBytes = status.ModelFileSize.Value
                    } : null,
                    Configuration = new
                    {
                        TrainingInterval = "Every 6 hours",
                        MinimumTrainingData = 100,
                        MinimumModelQuality = 0.05,
                        BatchSize = 50000
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting training status");
                return StatusCode(500, "Error retrieving training status");
            }
        }

        [HttpGet("configuration")]
        public async Task<ActionResult<MachineLearningConfigDto>> GetConfiguration()
        {
            var result = await _mediator.Send(new GetConfigurationQuery());
            return Ok(result);
        }

        [HttpPut("configuration")]
        public async Task<ActionResult<Unit>> UpsertConfiguration(
            [FromBody] MachineLearningConfigDto request)
        {
            var command = new UpsertConfigurationCommand
            {
                Configuration = request,
                UpdatedBy = User.Identity?.Name ?? "System"
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
} 