using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.Features.MachineLearning.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Services
{
    public class LicensePlateMlTrainingHostedService : BackgroundService
    {
        private readonly ILicensePlateMlTrainingService _trainingService;
        private readonly ILogger<LicensePlateMlTrainingHostedService> _logger;
        private readonly IMachineLearningConfiguration _configuration;
        
        private Timer _trainingTimer;

        public LicensePlateMlTrainingHostedService(
            ILicensePlateMlTrainingService trainingService,
            ILogger<LicensePlateMlTrainingHostedService> logger,
            IMachineLearningConfiguration configuration)
        {
            _trainingService = trainingService ?? throw new ArgumentNullException(nameof(trainingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("License Plate ML Training Hosted Service starting...");

            try
            {
                await _trainingService.LoadExistingModelAsync();
                
                var trainingInterval = _configuration.TrainingInterval;
                _trainingTimer = new Timer(
                    TriggerTrainingAsync,
                    null,
                    TimeSpan.Zero,
                    trainingInterval);

                _logger.LogInformation("License Plate ML Training Hosted Service started with interval: {Interval}", trainingInterval);

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogInformation(ex, "Training service cancellation requested");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error in LicensePlateMlTrainingHostedService");
                throw;
            }

            _logger.LogDebug("License Plate ML Training Hosted Service stopped.");
        }

        private async void TriggerTrainingAsync(object state)
        {
            try
            {
                _logger.LogDebug("Triggering scheduled model training");
                await _trainingService.TrainModelAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during scheduled model training");
            }
        }

        public override void Dispose()
        {
            _trainingTimer?.Dispose();
            base.Dispose();
        }
    }
} 