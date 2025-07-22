using Microsoft.Extensions.Options;
using System;
using System.IO;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Configuration
{
    public class MachineLearningConfiguration : IMachineLearningConfiguration
    {
        private readonly MachineLearningOptions _options;

        public MachineLearningConfiguration(IOptions<MachineLearningOptions> options)
        {
            _options = options.Value;
        }

        public string ModelFileName => _options.ModelFileName;
        public string ConfigFolderName => _options.ConfigFolderName;
        public string MlModelsFolderName => _options.MlModelsFolderName;
        public TimeSpan TrainingInterval => _options.TrainingInterval;
        public int TrainingBatchSize => _options.TrainingBatchSize;
        public int MinimumTrainingData => _options.MinimumTrainingData;
        public double MinimumModelQuality => _options.MinimumModelQuality;

        public string GetConfigPath()
        {
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), ConfigFolderName);
            var mlModelsPath = Path.Combine(configPath, MlModelsFolderName);

            Directory.CreateDirectory(configPath);
            Directory.CreateDirectory(mlModelsPath);

            return configPath;
        }

        public string GetModelPath()
        {
            return Path.Combine(GetConfigPath(), MlModelsFolderName, ModelFileName);
        }

        public string GetBackupPath(DateTime timestamp)
        {
            var configPath = GetConfigPath();
            var backupFileName = $"backup-{timestamp:yyyyMMdd-HHmmss}-{ModelFileName}";
            return Path.Combine(configPath, MlModelsFolderName, backupFileName);
        }
    }
}
