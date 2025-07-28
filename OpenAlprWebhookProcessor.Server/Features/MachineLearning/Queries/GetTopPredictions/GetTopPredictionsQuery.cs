using Mediator;
using OpenAlprWebhookProcessor.Features.MachineLearning.Models;
using System;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Queries.GetTopPredictions
{
    public class GetTopPredictionsQuery : IQuery<List<LicensePlatePredictionResult>>
    {
        public int Count { get; set; }
        public TimeSpan WithinHours { get; set; }

        public GetTopPredictionsQuery(int count, TimeSpan withinHours)
        {
            Count = count;
            WithinHours = withinHours;
        }
    }
} 