using System;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Services
{
    public class TrainingStatus
    {
        public bool IsTraining { get; set; }

        public DateTime? LastTrainingStarted { get; set; }

        public DateTime? LastTrainingCompleted { get; set; }

        public bool LastTrainingSuccessful { get; set; }

        public string LastError { get; set; }

        public int TrainingDataCount { get; set; }

        public double? RSquared { get; set; }

        public double? MeanAbsoluteError { get; set; }

        public double? RootMeanSquaredError { get; set; }

        public DateTime? ModelLastSaved { get; set; }

        public long? ModelFileSize { get; set; }
    }
}
