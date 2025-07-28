using Mediator;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Commands.UpsertConfiguration
{
    public class UpsertConfigurationCommand : IQuery<Unit>
    {
        public MachineLearningConfigDto Configuration { get; set; }

        public string UpdatedBy { get; set; }
    }
}
