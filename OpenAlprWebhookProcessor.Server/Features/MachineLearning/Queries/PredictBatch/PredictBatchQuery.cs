using MediatR;
using OpenAlprWebhookProcessor.Features.MachineLearning.Models;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Queries.PredictBatch
{
    public class PredictBatchQuery : IRequest<List<LicensePlatePredictionResult>>
    {
        public List<LicensePlateInput> Inputs { get; set; }

        public PredictBatchQuery(List<LicensePlateInput> inputs)
        {
            Inputs = inputs;
        }
    }
} 