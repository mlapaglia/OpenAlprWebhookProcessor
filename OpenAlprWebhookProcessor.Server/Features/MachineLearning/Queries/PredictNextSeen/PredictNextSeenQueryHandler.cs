using Mediator;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Features.MachineLearning.Models;
using OpenAlprWebhookProcessor.Features.MachineLearning.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Queries.PredictNextSeen
{
    public class PredictNextSeenQueryHandler : IQueryHandler<PredictNextSeenQuery, LicensePlatePredictionResult>
    {
        private readonly ILicensePlatePredictionService _predictionService;
        private readonly ILogger<PredictNextSeenQueryHandler> _logger;

        public PredictNextSeenQueryHandler(
            ILicensePlatePredictionService predictionService,
            ILogger<PredictNextSeenQueryHandler> logger)
        {
            _predictionService = predictionService;
            _logger = logger;
        }

        public async ValueTask<LicensePlatePredictionResult> Handle(
            PredictNextSeenQuery request, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(request.Input?.LicensePlate))
            {
                throw new ArgumentException("License plate is required");
            }

            try
            {
                var prediction = await _predictionService.PredictNextSeenAsync(request.Input);
                return prediction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error predicting for license plate {LicensePlate}", request.Input.LicensePlate);
                throw;
            }
        }
    }
} 