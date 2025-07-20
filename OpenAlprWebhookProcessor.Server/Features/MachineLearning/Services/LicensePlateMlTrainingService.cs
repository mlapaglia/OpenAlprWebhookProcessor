using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;
using OpenAlprWebhookProcessor.Features.MachineLearning.Models;
using OpenAlprWebhookProcessor.Features.MachineLearning.Configuration;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Services
{
    /// <summary>
    /// Background service that continuously trains and updates ML models for license plate prediction.
    /// Handles model versioning, training scheduling, and model persistence.
    /// </summary>
    public class LicensePlateMlTrainingService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger<LicensePlateMlTrainingService> _logger
            ;
        private readonly MLContext _mlContext;

        private Timer _trainingTimer;

        private readonly ConcurrentDictionary<string, ITransformer> _modelCache;

        private readonly string _configPath;

        public LicensePlateMlTrainingService(
            IServiceProvider serviceProvider,
            ILogger<LicensePlateMlTrainingService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _mlContext = new MLContext(seed: 42);
            _modelCache = new ConcurrentDictionary<string, ITransformer>();
            _configPath = MachineLearningConfiguration.GetConfigPath();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            _trainingTimer = new Timer(TriggerTraining, null, TimeSpan.Zero, MachineLearningConfiguration.DefaultTrainingInterval);

            _logger.LogInformation("License Plate ML Training Service started");
            
            await LoadExistingModelAsync(stoppingToken);
            
            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogInformation(ex, "Training service cancellation requested");
            }
        }

        private async void TriggerTraining(object state)
        {
            try
            {
                await TrainModelAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during scheduled model training");
            }
        }

        public async Task<bool> TrainModelAsync()
        {
            _logger.LogInformation("Starting ML model training");
            
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var featureExtractor = scope.ServiceProvider.GetRequiredService<ILicensePlateFeatureExtractor>();
                
                _logger.LogInformation("Extracting training data from database...");
                var trainingData = await featureExtractor.ExtractTrainingDataAsync(MachineLearningConfiguration.DefaultTrainingBatchSize);
                
                if (trainingData.Count < MachineLearningConfiguration.MinimumTrainingData)
                {
                    _logger.LogWarning("Insufficient training data ({Count} samples). Skipping training.", trainingData.Count);
                    return false;
                }

                _logger.LogInformation("Training with {Count} samples", trainingData.Count);
                
                var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);
                
                var trainTestSplit = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
                var trainData = trainTestSplit.TrainSet;
                var testData = trainTestSplit.TestSet;

                var pipeline = _mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CameraIdEncoded", inputColumnName: "CameraId")
                    // Add feature scaling to handle different ranges (TotalVisits: 1-2895, vs HourOfDay: 0-23)
                    .Append(_mlContext.Transforms.NormalizeMinMax("TotalVisits"))
                    .Append(_mlContext.Transforms.NormalizeMinMax("TimeSinceLastSeen"))  
                    .Append(_mlContext.Transforms.NormalizeMinMax("HistoricalFrequency"))
                    .Append(_mlContext.Transforms.NormalizeMinMax("AverageTimeBetweenVisits"))
                    .Append(_mlContext.Transforms.Concatenate("Features", 
                        "HourOfDay", "DayOfWeek", "DayOfMonth", "MonthOfYear",
                        "CameraIdEncoded", "TimeSinceLastSeen", "HistoricalFrequency", 
                        "AverageTimeBetweenVisits", "TotalVisits", "IsWeekend", "IsBusinessHour",
                        "SeasonalFactor", "VehicleTypeCode", "VehicleColorCode"))
                    .Append(_mlContext.Regression.Trainers.FastForest(labelColumnName: "Label", featureColumnName: "Features")); // FastTree regression for non-linear patterns

                _logger.LogInformation("Training model pipeline...");
                var model = pipeline.Fit(trainData);

                _logger.LogInformation("Evaluating model performance...");
                var predictions = model.Transform(testData);
                var metrics = _mlContext.Regression.Evaluate(predictions);

                _logger.LogInformation("Model training completed. Metrics:");
                _logger.LogInformation("R-Squared: {RSquared:F4}", metrics.RSquared);
                _logger.LogInformation("Mean Absolute Error: {MAE:F2} hours", metrics.MeanAbsoluteError);
                _logger.LogInformation("Root Mean Squared Error: {RMSE:F2} hours", metrics.RootMeanSquaredError);

                if (metrics.RSquared > MachineLearningConfiguration.MinimumModelQuality)
                {
                    await SaveModelAsync(model);
                    _modelCache.AddOrUpdate("current", model, (key, oldValue) => model);
                    
                    _logger.LogInformation("Model training completed successfully and saved");
                    return true;
                }
                else
                {
                    _logger.LogWarning("Model quality too low (R²: {RSquared:F4}). Not saving model.", metrics.RSquared);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during model training");
                return false;
            }
        }

        private Task SaveModelAsync(ITransformer model)
        {
            var modelPath = MachineLearningConfiguration.GetModelPath();
            var backupPath = MachineLearningConfiguration.GetBackupPath(DateTime.UtcNow);
            
            try
            {
                // Backup existing model if it exists
                if (File.Exists(modelPath))
                {
                    File.Copy(modelPath, backupPath);
                    _logger.LogInformation("Backed up existing model to {BackupPath}", backupPath);
                }

                // Save new model
                using var fileStream = new FileStream(modelPath, FileMode.Create, FileAccess.Write, FileShare.Read);
                _mlContext.Model.Save(model, null, fileStream);
                
                _logger.LogInformation("Model saved to {ModelPath}", modelPath);
                
                // Clean up old backups (keep last 5)
                CleanupOldBackupsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving model to {ModelPath}", modelPath);
                throw;
            }

            return Task.CompletedTask;
        }

        private Task LoadExistingModelAsync(CancellationToken cancellationToken)
        {
            var modelPath = MachineLearningConfiguration.GetModelPath();
            
            if (!File.Exists(modelPath))
            {
                _logger.LogInformation("No existing model found at {ModelPath}. Will train new model.", modelPath);
                return Task.CompletedTask;
            }

            try
            {
                using var fileStream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var model = _mlContext.Model.Load(fileStream, out var modelInputSchema);
                
                _modelCache.AddOrUpdate("current", model, (key, oldValue) => model);
                _logger.LogInformation("Loaded existing model from {ModelPath}", modelPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading existing model from {ModelPath}. Will train new model.", modelPath);
            }

            return Task.CompletedTask;
        }

        private Task CleanupOldBackupsAsync()
        {
            try
            {
                var mlModelsPath = Path.Combine(_configPath, MachineLearningConfiguration.MlModelsFolderName);
                var backupFiles = Directory.GetFiles(mlModelsPath, "backup-*" + MachineLearningConfiguration.ModelFileName)
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .Skip(5) // Keep 5 most recent backups
                    .ToList();

                foreach (var file in backupFiles)
                {
                    try
                    {
                        file.Delete();
                        _logger.LogDebug("Deleted old backup: {FileName}", file.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not delete backup file {FileName}", file.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during backup cleanup");
            }

            return Task.CompletedTask;
        }

        public ITransformer GetCurrentModel()
        {
            return _modelCache.TryGetValue("current", out var model) ? model : null;
        }

        public override void Dispose()
        {
            _trainingTimer?.Dispose();
            base.Dispose();
        }
    }
} 