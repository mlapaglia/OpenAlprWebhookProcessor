using Mediator;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Features.MachineLearning.Models;
using OpenAlprWebhookProcessor.Features.MachineLearning.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Queries.PredictBatch
{
    public class PredictBatchQueryHandler : IQueryHandler<PredictBatchQuery, List<LicensePlatePredictionResult>>
    {
        private readonly ILicensePlatePredictionService _predictionService;
        private readonly ILogger<PredictBatchQueryHandler> _logger;

        public PredictBatchQueryHandler(
            ILicensePlatePredictionService predictionService,
            ILogger<PredictBatchQueryHandler> logger)
        {
            _predictionService = predictionService;
            _logger = logger;
        }

        public async ValueTask<List<LicensePlatePredictionResult>> Handle(
            PredictBatchQuery query, 
            CancellationToken cancellationToken = default)
        {
            if (query.Inputs == null || query.Inputs.Count == 0)
            {
                throw new ArgumentException("At least one license plate input is required");
            }

            if (query.Inputs.Count > 100)
            {
                throw new ArgumentException("Maximum 100 predictions per batch");
            }

            try
            {
                var predictions = await _predictionService.PredictBatchAsync(query.Inputs);
                return predictions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in batch prediction for {Count} license plates", query.Inputs.Count);
                throw;
            }
        }
    }
} 