using System;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Models
{
    public class ModelStatusDto
    {
        public bool ModelAvailable { get; set; }
        public string Status { get; set; }
        public DateTime LastChecked { get; set; }
    }
} 