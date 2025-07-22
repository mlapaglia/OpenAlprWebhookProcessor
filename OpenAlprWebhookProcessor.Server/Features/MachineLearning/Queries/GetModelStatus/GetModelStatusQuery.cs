using MediatR;
using OpenAlprWebhookProcessor.Features.MachineLearning.Models;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Queries.GetModelStatus
{
    public class GetModelStatusQuery : IRequest<ModelStatusDto>
    {
    }
} 