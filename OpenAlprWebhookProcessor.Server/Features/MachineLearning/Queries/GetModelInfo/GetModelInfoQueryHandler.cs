using MediatR;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.MachineLearning.Models;
using OpenAlprWebhookProcessor.Features.MachineLearning.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Queries.GetModelInfo
{
    public class GetModelInfoQueryHandler : IRequestHandler<GetModelInfoQuery, ModelInfoDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILicensePlatePredictionService _predictionService;
        private readonly ILogger<GetModelInfoQueryHandler> _logger;

        public GetModelInfoQueryHandler(
            IUnitOfWork unitOfWork,
            ILicensePlatePredictionService predictionService,
            ILogger<GetModelInfoQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _predictionService = predictionService;
            _logger = logger;
        }

        public async Task<ModelInfoDto> Handle(
            GetModelInfoQuery request, 
            CancellationToken cancellationToken)
        {
            try
            {
                var isAvailable = _predictionService.IsModelAvailable();
                
                return new ModelInfoDto
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
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting model info");
                throw;
            }
        }
    }
} 