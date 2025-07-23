namespace OpenAlprWebhookProcessor.Features.MachineLearning.Models
{
    public class ModelInfoDto
    {
        public bool ModelAvailable { get; set; }
        public string ModelType { get; set; }
        public string[] Features { get; set; }
        public string Description { get; set; }
        public string TrainingSchedule { get; set; }
        public string LastUpdated { get; set; }
    }
} 