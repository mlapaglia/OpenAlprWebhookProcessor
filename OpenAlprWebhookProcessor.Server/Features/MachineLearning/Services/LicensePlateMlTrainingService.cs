using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using OpenAlprWebhookProcessor.Features.MachineLearning.Configuration;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Services
{
    public class TrainingStatus
    {
        public bool IsTraining { get; set; }
        public DateTime? LastTrainingStarted { get; set; }
        public DateTime? LastTrainingCompleted { get; set; }
        public bool LastTrainingSuccessful { get; set; }
        public string LastError { get; set; }
        public int TrainingDataCount { get; set; }
        public double? RSquared { get; set; }
        public double? MeanAbsoluteError { get; set; }
        public double? RootMeanSquaredError { get; set; }
        public DateTime? ModelLastSaved { get; set; }
        public long? ModelFileSize { get; set; }
    }

    /// <summary>
    /// Background service that continuously trains and updates ML models for license plate prediction.
    /// Handles model versioning, training scheduling, and model persistence.
    /// </summary>
    public class LicensePlateMlTrainingService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger<LicensePlateMlTrainingService> _logger;

        private readonly MLContext _mlContext;

        private Timer _trainingTimer;

        private readonly ConcurrentDictionary<string, ITransformer> _modelCache;

        private readonly TrainingStatus _trainingStatus;

        public LicensePlateMlTrainingService(
            IServiceProvider serviceProvider,
            ILogger<LicensePlateMlTrainingService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _mlContext = new MLContext(seed: 42);
            _modelCache = new ConcurrentDictionary<string, ITransformer>();
            _trainingStatus = new TrainingStatus();
        }

        public TrainingStatus GetTrainingStatus()
        {
            var modelPath = MachineLearningConfiguration.GetModelPath();
            if (File.Exists(modelPath))
            {
                var fileInfo = new FileInfo(modelPath);
                _trainingStatus.ModelLastSaved = fileInfo.LastWriteTime;
                _trainingStatus.ModelFileSize = fileInfo.Length;
            }
            return _trainingStatus;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _trainingTimer = new Timer(
                TriggerTrainingAsync,
                null,
                TimeSpan.Zero,
                MachineLearningConfiguration.DefaultTrainingInterval);

            _logger.LogInformation("License Plate ML Training Service started");
            
            LoadExistingModel();
            
            try
            {
                await Task.Delay(
                    Timeout.Infinite,
                    stoppingToken);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogInformation(ex, "Training service cancellation requested");
            }
        }

        private async void TriggerTrainingAsync(object state)
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
            
            _trainingStatus.IsTraining = true;
            _trainingStatus.LastTrainingStarted = DateTime.UtcNow;
            _trainingStatus.LastError = null;
            
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var featureExtractor = scope.ServiceProvider.GetRequiredService<ILicensePlateFeatureExtractor>();
                
                _logger.LogInformation("Extracting training data from database...");
                var trainingData = await featureExtractor.ExtractTrainingDataAsync(MachineLearningConfiguration.DefaultTrainingBatchSize);
                
                _trainingStatus.TrainingDataCount = trainingData.Count;
                
                if (trainingData.Count < MachineLearningConfiguration.MinimumTrainingData)
                {
                    _logger.LogWarning("Insufficient training data ({Count} samples). Skipping training.", trainingData.Count);
                    _trainingStatus.IsTraining = false;
                    _trainingStatus.LastTrainingCompleted = DateTime.UtcNow;
                    _trainingStatus.LastTrainingSuccessful = false;
                    _trainingStatus.LastError = $"Insufficient training data ({trainingData.Count} samples, minimum required: {MachineLearningConfiguration.MinimumTrainingData})";
                    return false;
                }

                _logger.LogInformation("Training with {Count} samples", trainingData.Count);
                
                var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);
                
                var trainTestSplit = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
                var trainData = trainTestSplit.TrainSet;
                var testData = trainTestSplit.TestSet;

                var pipeline = _mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CameraIdEncoded", inputColumnName: "CameraId")
                    .Append(_mlContext.Transforms.NormalizeMinMax("TotalVisits"))
                    .Append(_mlContext.Transforms.NormalizeMinMax("TimeSinceLastSeen"))  
                    .Append(_mlContext.Transforms.NormalizeMinMax("HistoricalFrequency"))
                    .Append(_mlContext.Transforms.NormalizeMinMax("AverageTimeBetweenVisits"))
                    .Append(_mlContext.Transforms.Concatenate("Features", 
                        "HourOfDay", "DayOfWeek", "DayOfMonth", "MonthOfYear",
                        "CameraIdEncoded", "TimeSinceLastSeen", "HistoricalFrequency", 
                        "AverageTimeBetweenVisits", "TotalVisits", "IsWeekend", "IsBusinessHour",
                        "SeasonalFactor", "VehicleTypeCode", "VehicleColorCode"))
                    .Append(_mlContext.Regression.Trainers.FastForest(labelColumnName: "Label", featureColumnName: "Features"));

                _logger.LogInformation("Training model pipeline...");
                var model = pipeline.Fit(trainData);

                _logger.LogInformation("Evaluating model performance...");
                var predictions = model.Transform(testData);
                var metrics = _mlContext.Regression.Evaluate(predictions);

                _logger.LogInformation("Model training completed. Metrics:");
                _logger.LogInformation("R-Squared: {RSquared:F4}", metrics.RSquared);
                _logger.LogInformation("Mean Absolute Error: {MAE:F2} hours", metrics.MeanAbsoluteError);
                _logger.LogInformation("Root Mean Squared Error: {RMSE:F2} hours", metrics.RootMeanSquaredError);

                _trainingStatus.RSquared = metrics.RSquared;
                _trainingStatus.MeanAbsoluteError = metrics.MeanAbsoluteError;
                _trainingStatus.RootMeanSquaredError = metrics.RootMeanSquaredError;

                if (metrics.RSquared > MachineLearningConfiguration.MinimumModelQuality)
                {
                    await SaveModelAsync(model);
                    _modelCache.AddOrUpdate("current", model, (key, oldValue) => model);
                    
                    _trainingStatus.IsTraining = false;
                    _trainingStatus.LastTrainingCompleted = DateTime.UtcNow;
                    _trainingStatus.LastTrainingSuccessful = true;
                    
                    _logger.LogInformation("Model training completed successfully and saved");
                    return true;
                }
                else
                {
                    _logger.LogWarning("Model quality too low (R²: {RSquared:F4}). Not saving model.", metrics.RSquared);
                    _trainingStatus.IsTraining = false;
                    _trainingStatus.LastTrainingCompleted = DateTime.UtcNow;
                    _trainingStatus.LastTrainingSuccessful = false;
                    _trainingStatus.LastError = $"Model quality too low (R²: {metrics.RSquared:F4}, minimum required: {MachineLearningConfiguration.MinimumModelQuality})";
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during model training");
                _trainingStatus.IsTraining = false;
                _trainingStatus.LastTrainingCompleted = DateTime.UtcNow;
                _trainingStatus.LastTrainingSuccessful = false;
                _trainingStatus.LastError = ex.Message;
                return false;
            }
        }

        private Task SaveModelAsync(ITransformer model)
        {
            var modelPath = MachineLearningConfiguration.GetModelPath();
            
            try
            {
                using var fileStream = new FileStream(
                    modelPath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.Read);

                _mlContext.Model.Save(
                    model,
                    null,
                    fileStream);
                
                _logger.LogInformation("Model saved to {ModelPath}", modelPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving model to {ModelPath}", modelPath);
                throw;
            }

            return Task.CompletedTask;
        }

        private void LoadExistingModel()
        {
            var modelPath = MachineLearningConfiguration.GetModelPath();
            
            if (!File.Exists(modelPath))
            {
                _logger.LogInformation("No existing model found at {ModelPath}. Will train new model.", modelPath);
            }

            try
            {
                using var fileStream = new FileStream(
                    modelPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read);

                var model = _mlContext.Model.Load(fileStream, out var _);
                
                _modelCache.AddOrUpdate("current", model, (key, oldValue) => model);
                _logger.LogInformation("Loaded existing model from {ModelPath}", modelPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading existing model from {ModelPath}. Will train new model.", modelPath);
            }
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