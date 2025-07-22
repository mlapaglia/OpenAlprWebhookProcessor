using System;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Configuration
{
    public interface IMachineLearningConfiguration
    {
        string ModelFileName { get; }
        string ConfigFolderName { get; }
        string MlModelsFolderName { get; }
        TimeSpan TrainingInterval { get; }
        int TrainingBatchSize { get; }
        int MinimumTrainingData { get; }
        double MinimumModelQuality { get; }

        string GetConfigPath();
        string GetModelPath();
        string GetBackupPath(DateTime timestamp);
    }
}
