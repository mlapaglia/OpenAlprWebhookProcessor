using FluentAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.MachineLearning.Models;
using System;

namespace Tests.Features.MachineLearning.Models
{
    [TestFixture]
    public class MachineLearningDtoTests
    {
        #region TrainingStatusDto Tests

        [Test]
        public void TrainingStatusDto_Properties_GetAndSetCorrectly()
        {
            // Arrange
            var dto = new TrainingStatusDto();
            var lastTrainingStarted = DateTime.UtcNow.AddHours(-2);
            var lastTrainingCompleted = DateTime.UtcNow.AddHours(-1);
            var lastError = "Test error message";
            var trainingDataCount = 1500;
            var modelMetrics = new ModelMetricsDto();
            var modelFile = new ModelFileDto();
            var configuration = new TrainingConfigurationDto();

            // Act
            dto.IsTraining = true;
            dto.LastTrainingStarted = lastTrainingStarted;
            dto.LastTrainingCompleted = lastTrainingCompleted;
            dto.LastTrainingSuccessful = false;
            dto.LastError = lastError;
            dto.TrainingDataCount = trainingDataCount;
            dto.ModelMetrics = modelMetrics;
            dto.ModelFile = modelFile;
            dto.Configuration = configuration;

            // Assert
            dto.IsTraining.Should().BeTrue();
            dto.LastTrainingStarted.Should().Be(lastTrainingStarted);
            dto.LastTrainingCompleted.Should().Be(lastTrainingCompleted);
            dto.LastTrainingSuccessful.Should().BeFalse();
            dto.LastError.Should().Be(lastError);
            dto.TrainingDataCount.Should().Be(trainingDataCount);
            dto.ModelMetrics.Should().Be(modelMetrics);
            dto.ModelFile.Should().Be(modelFile);
            dto.Configuration.Should().Be(configuration);
        }

        [Test]
        public void TrainingStatusDto_DefaultValues_AreCorrect()
        {
            // Act
            var dto = new TrainingStatusDto();

            // Assert
            dto.IsTraining.Should().BeFalse();
            dto.LastTrainingStarted.Should().BeNull();
            dto.LastTrainingCompleted.Should().BeNull();
            dto.LastTrainingSuccessful.Should().BeFalse();
            dto.LastError.Should().BeNull();
            dto.TrainingDataCount.Should().Be(0);
            dto.ModelMetrics.Should().BeNull();
            dto.ModelFile.Should().BeNull();
            dto.Configuration.Should().BeNull();
        }

        #endregion

        #region ModelMetricsDto Tests

        [Test]
        public void ModelMetricsDto_Properties_GetAndSetCorrectly()
        {
            // Arrange
            var dto = new ModelMetricsDto();
            var rSquared = 0.85;
            var meanAbsoluteError = 12.5;
            var rootMeanSquaredError = 15.7;

            // Act
            dto.RSquared = rSquared;
            dto.MeanAbsoluteError = meanAbsoluteError;
            dto.RootMeanSquaredError = rootMeanSquaredError;

            // Assert
            dto.RSquared.Should().Be(rSquared);
            dto.MeanAbsoluteError.Should().Be(meanAbsoluteError);
            dto.RootMeanSquaredError.Should().Be(rootMeanSquaredError);
        }

        [Test]
        public void ModelMetricsDto_DefaultValues_AreZero()
        {
            // Act
            var dto = new ModelMetricsDto();

            // Assert
            dto.RSquared.Should().Be(0);
            dto.MeanAbsoluteError.Should().Be(0);
            dto.RootMeanSquaredError.Should().Be(0);
        }

        #endregion

        #region ModelFileDto Tests

        [Test]
        public void ModelFileDto_Properties_GetAndSetCorrectly()
        {
            // Arrange
            var dto = new ModelFileDto();
            var lastSaved = DateTime.UtcNow.AddMinutes(-30);
            var fileSizeBytes = 1024000L;

            // Act
            dto.LastSaved = lastSaved;
            dto.FileSizeBytes = fileSizeBytes;

            // Assert
            dto.LastSaved.Should().Be(lastSaved);
            dto.FileSizeBytes.Should().Be(fileSizeBytes);
        }

        [Test]
        public void ModelFileDto_DefaultValues_AreCorrect()
        {
            // Act
            var dto = new ModelFileDto();

            // Assert
            dto.LastSaved.Should().Be(default(DateTime));
            dto.FileSizeBytes.Should().Be(0);
        }

        #endregion

        #region TrainingConfigurationDto Tests

        [Test]
        public void TrainingConfigurationDto_Properties_GetAndSetCorrectly()
        {
            // Arrange
            var dto = new TrainingConfigurationDto();
            var trainingInterval = "Every 8 hours";
            var minimumTrainingData = 500;
            var minimumModelQuality = 0.75;
            var batchSize = 25000;

            // Act
            dto.TrainingInterval = trainingInterval;
            dto.MinimumTrainingData = minimumTrainingData;
            dto.MinimumModelQuality = minimumModelQuality;
            dto.BatchSize = batchSize;

            // Assert
            dto.TrainingInterval.Should().Be(trainingInterval);
            dto.MinimumTrainingData.Should().Be(minimumTrainingData);
            dto.MinimumModelQuality.Should().Be(minimumModelQuality);
            dto.BatchSize.Should().Be(batchSize);
        }

        [Test]
        public void TrainingConfigurationDto_DefaultValues_AreCorrect()
        {
            // Act
            var dto = new TrainingConfigurationDto();

            // Assert
            dto.TrainingInterval.Should().BeNull();
            dto.MinimumTrainingData.Should().Be(0);
            dto.MinimumModelQuality.Should().Be(0);
            dto.BatchSize.Should().Be(0);
        }

        #endregion

        #region TrainingResultDto Tests

        [Test]
        public void TrainingResultDto_Properties_GetAndSetCorrectly()
        {
            // Arrange
            var dto = new TrainingResultDto();
            var message = "Training completed successfully";
            var timestamp = DateTime.UtcNow;
            var success = true;

            // Act
            dto.Message = message;
            dto.Timestamp = timestamp;
            dto.Success = success;

            // Assert
            dto.Message.Should().Be(message);
            dto.Timestamp.Should().Be(timestamp);
            dto.Success.Should().Be(success);
        }

        [Test]
        public void TrainingResultDto_DefaultValues_AreCorrect()
        {
            // Act
            var dto = new TrainingResultDto();

            // Assert
            dto.Message.Should().BeNull();
            dto.Timestamp.Should().Be(default(DateTime));
            dto.Success.Should().BeFalse();
        }

        #endregion

        #region ModelStatusDto Tests

        [Test]
        public void ModelStatusDto_Properties_GetAndSetCorrectly()
        {
            // Arrange
            var dto = new ModelStatusDto();
            var modelAvailable = true;
            var status = "Ready for predictions";
            var lastChecked = DateTime.UtcNow;

            // Act
            dto.ModelAvailable = modelAvailable;
            dto.Status = status;
            dto.LastChecked = lastChecked;

            // Assert
            dto.ModelAvailable.Should().Be(modelAvailable);
            dto.Status.Should().Be(status);
            dto.LastChecked.Should().Be(lastChecked);
        }

        [Test]
        public void ModelStatusDto_DefaultValues_AreCorrect()
        {
            // Act
            var dto = new ModelStatusDto();

            // Assert
            dto.ModelAvailable.Should().BeFalse();
            dto.Status.Should().BeNull();
            dto.LastChecked.Should().Be(default(DateTime));
        }

        #endregion

        #region ModelInfoDto Tests

        [Test]
        public void ModelInfoDto_Properties_GetAndSetCorrectly()
        {
            // Arrange
            var dto = new ModelInfoDto();
            var modelAvailable = true;
            var modelType = "Regression";
            var features = new[] { "LicensePlate", "TimeDelta", "CameraId" };
            var description = "License plate prediction model";
            var trainingSchedule = "Every 6 hours";
            var lastUpdated = "2023-10-15T14:30:00Z";

            // Act
            dto.ModelAvailable = modelAvailable;
            dto.ModelType = modelType;
            dto.Features = features;
            dto.Description = description;
            dto.TrainingSchedule = trainingSchedule;
            dto.LastUpdated = lastUpdated;

            // Assert
            dto.ModelAvailable.Should().Be(modelAvailable);
            dto.ModelType.Should().Be(modelType);
            dto.Features.Should().BeEquivalentTo(features);
            dto.Description.Should().Be(description);
            dto.TrainingSchedule.Should().Be(trainingSchedule);
            dto.LastUpdated.Should().Be(lastUpdated);
        }

        [Test]
        public void ModelInfoDto_DefaultValues_AreCorrect()
        {
            // Act
            var dto = new ModelInfoDto();

            // Assert
            dto.ModelAvailable.Should().BeFalse();
            dto.ModelType.Should().BeNull();
            dto.Features.Should().BeNull();
            dto.Description.Should().BeNull();
            dto.TrainingSchedule.Should().BeNull();
            dto.LastUpdated.Should().BeNull();
        }

        #endregion

        #region Integration Tests

        [Test]
        public void TrainingStatusDto_WithAllNestedObjects_WorksCorrectly()
        {
            // Arrange
            var modelMetrics = new ModelMetricsDto
            {
                RSquared = 0.92,
                MeanAbsoluteError = 8.5,
                RootMeanSquaredError = 12.1
            };

            var modelFile = new ModelFileDto
            {
                LastSaved = DateTime.UtcNow.AddHours(-1),
                FileSizeBytes = 2048000L
            };

            var configuration = new TrainingConfigurationDto
            {
                TrainingInterval = "Every 6 hours",
                MinimumTrainingData = 1000,
                MinimumModelQuality = 0.8,
                BatchSize = 50000
            };

            // Act
            var trainingStatus = new TrainingStatusDto
            {
                IsTraining = false,
                LastTrainingStarted = DateTime.UtcNow.AddHours(-2),
                LastTrainingCompleted = DateTime.UtcNow.AddHours(-1),
                LastTrainingSuccessful = true,
                LastError = null,
                TrainingDataCount = 2500,
                ModelMetrics = modelMetrics,
                ModelFile = modelFile,
                Configuration = configuration
            };

            // Assert
            trainingStatus.ModelMetrics.RSquared.Should().Be(0.92);
            trainingStatus.ModelFile.FileSizeBytes.Should().Be(2048000L);
            trainingStatus.Configuration.BatchSize.Should().Be(50000);
        }

        [Test]
        public void AllDtoTypes_CanBeInstantiated_WithoutErrors()
        {
            // Act & Assert
            var act1 = () => new TrainingStatusDto();
            var act2 = () => new ModelMetricsDto();
            var act3 = () => new ModelFileDto();
            var act4 = () => new TrainingConfigurationDto();
            var act5 = () => new TrainingResultDto();
            var act6 = () => new ModelStatusDto();
            var act7 = () => new ModelInfoDto();

            act1.Should().NotThrow();
            act2.Should().NotThrow();
            act3.Should().NotThrow();
            act4.Should().NotThrow();
            act5.Should().NotThrow();
            act6.Should().NotThrow();
            act7.Should().NotThrow();
        }

        #endregion
    }
} 