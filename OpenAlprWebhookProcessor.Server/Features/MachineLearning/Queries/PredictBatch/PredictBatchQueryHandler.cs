using MediatR;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.MachineLearning.Models;
using OpenAlprWebhookProcessor.Features.MachineLearning.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Queries.PredictBatch
{
    public class PredictBatchQueryHandler : IRequestHandler<PredictBatchQuery, List<LicensePlatePredictionResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILicensePlatePredictionService _predictionService;
        private readonly ILogger<PredictBatchQueryHandler> _logger;

        public PredictBatchQueryHandler(
            IUnitOfWork unitOfWork,
            ILicensePlatePredictionService predictionService,
            ILogger<PredictBatchQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _predictionService = predictionService;
            _logger = logger;
        }

        public async Task<List<LicensePlatePredictionResult>> Handle(
            PredictBatchQuery request, 
            CancellationToken cancellationToken)
        {
            if (request.Inputs == null || request.Inputs.Count == 0)
            {
                throw new ArgumentException("At least one license plate input is required");
            }

            if (request.Inputs.Count > 100)
            {
                throw new ArgumentException("Maximum 100 predictions per batch");
            }

            try
            {
                var predictions = await _predictionService.PredictBatchAsync(request.Inputs);
                return predictions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in batch prediction for {Count} license plates", request.Inputs.Count);
                throw;
            }
        }
    }
} 