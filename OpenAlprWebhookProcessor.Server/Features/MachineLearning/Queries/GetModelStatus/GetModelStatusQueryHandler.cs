using MediatR;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.MachineLearning.Models;
using OpenAlprWebhookProcessor.Features.MachineLearning.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Queries.GetModelStatus
{
    public class GetModelStatusQueryHandler : IRequestHandler<GetModelStatusQuery, ModelStatusDto>
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

        public Task<ModelStatusDto> Handle(
            GetModelStatusQuery request, 
            CancellationToken cancellationToken)
        {
            try
            {
                var isAvailable = _predictionService.IsModelAvailable();
                
                return Task.FromResult(new ModelStatusDto
                {
                    ModelAvailable = isAvailable,
                    Status = isAvailable ? "Ready" : "Training or Not Available",
                    LastChecked = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking model status");
                throw;
            }
        }
    }
} 