using MediatR;
using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Queries.GetConfiguration
{
    public class GetConfigurationQueryHandler : IRequestHandler<GetConfigurationQuery, MachineLearningConfigDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetConfigurationQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<MachineLearningConfigDto> Handle(
            GetConfigurationQuery request,
            CancellationToken cancellationToken = default)
        {
            var repo = _unitOfWork.MachineLearningConfigurations;

            var config = new MachineLearningConfigDto
            {
                MinimumModelQuality = await repo.GetDoubleValueAsync("MinimumModelQuality", 0.01),
                MinimumTrainingData = await repo.GetIntValueAsync("MinimumTrainingData", 100),
                TrainingBatchSize = await repo.GetIntValueAsync("TrainingBatchSize", 50000),
                TrainingInterval = await repo.GetTimeSpanValueAsync("TrainingInterval", TimeSpan.FromHours(6)),
                ModelFileName = await repo.GetValueAsync("ModelFileName", "license-plate-prediction-model.zip"),
                ConfigFolderName = await repo.GetValueAsync("ConfigFolderName", "config"),
                MlModelsFolderName = await repo.GetValueAsync("MlModelsFolderName", "ml-models")
            };

            // Get the most recent update info
            var allConfigs = (await _unitOfWork.MachineLearningConfigurations.GetAllAsync()).ToList();
            var mostRecentUpdate = allConfigs
                .OrderByDescending(c => c.UpdatedAt)
                .FirstOrDefault();

            if (mostRecentUpdate != null)
            {
                config.LastUpdated = mostRecentUpdate.UpdatedAt;
                config.UpdatedBy = mostRecentUpdate.UpdatedBy;
            }

            return config;
        }
    }
}
