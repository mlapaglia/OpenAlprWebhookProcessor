using MediatR;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Commands.UpsertConfiguration
{
    public class UpsertConfigurationCommandHandler : IRequestHandler<UpsertConfigurationCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpsertConfigurationCommandHandler(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(UpsertConfigurationCommand request, CancellationToken cancellationToken)
        {
            var updateTime = DateTime.UtcNow;

            await UpsertConfigValue("MinimumModelQuality", request.Configuration.MinimumModelQuality.ToString(), "double", "Minimum R-squared value required to save a trained model", request.UpdatedBy, updateTime);
            await UpsertConfigValue("MinimumTrainingData", request.Configuration.MinimumTrainingData.ToString(), "int", "Minimum number of training samples required", request.UpdatedBy, updateTime);
            await UpsertConfigValue("TrainingBatchSize", request.Configuration.TrainingBatchSize.ToString(), "int", "Maximum number of records to process in one training batch", request.UpdatedBy, updateTime);
            await UpsertConfigValue("TrainingInterval", request.Configuration.TrainingInterval.ToString(), "timespan", "Interval between automatic model training sessions", request.UpdatedBy, updateTime);
            await UpsertConfigValue("ModelFileName", request.Configuration.ModelFileName, "string", "Filename for the saved ML model", request.UpdatedBy, updateTime);
            await UpsertConfigValue("ConfigFolderName", request.Configuration.ConfigFolderName, "string", "Folder name for configuration files", request.UpdatedBy, updateTime);
            await UpsertConfigValue("MlModelsFolderName", request.Configuration.MlModelsFolderName, "string", "Subfolder name for ML model files", request.UpdatedBy, updateTime);

            await _unitOfWork.SaveChangesAsync();

            // Return the updated configuration
            var updatedConfig = request.Configuration;
            updatedConfig.LastUpdated = updateTime;
            updatedConfig.UpdatedBy = request.UpdatedBy;

            return Unit.Value;
        }

        private async Task UpsertConfigValue(string key, string value, string valueType, string description, string updatedBy, DateTime updateTime)
        {
            var existing = await _unitOfWork.MachineLearningConfigurations.GetByKeyAsync(key);

            if (existing != null)
            {
                existing.Value = value;
                existing.UpdatedAt = updateTime;
                existing.UpdatedBy = updatedBy;
                _unitOfWork.MachineLearningConfigurations.Update(existing);
            }
            else
            {
                var newConfig = new MachineLearningConfiguration
                {
                    Key = key,
                    Value = value,
                    ValueType = valueType,
                    Description = description,
                    CreatedAt = updateTime,
                    UpdatedAt = updateTime,
                    UpdatedBy = updatedBy
                };
                await _unitOfWork.MachineLearningConfigurations.AddAsync(newConfig);
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
