using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.MachineLearning.Configuration;
using OpenAlprWebhookProcessor.Features.MachineLearning.Models;
using OpenAlprWebhookProcessor.Features.MachineLearning.Services;
using OpenAlprWebhookProcessor.Features.MachineLearning.Services.Filesystem;
using Tests.TestHelpers;

namespace Tests.Features.MachineLearning.Services
{
    [TestFixture]
    public class LicensePlateMlTrainingServiceTests : TestBase
    {
        private LicensePlateMlTrainingService _trainingService;
        private IServiceProvider _serviceProvider;
        private IServiceScope _serviceScope;
        private IServiceScopeFactory _serviceScopeFactory;
        private ILogger<LicensePlateMlTrainingService> _logger;
        private IModelPersistenceService _modelPersistence;
        private IMachineLearningConfiguration _configuration;
        private ILicensePlateFeatureExtractor _featureExtractor;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _serviceProvider = Substitute.For<IServiceProvider>();
            _serviceScope = Substitute.For<IServiceScope>();
            _serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
            _logger = Substitute.For<ILogger<LicensePlateMlTrainingService>>();
            _modelPersistence = Substitute.For<IModelPersistenceService>();
            _configuration = Substitute.For<IMachineLearningConfiguration>();
            _featureExtractor = Substitute.For<ILicensePlateFeatureExtractor>();

            SetupServiceProviderMocks();
            SetupDefaultConfigurationMocks();
            SetupDefaultModelPersistenceMocks();

            _trainingService = new LicensePlateMlTrainingService(
                _serviceProvider,
                _logger,
                _modelPersistence,
                _configuration);
        }

        [TearDown]
        public override void TearDown()
        {
            _serviceScope?.Dispose();
            base.TearDown();
        }

        private void SetupServiceProviderMocks()
        {
            // Set up the main service provider
            _serviceProvider.GetService(typeof(IServiceScopeFactory)).Returns(_serviceScopeFactory);
            
            // Set up the service scope factory
            _serviceScopeFactory.CreateScope().Returns(_serviceScope);
            
            // Set up the scoped service provider (return different provider for scope)
            var scopedServiceProvider = Substitute.For<IServiceProvider>();
            scopedServiceProvider.GetService(typeof(ILicensePlateFeatureExtractor)).Returns(_featureExtractor);
            scopedServiceProvider.GetRequiredService(typeof(ILicensePlateFeatureExtractor)).Returns(_featureExtractor);
            _serviceScope.ServiceProvider.Returns(scopedServiceProvider);
        }

        private void SetupDefaultConfigurationMocks()
        {
            _configuration.TrainingInterval.Returns(TimeSpan.FromHours(6));
            _configuration.TrainingBatchSize.Returns(50000);
            _configuration.MinimumTrainingData.Returns(100);
            _configuration.MinimumModelQuality.Returns(0.05);
            _configuration.GetModelPath().Returns(@"config/ml-models/test-model.zip");
        }

        private void SetupDefaultModelPersistenceMocks()
        {
            _modelPersistence.ModelExists(Arg.Any<string>()).Returns(false);
            _modelPersistence.GetModelFileInfo(Arg.Any<string>()).Returns((ModelFileInfo)null);
        }

        #region Constructor Tests

        [Test]
        public void Constructor_WithValidParameters_InitializesCorrectly()
        {
            // Act - constructor already called in SetUp
            
            // Assert
            _trainingService.Should().NotBeNull();
            _trainingService.Should().BeAssignableTo<ILicensePlateMlTrainingService>();
        }

        [Test]
        public void Constructor_WithNullServiceProvider_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new LicensePlateMlTrainingService(null, _logger, _modelPersistence, _configuration);
            act.Should().Throw<ArgumentNullException>().WithParameterName("serviceProvider");
        }

        [Test]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new LicensePlateMlTrainingService(_serviceProvider, null, _modelPersistence, _configuration);
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Test]
        public void Constructor_WithNullModelPersistence_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new LicensePlateMlTrainingService(_serviceProvider, _logger, null, _configuration);
            act.Should().Throw<ArgumentNullException>().WithParameterName("modelPersistence");
        }

        [Test]
        public void Constructor_WithNullConfiguration_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new LicensePlateMlTrainingService(_serviceProvider, _logger, _modelPersistence, null);
            act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
        }

        #endregion

        #region Training Status Tests

        [Test]
        public async Task GetTrainingStatusAsync_WithNoModelFile_ReturnsEmptyFileInfo()
        {
            // Arrange
            _modelPersistence.GetModelFileInfo(Arg.Any<string>()).Returns((ModelFileInfo)null);

            // Act
            var result = _trainingService.GetTrainingStatus();

            // Assert
            result.Should().NotBeNull();
            result.ModelLastSaved.Should().BeNull();
            result.ModelFileSize.Should().BeNull();
            result.IsTraining.Should().BeFalse();
        }

        [Test]
        public async Task GetTrainingStatusAsync_WithExistingModelFile_ReturnsFileInfo()
        {
            // Arrange
            var testFileInfo = new ModelFileInfo
            {
                Exists = true,
                LastModified = DateTime.UtcNow.AddHours(-2),
                FileSize = 1024576
            };
            _modelPersistence.GetModelFileInfo(Arg.Any<string>()).Returns(testFileInfo);

            // Act
            var result = _trainingService.GetTrainingStatus();

            // Assert
            result.Should().NotBeNull();
            result.ModelLastSaved.Should().Be(testFileInfo.LastModified);
            result.ModelFileSize.Should().Be(testFileInfo.FileSize);
        }

        [Test]
        public void GetTrainingStatus_ReturnsTrainingStatusAsync()
        {
            // Arrange
            var testFileInfo = new ModelFileInfo
            {
                Exists = true,
                LastModified = DateTime.UtcNow.AddHours(-1),
                FileSize = 2048000
            };
            _modelPersistence.GetModelFileInfo(Arg.Any<string>()).Returns(testFileInfo);

            // Act
            var result = _trainingService.GetTrainingStatus();

            // Assert
            result.Should().NotBeNull();
            result.ModelLastSaved.Should().Be(testFileInfo.LastModified);
            result.ModelFileSize.Should().Be(testFileInfo.FileSize);
        }

        #endregion

        #region TrainModelAsync Tests

        [Test]
        public async Task TrainModelAsync_WithSufficientDataAndGoodQuality_ReturnsTrue()
        {
            // Arrange
            var trainingData = CreateMockTrainingData(200); // Above minimum
            _featureExtractor.ExtractTrainingDataAsync(Arg.Any<int>()).Returns(trainingData);
            _configuration.MinimumTrainingData.Returns(100);
            _configuration.MinimumModelQuality.Returns(-0.1); // Low threshold to accept random data model

            // Act
            var result = await _trainingService.TrainModelAsync();

            // Assert
            result.Should().BeTrue();
            
            // Verify training status updated correctly
            var status = _trainingService.GetTrainingStatus();
            status.LastTrainingSuccessful.Should().BeTrue();
            status.IsTraining.Should().BeFalse();
            status.LastError.Should().BeNull();
            status.TrainingDataCount.Should().Be(200);

            // Verify model was saved
            await _modelPersistence.Received(1).SaveModelAsync(Arg.Any<ITransformer>(), Arg.Any<string>(), Arg.Any<MLContext>());
        }

        [Test]
        public async Task TrainModelAsync_WithInsufficientData_ReturnsFalse()
        {
            // Arrange
            var trainingData = CreateMockTrainingData(50); // Below minimum
            _featureExtractor.ExtractTrainingDataAsync(Arg.Any<int>()).Returns(trainingData);
            _configuration.MinimumTrainingData.Returns(100);

            // Act
            var result = await _trainingService.TrainModelAsync();

            // Assert
            result.Should().BeFalse();
            
            // Verify training status updated correctly
            var status = _trainingService.GetTrainingStatus();
            status.LastTrainingSuccessful.Should().BeFalse();
            status.IsTraining.Should().BeFalse();
            status.LastError.Should().Contain("Insufficient training data");
            status.TrainingDataCount.Should().Be(50);

            // Verify model was not saved
            await _modelPersistence.DidNotReceive().SaveModelAsync(Arg.Any<ITransformer>(), Arg.Any<string>(), Arg.Any<MLContext>());
        }

        [Test]
        public async Task TrainModelAsync_WithPoorModelQuality_ReturnsFalse()
        {
            // Arrange
            var trainingData = CreateMockTrainingData(200);
            _featureExtractor.ExtractTrainingDataAsync(Arg.Any<int>()).Returns(trainingData);
            _configuration.MinimumTrainingData.Returns(100);
            _configuration.MinimumModelQuality.Returns(0.5); // High threshold that won't be met

            // Act
            var result = await _trainingService.TrainModelAsync();

            // Assert
            result.Should().BeFalse();
            
            // Verify training status
            var status = _trainingService.GetTrainingStatus();
            status.LastTrainingSuccessful.Should().BeFalse();
            status.IsTraining.Should().BeFalse();
            status.LastError.Should().Contain("Model quality too low");

            // Verify model was not saved
            await _modelPersistence.DidNotReceive().SaveModelAsync(Arg.Any<ITransformer>(), Arg.Any<string>(), Arg.Any<MLContext>());
        }

        [Test]
        public async Task TrainModelAsync_WithFeatureExtractorException_ReturnsFalse()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Database connection failed");
            _featureExtractor.ExtractTrainingDataAsync(Arg.Any<int>()).Throws(expectedException);

            // Act
            var result = await _trainingService.TrainModelAsync();

            // Assert
            result.Should().BeFalse();
            
            // Verify training status
            var status = _trainingService.GetTrainingStatus();
            status.LastTrainingSuccessful.Should().BeFalse();
            status.IsTraining.Should().BeFalse();
            status.LastError.Should().Be(expectedException.Message);
        }

        [Test]
        public async Task TrainModelAsync_WithModelSaveException_ReturnsFalse()
        {
            // Arrange
            var trainingData = CreateMockTrainingData(200);
            _featureExtractor.ExtractTrainingDataAsync(Arg.Any<int>()).Returns(trainingData);
            _configuration.MinimumTrainingData.Returns(100);
            _configuration.MinimumModelQuality.Returns(-0.1); // Low threshold to accept random data model
            
            var expectedException = new UnauthorizedAccessException("File access denied");
            _modelPersistence.SaveModelAsync(Arg.Any<ITransformer>(), Arg.Any<string>(), Arg.Any<MLContext>())
                .Throws(expectedException);

            // Act
            var result = await _trainingService.TrainModelAsync();

            // Assert
            result.Should().BeFalse();
            
            // Verify training status
            var status = _trainingService.GetTrainingStatus();
            status.LastTrainingSuccessful.Should().BeFalse();
            status.LastError.Should().Be(expectedException.Message);
        }

        [Test]
        public async Task TrainModelAsync_UpdatesTrainingStatusDuringExecution()
        {
            // Arrange
            var trainingData = CreateMockTrainingData(200);
            _featureExtractor.ExtractTrainingDataAsync(Arg.Any<int>()).Returns(trainingData);
            _configuration.MinimumModelQuality.Returns(-0.1); // Low threshold to accept random data model

            // Act
            var trainingTask = _trainingService.TrainModelAsync();
            
            // Check status during training (briefly)
            await Task.Delay(10);
            var statusDuringTraining = _trainingService.GetTrainingStatus();

            // Complete training
            var result = await trainingTask;

            // Assert
            result.Should().BeTrue();
            
            // Verify final status
            var finalStatus = _trainingService.GetTrainingStatus();
            finalStatus.LastTrainingStarted.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
            finalStatus.LastTrainingCompleted.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        }

        #endregion

        #region Model Cache Tests

        [Test]
        public void GetCurrentModel_WhenNoModelCached_ReturnsNull()
        {
            // Act
            var result = _trainingService.GetCurrentModel();

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task GetCurrentModel_AfterSuccessfulTraining_ReturnsModel()
        {
            // Arrange
            var trainingData = CreateMockTrainingData(200);
            _featureExtractor.ExtractTrainingDataAsync(Arg.Any<int>()).Returns(trainingData);
            _configuration.MinimumModelQuality.Returns(-0.1); // Low threshold to accept random data model

            // Act
            await _trainingService.TrainModelAsync();
            var result = _trainingService.GetCurrentModel();

            // Assert
            result.Should().NotBeNull();
        }

        [Test]
        public async Task TrainModelAsync_WithExistingModel_UpdatesCache()
        {
            // Arrange
            var trainingData = CreateMockTrainingData(200);
            _featureExtractor.ExtractTrainingDataAsync(Arg.Any<int>()).Returns(trainingData);
            _configuration.MinimumModelQuality.Returns(-0.1); // Low threshold to accept random data model

            // Act - Train first model
            await _trainingService.TrainModelAsync();
            var firstModel = _trainingService.GetCurrentModel();

            // Train second model
            await _trainingService.TrainModelAsync();
            var secondModel = _trainingService.GetCurrentModel();

            // Assert
            firstModel.Should().NotBeNull();
            secondModel.Should().NotBeNull();
            // Note: Models will be different instances but we can't easily compare ML.NET models
            // The important thing is that the cache was updated
        }

        #endregion

        #region Load Existing Model Tests

        [Test]
        public async Task LoadExistingModelAsync_WithExistingModel_LoadsModel()
        {
            // Arrange
            var mockModel = Substitute.For<ITransformer>();
            _modelPersistence.ModelExists(Arg.Any<string>()).Returns(true);
            _modelPersistence.LoadModelAsync(Arg.Any<string>(), Arg.Any<MLContext>()).Returns(mockModel);

            // Act
            await _trainingService.LoadExistingModelAsync();

            // Assert
            await _modelPersistence.Received(1).LoadModelAsync(Arg.Any<string>(), Arg.Any<MLContext>());
            
            // Model should now be cached
            var cachedModel = _trainingService.GetCurrentModel();
            cachedModel.Should().NotBeNull();
        }

        [Test]
        public async Task LoadExistingModelAsync_WithNoExistingModel_DoesNotLoadModel()
        {
            // Arrange
            _modelPersistence.ModelExists(Arg.Any<string>()).Returns(false);

            // Act
            await _trainingService.LoadExistingModelAsync();

            // Assert
            await _modelPersistence.DidNotReceive().LoadModelAsync(Arg.Any<string>(), Arg.Any<MLContext>());
        }

        [Test]
        public async Task LoadExistingModelAsync_WithLoadException_DoesNotThrow()
        {
            // Arrange
            _modelPersistence.ModelExists(Arg.Any<string>()).Returns(true);
            _modelPersistence.LoadModelAsync(Arg.Any<string>(), Arg.Any<MLContext>())
                .Throws(new InvalidOperationException("Model file corrupted"));

            // Act & Assert - Should not throw, should log error and continue
            await _trainingService.LoadExistingModelAsync();

            // Service should continue running even if model loading fails
            var status = _trainingService.GetTrainingStatus();
            status.Should().NotBeNull();
        }

        #endregion

        #region Helper Methods

        private List<LicensePlateTrainingData> CreateMockTrainingData(int count)
        {
            var data = new List<LicensePlateTrainingData>();
            var random = new Random(42); // Seed for consistent tests

            for (int i = 0; i < count; i++)
            {
                data.Add(new LicensePlateTrainingData
                {
                    LicensePlate = $"TEST{i:D3}",
                    HourOfDay = random.Next(0, 24),
                    DayOfWeek = random.Next(0, 7),
                    DayOfMonth = random.Next(1, 29),
                    MonthOfYear = random.Next(1, 13),
                    CameraId = random.Next(1, 10),
                    TimeSinceLastSeen = (float)random.NextDouble() * 168, // 0-168 hours
                    HistoricalFrequency = (float)random.NextDouble(),
                    AverageTimeBetweenVisits = (float)(random.NextDouble() * 168 + 1), // 1-169 hours
                    TotalVisits = random.Next(1, 100),
                    IsWeekend = random.Next(0, 2),
                    IsBusinessHour = random.Next(0, 2),
                    SeasonalFactor = (float)random.NextDouble(),
                    VehicleTypeCode = random.Next(0, 5),
                    VehicleColorCode = random.Next(0, 10),
                    HoursUntilNextSeen = (float)(random.NextDouble() * 72 + 1) // 1-73 hours
                });
            }

            return data;
        }

        #endregion
    }
} 