using System;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Models
{
    public class TrainingStatusDto
    {
        public bool IsTraining { get; set; }
        public DateTime? LastTrainingStarted { get; set; }
        public DateTime? LastTrainingCompleted { get; set; }
        public bool LastTrainingSuccessful { get; set; }
        public string LastError { get; set; }
        public int TrainingDataCount { get; set; }
        public ModelMetricsDto ModelMetrics { get; set; }
        public ModelFileDto ModelFile { get; set; }
        public TrainingConfigurationDto Configuration { get; set; }
    }

    public class ModelMetricsDto
    {
        public double RSquared { get; set; }
        public double MeanAbsoluteError { get; set; }
        public double RootMeanSquaredError { get; set; }
    }

    public class ModelFileDto
    {
        public DateTime LastSaved { get; set; }
        public long FileSizeBytes { get; set; }
    }

    public class TrainingConfigurationDto
    {
        public string TrainingInterval { get; set; }
        public int MinimumTrainingData { get; set; }
        public double MinimumModelQuality { get; set; }
        public int BatchSize { get; set; }
    }
} 