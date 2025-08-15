using System;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Configuration
{
    public class MachineLearningOptions
    {
        public const string SectionName = "MachineLearning";

        public string ModelFileName { get; set; } = "license-plate-prediction-model.zip";
        public string ConfigFolderName { get; set; } = "config";
        public string MlModelsFolderName { get; set; } = "ml-models";
        public TimeSpan TrainingInterval { get; set; } = TimeSpan.FromHours(6);
        public int TrainingBatchSize { get; set; } = 50000;
        public int MinimumTrainingData { get; set; } = 100;
        public double MinimumModelQuality { get; set; } = 0.05; // R-squared threshold
    }
}
