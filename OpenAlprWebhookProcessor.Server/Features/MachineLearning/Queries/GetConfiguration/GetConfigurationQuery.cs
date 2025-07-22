using MediatR;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Queries.GetConfiguration
{
    public class GetConfigurationQuery : IRequest<MachineLearningConfigDto>
    {
    }
}
