using MediatR;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Commands.UpsertConfiguration
{
    public class UpsertConfigurationCommand : IRequest<Unit>
    {
        public MachineLearningConfigDto Configuration { get; set; }

        public string UpdatedBy { get; set; }
    }
}
