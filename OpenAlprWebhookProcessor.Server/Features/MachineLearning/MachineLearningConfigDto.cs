using System;
using System.ComponentModel.DataAnnotations;

namespace OpenAlprWebhookProcessor.Features.MachineLearning
{
    public class MachineLearningConfigDto
    {
        [Required]
        [Range(0.001, 1.0)]
        public double MinimumModelQuality { get; set; } = 0.01;

        [Required]
        [Range(10, 1000000)]
        public int MinimumTrainingData { get; set; } = 100;

        [Required]
        [Range(1000, 1000000)]
        public int TrainingBatchSize { get; set; } = 50000;

        [Required]
        public TimeSpan TrainingInterval { get; set; } = TimeSpan.FromHours(6);

        [Required]
        [MaxLength(255)]
        public string ModelFileName { get; set; } = "license-plate-prediction-model.zip";

        [Required]
        [MaxLength(255)]
        public string ConfigFolderName { get; set; } = "config";

        [Required]
        [MaxLength(255)]
        public string MlModelsFolderName { get; set; } = "ml-models";

        public DateTime? LastUpdated { get; set; }
        public string UpdatedBy { get; set; }
    }
}
