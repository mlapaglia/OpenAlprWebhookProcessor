using Mediator;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.MachineLearning.Models;
using OpenAlprWebhookProcessor.Features.MachineLearning.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Queries.GetModelStatus
{
    public class GetModelStatusQueryHandler : IQueryHandler<GetModelStatusQuery, ModelStatusDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILicensePlatePredictionService _predictionService;
        private readonly ILogger<GetModelStatusQueryHandler> _logger;

        public GetModelStatusQueryHandler(
            IUnitOfWork unitOfWork,
            ILicensePlatePredictionService predictionService,
            ILogger<GetModelStatusQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _predictionService = predictionService;
            _logger = logger;
        }

        public async ValueTask<ModelStatusDto> Handle(
            GetModelStatusQuery request, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var isAvailable = _predictionService.IsModelAvailable();
                
                return new ModelStatusDto
                {
                    ModelAvailable = isAvailable,
                    Status = isAvailable ? "Ready" : "Training or Not Available",
                    LastChecked = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking model status");
                throw;
            }
        }
    }
} 