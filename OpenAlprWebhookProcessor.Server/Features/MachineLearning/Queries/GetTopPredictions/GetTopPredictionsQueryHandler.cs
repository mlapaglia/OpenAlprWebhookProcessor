using Mediator;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Features.MachineLearning.Models;
using OpenAlprWebhookProcessor.Features.MachineLearning.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Queries.GetTopPredictions
{
    public class GetTopPredictionsQueryHandler : IQueryHandler<GetTopPredictionsQuery, List<LicensePlatePredictionResult>>
    {
        private readonly ILicensePlatePredictionService _predictionService;
        private readonly ILogger<GetTopPredictionsQueryHandler> _logger;

        public GetTopPredictionsQueryHandler(
            ILicensePlatePredictionService predictionService,
            ILogger<GetTopPredictionsQueryHandler> logger)
        {
            _predictionService = predictionService;
            _logger = logger;
        }

        public async ValueTask<List<LicensePlatePredictionResult>> Handle(
            GetTopPredictionsQuery query, 
            CancellationToken cancellationToken = default)
        {
            if (query.Count <= 0 || query.Count > 50)
            {
                throw new ArgumentException("Count must be between 1 and 50");
            }

            if (query.WithinHours.TotalHours <= 0 || query.WithinHours.TotalHours > 8760) // Max 1 year
            {
                throw new ArgumentException("WithinHours must be between 1 and 8760");
            }

            try
            {
                var predictions = await _predictionService.GetTopPredictionsAsync(
                    query.Count, 
                    query.WithinHours,
                    cancellationToken);
                
                return predictions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top predictions");
                throw;
            }
        }
    }
} 