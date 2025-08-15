using Mediator;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Features.MachineLearning.Models;
using OpenAlprWebhookProcessor.Features.MachineLearning.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Queries.GetModelInfo
{
    public class GetModelInfoQueryHandler : IQueryHandler<GetModelInfoQuery, ModelInfoDto>
    {
        private readonly ILicensePlatePredictionService _predictionService;

        private readonly ILogger<GetModelInfoQueryHandler> _logger;

        private static readonly string[] result = new[]
        {
            "HourOfDay", "DayOfWeek", "DayOfMonth", "MonthOfYear",
            "CameraId", "TimeSinceLastSeen", "HistoricalFrequency",
            "AverageTimeBetweenVisits", "TotalVisits", "IsWeekend",
            "IsBusinessHour", "SeasonalFactor", "VehicleType", "VehicleColor"
        };

        public GetModelInfoQueryHandler(
            ILicensePlatePredictionService predictionService,
            ILogger<GetModelInfoQueryHandler> logger)
        {
            _predictionService = predictionService;
            _logger = logger;
        }

        public async ValueTask<ModelInfoDto> Handle(
            GetModelInfoQuery request, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var isAvailable = _predictionService.IsModelAvailable();
                
                return await Task.FromResult(new ModelInfoDto
                {
                    ModelAvailable = isAvailable,
                    ModelType = "FastTree Regression",
                    Features = result,
                    Description = "Predicts when a license plate will next be seen based on historical patterns",
                    TrainingSchedule = "Every 6 hours",
                    LastUpdated = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting model info");
                throw;
            }
        }
    }
} 