using Mediator;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Features.MachineLearning.Models;
using OpenAlprWebhookProcessor.Features.MachineLearning.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Queries.GetTrainingStatus
{
    public class GetTrainingStatusQueryHandler : IQueryHandler<GetTrainingStatusQuery, TrainingStatusDto>
    {
        private readonly ILicensePlateMlTrainingService _trainingService;
        private readonly ILogger<GetTrainingStatusQueryHandler> _logger;

        public GetTrainingStatusQueryHandler(
            ILicensePlateMlTrainingService trainingService,
            ILogger<GetTrainingStatusQueryHandler> logger)
        {
            _trainingService = trainingService;
            _logger = logger;
        }

        public async ValueTask<TrainingStatusDto> Handle(
            GetTrainingStatusQuery request, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var status = _trainingService.GetTrainingStatus();
                
                return await Task.FromResult(new TrainingStatusDto
                {
                    IsTraining = status.IsTraining,
                    LastTrainingStarted = status.LastTrainingStarted,
                    LastTrainingCompleted = status.LastTrainingCompleted,
                    LastTrainingSuccessful = status.LastTrainingSuccessful,
                    LastError = status.LastError,
                    TrainingDataCount = status.TrainingDataCount,
                    ModelMetrics = status.RSquared.HasValue ? new ModelMetricsDto
                    {
                        RSquared = status.RSquared.Value,
                        MeanAbsoluteError = status.MeanAbsoluteError.Value,
                        RootMeanSquaredError = status.RootMeanSquaredError.Value
                    } : null,
                    ModelFile = status.ModelLastSaved.HasValue ? new ModelFileDto
                    {
                        LastSaved = status.ModelLastSaved.Value,
                        FileSizeBytes = status.ModelFileSize.Value
                    } : null,
                    Configuration = new TrainingConfigurationDto
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
                throw;
            }
        }
    }
} 