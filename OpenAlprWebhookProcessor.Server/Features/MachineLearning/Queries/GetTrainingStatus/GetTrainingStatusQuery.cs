using MediatR;
using OpenAlprWebhookProcessor.Features.MachineLearning.Models;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Queries.GetTrainingStatus
{
    public class GetTrainingStatusQuery : IRequest<TrainingStatusDto>
    {
    }
} 