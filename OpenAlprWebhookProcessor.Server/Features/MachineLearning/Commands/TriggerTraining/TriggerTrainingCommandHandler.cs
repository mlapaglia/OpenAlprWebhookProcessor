using MediatR;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.MachineLearning.Models;
using OpenAlprWebhookProcessor.Features.MachineLearning.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Commands.TriggerTraining
{
    public class TriggerTrainingCommandHandler : IRequestHandler<TriggerTrainingCommand, TrainingResultDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILicensePlateMlTrainingService _trainingService;
        private readonly ILogger<TriggerTrainingCommandHandler> _logger;

        public TriggerTrainingCommandHandler(
            IUnitOfWork unitOfWork,
            ILicensePlateMlTrainingService trainingService,
            ILogger<TriggerTrainingCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _trainingService = trainingService;
            _logger = logger;
        }

        public async Task<TrainingResultDto> Handle(
            TriggerTrainingCommand request, 
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Manual model training requested by user: {RequestedBy}", request.RequestedBy);
                
                var success = await _trainingService.TrainModelAsync();
                
                if (success)
                {
                    return new TrainingResultDto
                    {
                        Message = "Model training completed successfully",
                        Timestamp = DateTime.UtcNow,
                        Success = true
                    };
                }
                else
                {
                    return new TrainingResultDto
                    {
                        Message = "Model training failed or insufficient data",
                        Timestamp = DateTime.UtcNow,
                        Success = false
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering model training");
                
                return new TrainingResultDto
                {
                    Message = "Error triggering model training",
                    Timestamp = DateTime.UtcNow,
                    Success = false
                };
            }
        }
    }
} 