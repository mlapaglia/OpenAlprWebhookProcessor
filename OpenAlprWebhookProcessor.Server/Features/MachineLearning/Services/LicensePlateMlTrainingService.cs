using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using OpenAlprWebhookProcessor.Features.MachineLearning.Configuration;
using OpenAlprWebhookProcessor.Features.MachineLearning.Services.Filesystem;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Services
{
    public class LicensePlateMlTrainingService : ILicensePlateMlTrainingService
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger<LicensePlateMlTrainingService> _logger;

        private readonly IModelPersistenceService _modelPersistence;

        private readonly IMachineLearningConfiguration _configuration;

        private readonly MLContext _mlContext;

        private readonly ConcurrentDictionary<string, ITransformer> _modelCache;

        private readonly TrainingStatus _trainingStatus;

        public LicensePlateMlTrainingService(
            IServiceProvider serviceProvider,
            ILogger<LicensePlateMlTrainingService> logger,
            IModelPersistenceService modelPersistence,
            IMachineLearningConfiguration configuration)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _modelPersistence = modelPersistence ?? throw new ArgumentNullException(nameof(modelPersistence));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _mlContext = new MLContext(seed: 42);
            _modelCache = new ConcurrentDictionary<string, ITransformer>();
            _trainingStatus = new TrainingStatus();
        }

        public TrainingStatus GetTrainingStatus()
        {
            var modelPath = _configuration.GetModelPath();
            var fileInfo = _modelPersistence.GetModelFileInfo(modelPath);

            if (fileInfo?.Exists == true)
            {
                _trainingStatus.ModelLastSaved = fileInfo.LastModified;
                _trainingStatus.ModelFileSize = fileInfo.FileSize;
            }

            return _trainingStatus;
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

                _logger.LogDebug("Extracting training data from database...");
                var trainingBatchSize = _configuration.TrainingBatchSize;
                var trainingData = await featureExtractor.ExtractTrainingDataAsync(trainingBatchSize);

                _trainingStatus.TrainingDataCount = trainingData.Count;

                var minimumTrainingData = _configuration.MinimumTrainingData;
                if (trainingData.Count < minimumTrainingData)
                {
                    _logger.LogWarning("Insufficient training data ({Count} samples). Skipping training.", trainingData.Count);
                    _trainingStatus.IsTraining = false;
                    _trainingStatus.LastTrainingCompleted = DateTime.UtcNow;
                    _trainingStatus.LastTrainingSuccessful = false;
                    _trainingStatus.LastError = $"Insufficient training data ({trainingData.Count} samples, minimum required: {minimumTrainingData})";
                    return false;
                }

                _logger.LogDebug("Training with {Count} samples", trainingData.Count);

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

                _logger.LogDebug("Training model pipeline...");
                var model = pipeline.Fit(trainData);

                _logger.LogDebug("Evaluating model performance...");
                var predictions = model.Transform(testData);
                var metrics = _mlContext.Regression.Evaluate(predictions);

                _logger.LogDebug("Model training completed. Metrics:");
                _logger.LogDebug("R-Squared: {RSquared:F4}", metrics.RSquared);
                _logger.LogDebug("Mean Absolute Error: {MAE:F2} hours", metrics.MeanAbsoluteError);
                _logger.LogDebug("Root Mean Squared Error: {RMSE:F2} hours", metrics.RootMeanSquaredError);

                _trainingStatus.RSquared = metrics.RSquared;
                _trainingStatus.MeanAbsoluteError = metrics.MeanAbsoluteError;
                _trainingStatus.RootMeanSquaredError = metrics.RootMeanSquaredError;

                var minimumModelQuality = _configuration.MinimumModelQuality;
                if (metrics.RSquared > minimumModelQuality)
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
                    _trainingStatus.LastError = $"Model quality too low (R²: {metrics.RSquared:F4}, minimum required: {minimumModelQuality})";
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

        private async Task SaveModelAsync(ITransformer model)
        {
            var modelPath = _configuration.GetModelPath();

            try
            {
                await _modelPersistence.SaveModelAsync(model, modelPath, _mlContext);
                _logger.LogDebug("Model saved to {ModelPath}", modelPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving model to {ModelPath}", modelPath);
                throw;
            }
        }

        public void LoadExistingModel()
        {
            var modelPath = _configuration.GetModelPath();

            if (!_modelPersistence.ModelExists(modelPath))
            {
                _logger.LogInformation("No existing model found at {ModelPath}. Will train new model.", modelPath);
                return;
            }

            try
            {
                var model = _modelPersistence.LoadModel(modelPath, _mlContext);

                if (model != null)
                {
                    _modelCache.AddOrUpdate("current", model, (key, oldValue) => model);
                    _logger.LogInformation("Loaded existing model from {ModelPath}", modelPath);
                }
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
    }
}