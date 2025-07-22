using MediatR;
using OpenAlprWebhookProcessor.Features.MachineLearning.Models;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Queries.PredictNextSeen
{
    public class PredictNextSeenQuery : IRequest<LicensePlatePredictionResult>
    {
        public LicensePlateInput Input { get; set; }

        public PredictNextSeenQuery(LicensePlateInput input)
        {
            Input = input;
        }
    }
} 