using Mediator;
using OpenAlprWebhookProcessor.Features.MachineLearning.Models;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Queries.PredictNextSeen
{
    public class PredictNextSeenQuery : IQuery<LicensePlatePredictionResult>
    {
        public LicensePlateInput Input { get; set; }

        public PredictNextSeenQuery(LicensePlateInput input)
        {
            Input = input;
        }
    }
} 