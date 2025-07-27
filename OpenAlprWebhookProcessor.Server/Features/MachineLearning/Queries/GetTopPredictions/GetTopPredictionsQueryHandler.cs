using MediatR;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.MachineLearning.Models;
using OpenAlprWebhookProcessor.Features.MachineLearning.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Queries.GetTopPredictions
{
    public class GetTopPredictionsQueryHandler : IRequestHandler<GetTopPredictionsQuery, List<LicensePlatePredictionResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILicensePlatePredictionService _predictionService;
        private readonly ILogger<GetTopPredictionsQueryHandler> _logger;

        public GetTopPredictionsQueryHandler(
            IUnitOfWork unitOfWork,
            ILicensePlatePredictionService predictionService,
            ILogger<GetTopPredictionsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _predictionService = predictionService;
            _logger = logger;
        }

        public async Task<List<LicensePlatePredictionResult>> Handle(
            GetTopPredictionsQuery request, 
            CancellationToken cancellationToken = default)
        {
            if (request.Count <= 0 || request.Count > 50)
            {
                throw new ArgumentException("Count must be between 1 and 50");
            }

            if (request.WithinHours.TotalHours <= 0 || request.WithinHours.TotalHours > 8760) // Max 1 year
            {
                throw new ArgumentException("WithinHours must be between 1 and 8760");
            }

            try
            {
                var predictions = await _predictionService.GetTopPredictionsAsync(
                    request.Count, 
                    request.WithinHours);
                
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