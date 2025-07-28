using AwesomeAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.MachineLearning.Configuration;

namespace Tests.Features.MachineLearning.Configuration
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class MachineLearningConfigurationTests
    {
        private IOptions<MachineLearningOptions> _mockOptions;
        private MachineLearningOptions _options;
        private MachineLearningConfiguration _configuration;

        [SetUp]
        public void SetUp()
        {
            _options = new MachineLearningOptions
            {
                ModelFileName = "test-model.zip",
                ConfigFolderName = "test-config",
                MlModelsFolderName = "test-ml-models",
                TrainingInterval = TimeSpan.FromHours(4),
                TrainingBatchSize = 10000,
                MinimumTrainingData = 50,
                MinimumModelQuality = 0.7
            };

            _mockOptions = Substitute.For<IOptions<MachineLearningOptions>>();
            _mockOptions.Value.Returns(_options);

            _configuration = new MachineLearningConfiguration(_mockOptions);
        }

        [Test]
        public void Constructor_WithValidOptions_InitializesCorrectly()
        {
            // Act & Assert
            _configuration.Should().NotBeNull();
        }

        [Test]
        public void ModelFileName_ReturnsOptionsValue()
        {
            // Act
            var result = _configuration.ModelFileName;

            // Assert
            result.Should().Be("test-model.zip");
        }

        [Test]
        public void ConfigFolderName_ReturnsOptionsValue()
        {
            // Act
            var result = _configuration.ConfigFolderName;

            // Assert
            result.Should().Be("test-config");
        }

        [Test]
        public void MlModelsFolderName_ReturnsOptionsValue()
        {
            // Act
            var result = _configuration.MlModelsFolderName;

            // Assert
            result.Should().Be("test-ml-models");
        }

        [Test]
        public void TrainingInterval_ReturnsOptionsValue()
        {
            // Act
            var result = _configuration.TrainingInterval;

            // Assert
            result.Should().Be(TimeSpan.FromHours(4));
        }

        [Test]
        public void TrainingBatchSize_ReturnsOptionsValue()
        {
            // Act
            var result = _configuration.TrainingBatchSize;

            // Assert
            result.Should().Be(10000);
        }

        [Test]
        public void MinimumTrainingData_ReturnsOptionsValue()
        {
            // Act
            var result = _configuration.MinimumTrainingData;

            // Assert
            result.Should().Be(50);
        }

        [Test]
        public void MinimumModelQuality_ReturnsOptionsValue()
        {
            // Act
            var result = _configuration.MinimumModelQuality;

            // Assert
            result.Should().Be(0.7);
        }

        [Test]
        public void GetConfigPath_ReturnsExpectedPath()
        {
            // Act
            var result = _configuration.GetConfigPath();

            // Assert
            var expectedPath = Path.Combine(Directory.GetCurrentDirectory(), "test-config");
            result.Should().Be(expectedPath);
        }

        [Test]
        public void GetModelPath_ReturnsExpectedPath()
        {
            // Act
            var result = _configuration.GetModelPath();

            // Assert
            var expectedPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "test-config",
                "test-ml-models",
                "test-model.zip");
            result.Should().Be(expectedPath);
        }

        [Test]
        public void GetBackupPath_WithTimestamp_ReturnsExpectedPath()
        {
            // Arrange
            var timestamp = new DateTime(2023, 10, 15, 14, 30, 45);

            // Act
            var result = _configuration.GetBackupPath(timestamp);

            // Assert
            var expectedPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "test-config",
                "test-ml-models",
                "backup-20231015-143045-test-model.zip");
            result.Should().Be(expectedPath);
        }

        [Test]
        public void GetBackupPath_WithDifferentTimestamp_GeneratesDifferentPath()
        {
            // Arrange
            var timestamp1 = new DateTime(2023, 10, 15, 14, 30, 45);
            var timestamp2 = new DateTime(2023, 10, 16, 16, 45, 30);

            // Act
            var result1 = _configuration.GetBackupPath(timestamp1);
            var result2 = _configuration.GetBackupPath(timestamp2);

            // Assert
            result1.Should().NotBe(result2);
            result1.Should().Contain("backup-20231015-143045");
            result2.Should().Contain("backup-20231016-164530");
        }

        [Test]
        public void GetConfigPath_CreatesDirectoryIfNotExists()
        {
            // Arrange
            var tempConfigFolder = Path.Combine(Path.GetTempPath(), "test-config-" + Guid.NewGuid());
            _options.ConfigFolderName = tempConfigFolder;

            try
            {
                // Act
                var result = _configuration.GetConfigPath();

                // Assert
                Directory.Exists(result).Should().BeTrue();
                Directory.Exists(Path.Combine(result, "test-ml-models")).Should().BeTrue();
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempConfigFolder))
                {
                    Directory.Delete(tempConfigFolder, true);
                }
            }
        }

        [Test]
        public void AllProperties_WithDefaultOptions_ReturnDefaults()
        {
            // Arrange
            var defaultOptions = new MachineLearningOptions();
            _mockOptions.Value.Returns(defaultOptions);
            var configWithDefaults = new MachineLearningConfiguration(_mockOptions);

            // Act & Assert
            configWithDefaults.ModelFileName.Should().Be("license-plate-prediction-model.zip");
            configWithDefaults.ConfigFolderName.Should().Be("config");
            configWithDefaults.MlModelsFolderName.Should().Be("ml-models");
            configWithDefaults.TrainingInterval.Should().Be(TimeSpan.FromHours(6));
            configWithDefaults.TrainingBatchSize.Should().Be(50000);
            configWithDefaults.MinimumTrainingData.Should().Be(100);
            configWithDefaults.MinimumModelQuality.Should().Be(0.05);
        }

        [Test]
        public void PathMethods_WithSpecialCharacters_HandleCorrectly()
        {
            // Arrange
            _options.ConfigFolderName = "config with spaces";
            _options.MlModelsFolderName = "ml-models-special";
            _options.ModelFileName = "model file.zip";

            // Act
            var configPath = _configuration.GetConfigPath();
            var modelPath = _configuration.GetModelPath();
            var backupPath = _configuration.GetBackupPath(DateTime.Now);

            // Assert
            configPath.Should().Contain("config with spaces");
            modelPath.Should().Contain("model file.zip");
            backupPath.Should().Contain("ml-models-special");
        }

        [Test]
        public void GetBackupPath_WithMinDateTime_HandlesCorrectly()
        {
            // Act
            var result = _configuration.GetBackupPath(DateTime.MinValue);

            // Assert
            result.Should().Contain("backup-00010101-000000");
            result.Should().EndWith("test-model.zip");
        }

        [Test]
        public void GetBackupPath_WithMaxDateTime_HandlesCorrectly()
        {
            // Act
            var result = _configuration.GetBackupPath(DateTime.MaxValue);

            // Assert
            result.Should().Contain("backup-99991231-235959");
            result.Should().EndWith("test-model.zip");
        }

        [Test]
        public void Interface_Implementation_IsCorrect()
        {
            // Act & Assert
            _configuration.Should().BeAssignableTo<IMachineLearningConfiguration>();
        }
    }
} 