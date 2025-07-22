using System;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Models
{
    public class TrainingResultDto
    {
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Success { get; set; }
    }
} 