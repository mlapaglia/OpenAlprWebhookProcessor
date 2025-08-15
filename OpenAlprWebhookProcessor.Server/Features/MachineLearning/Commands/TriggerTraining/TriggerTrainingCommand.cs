using Mediator;
using OpenAlprWebhookProcessor.Features.MachineLearning.Models;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Commands.TriggerTraining
{
    public class TriggerTrainingCommand : IQuery<TrainingResultDto>
    {
        public string RequestedBy { get; set; }

        public TriggerTrainingCommand(string requestedBy)
        {
            RequestedBy = requestedBy;
        }
    }
} 