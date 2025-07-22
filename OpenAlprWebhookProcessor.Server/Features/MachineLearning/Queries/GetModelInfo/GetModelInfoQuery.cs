using MediatR;
using OpenAlprWebhookProcessor.Features.MachineLearning.Models;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Queries.GetModelInfo
{
    public class GetModelInfoQuery : IRequest<ModelInfoDto>
    {
    }
} 