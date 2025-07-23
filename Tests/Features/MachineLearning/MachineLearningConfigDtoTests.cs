using FluentAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.MachineLearning;
using System.ComponentModel.DataAnnotations;

namespace Tests.Features.MachineLearning
{
    [TestFixture]
    public class MachineLearningConfigDtoTests
    {
        [Test]
        public void Constructor_DefaultValues_AreCorrect()
        {
            // Act
            var dto = new MachineLearningConfigDto();

            // Assert
            dto.MinimumModelQuality.Should().Be(0.01);
            dto.MinimumTrainingData.Should().Be(100);
            dto.TrainingBatchSize.Should().Be(50000);
            dto.TrainingInterval.Should().Be(TimeSpan.FromHours(6));
            dto.ModelFileName.Should().Be("license-plate-prediction-model.zip");
            dto.ConfigFolderName.Should().Be("config");
            dto.MlModelsFolderName.Should().Be("ml-models");
            dto.LastUpdated.Should().BeNull();
            dto.UpdatedBy.Should().BeNull();
        }

        [Test]
        public void Properties_GetAndSetCorrectly()
        {
            // Arrange
            var dto = new MachineLearningConfigDto();
            var lastUpdated = DateTime.UtcNow;
            var updatedBy = "TestUser";

            // Act
            dto.MinimumModelQuality = 0.8;
            dto.MinimumTrainingData = 500;
            dto.TrainingBatchSize = 25000;
            dto.TrainingInterval = TimeSpan.FromHours(8);
            dto.ModelFileName = "custom-model.zip";
            dto.ConfigFolderName = "custom-config";
            dto.MlModelsFolderName = "custom-ml-models";
            dto.LastUpdated = lastUpdated;
            dto.UpdatedBy = updatedBy;

            // Assert
            dto.MinimumModelQuality.Should().Be(0.8);
            dto.MinimumTrainingData.Should().Be(500);
            dto.TrainingBatchSize.Should().Be(25000);
            dto.TrainingInterval.Should().Be(TimeSpan.FromHours(8));
            dto.ModelFileName.Should().Be("custom-model.zip");
            dto.ConfigFolderName.Should().Be("custom-config");
            dto.MlModelsFolderName.Should().Be("custom-ml-models");
            dto.LastUpdated.Should().Be(lastUpdated);
            dto.UpdatedBy.Should().Be(updatedBy);
        }

        [Test]
        public void MinimumModelQuality_HasCorrectValidationAttributes()
        {
            // Arrange
            var property = typeof(MachineLearningConfigDto).GetProperty(nameof(MachineLearningConfigDto.MinimumModelQuality));

            // Act
            var requiredAttribute = property.GetCustomAttributes(typeof(RequiredAttribute), false).FirstOrDefault() as RequiredAttribute;
            var rangeAttribute = property.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.RangeAttribute), false).FirstOrDefault() as System.ComponentModel.DataAnnotations.RangeAttribute;

            // Assert
            requiredAttribute.Should().NotBeNull();
            rangeAttribute.Should().NotBeNull();
            rangeAttribute.Minimum.Should().Be(0.001);
            rangeAttribute.Maximum.Should().Be(1.0);
        }

        [Test]
        public void MinimumTrainingData_HasCorrectValidationAttributes()
        {
            // Arrange
            var property = typeof(MachineLearningConfigDto).GetProperty(nameof(MachineLearningConfigDto.MinimumTrainingData));

            // Act
            var requiredAttribute = property.GetCustomAttributes(typeof(RequiredAttribute), false).FirstOrDefault() as RequiredAttribute;
            var rangeAttribute = property.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.RangeAttribute), false).FirstOrDefault() as System.ComponentModel.DataAnnotations.RangeAttribute;

            // Assert
            requiredAttribute.Should().NotBeNull();
            rangeAttribute.Should().NotBeNull();
            rangeAttribute.Minimum.Should().Be(10);
            rangeAttribute.Maximum.Should().Be(1000000);
        }

        [Test]
        public void TrainingBatchSize_HasCorrectValidationAttributes()
        {
            // Arrange
            var property = typeof(MachineLearningConfigDto).GetProperty(nameof(MachineLearningConfigDto.TrainingBatchSize));

            // Act
            var requiredAttribute = property.GetCustomAttributes(typeof(RequiredAttribute), false).FirstOrDefault() as RequiredAttribute;
            var rangeAttribute = property.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.RangeAttribute), false).FirstOrDefault() as System.ComponentModel.DataAnnotations.RangeAttribute;

            // Assert
            requiredAttribute.Should().NotBeNull();
            rangeAttribute.Should().NotBeNull();
            rangeAttribute.Minimum.Should().Be(1000);
            rangeAttribute.Maximum.Should().Be(1000000);
        }

        [Test]
        public void TrainingInterval_HasRequiredAttribute()
        {
            // Arrange
            var property = typeof(MachineLearningConfigDto).GetProperty(nameof(MachineLearningConfigDto.TrainingInterval));

            // Act
            var requiredAttribute = property.GetCustomAttributes(typeof(RequiredAttribute), false).FirstOrDefault() as RequiredAttribute;

            // Assert
            requiredAttribute.Should().NotBeNull();
        }

        [Test]
        public void ModelFileName_HasCorrectValidationAttributes()
        {
            // Arrange
            var property = typeof(MachineLearningConfigDto).GetProperty(nameof(MachineLearningConfigDto.ModelFileName));

            // Act
            var requiredAttribute = property.GetCustomAttributes(typeof(RequiredAttribute), false).FirstOrDefault() as RequiredAttribute;
            var maxLengthAttribute = property.GetCustomAttributes(typeof(MaxLengthAttribute), false).FirstOrDefault() as MaxLengthAttribute;

            // Assert
            requiredAttribute.Should().NotBeNull();
            maxLengthAttribute.Should().NotBeNull();
            maxLengthAttribute.Length.Should().Be(255);
        }

        [Test]
        public void ConfigFolderName_HasCorrectValidationAttributes()
        {
            // Arrange
            var property = typeof(MachineLearningConfigDto).GetProperty(nameof(MachineLearningConfigDto.ConfigFolderName));

            // Act
            var requiredAttribute = property.GetCustomAttributes(typeof(RequiredAttribute), false).FirstOrDefault() as RequiredAttribute;
            var maxLengthAttribute = property.GetCustomAttributes(typeof(MaxLengthAttribute), false).FirstOrDefault() as MaxLengthAttribute;

            // Assert
            requiredAttribute.Should().NotBeNull();
            maxLengthAttribute.Should().NotBeNull();
            maxLengthAttribute.Length.Should().Be(255);
        }

        [Test]
        public void MlModelsFolderName_HasCorrectValidationAttributes()
        {
            // Arrange
            var property = typeof(MachineLearningConfigDto).GetProperty(nameof(MachineLearningConfigDto.MlModelsFolderName));

            // Act
            var requiredAttribute = property.GetCustomAttributes(typeof(RequiredAttribute), false).FirstOrDefault() as RequiredAttribute;
            var maxLengthAttribute = property.GetCustomAttributes(typeof(MaxLengthAttribute), false).FirstOrDefault() as MaxLengthAttribute;

            // Assert
            requiredAttribute.Should().NotBeNull();
            maxLengthAttribute.Should().NotBeNull();
            maxLengthAttribute.Length.Should().Be(255);
        }

        [Test]
        public void LastUpdated_DoesNotHaveRequiredAttribute()
        {
            // Arrange
            var property = typeof(MachineLearningConfigDto).GetProperty(nameof(MachineLearningConfigDto.LastUpdated));

            // Act
            var requiredAttribute = property.GetCustomAttributes(typeof(RequiredAttribute), false).FirstOrDefault() as RequiredAttribute;

            // Assert
            requiredAttribute.Should().BeNull();
        }

        [Test]
        public void UpdatedBy_DoesNotHaveRequiredAttribute()
        {
            // Arrange
            var property = typeof(MachineLearningConfigDto).GetProperty(nameof(MachineLearningConfigDto.UpdatedBy));

            // Act
            var requiredAttribute = property.GetCustomAttributes(typeof(RequiredAttribute), false).FirstOrDefault() as RequiredAttribute;

            // Assert
            requiredAttribute.Should().BeNull();
        }

        [Test]
        public void TrainingInterval_WithVariousTimeSpans_WorksCorrectly()
        {
            // Arrange
            var dto = new MachineLearningConfigDto();

            // Act & Assert
            dto.TrainingInterval = TimeSpan.FromMinutes(30);
            dto.TrainingInterval.Should().Be(TimeSpan.FromMinutes(30));

            dto.TrainingInterval = TimeSpan.FromHours(12);
            dto.TrainingInterval.Should().Be(TimeSpan.FromHours(12));

            dto.TrainingInterval = TimeSpan.FromDays(1);
            dto.TrainingInterval.Should().Be(TimeSpan.FromDays(1));
        }

        [Test]
        public void AllProperties_CanBeSetToNull_WhereApplicable()
        {
            // Arrange
            var dto = new MachineLearningConfigDto();

            // Act & Assert - Nullable properties
            dto.LastUpdated = null;
            dto.UpdatedBy = null;
            dto.ModelFileName = null;
            dto.ConfigFolderName = null;
            dto.MlModelsFolderName = null;

            dto.LastUpdated.Should().BeNull();
            dto.UpdatedBy.Should().BeNull();
            dto.ModelFileName.Should().BeNull();
            dto.ConfigFolderName.Should().BeNull();
            dto.MlModelsFolderName.Should().BeNull();
        }

        [Test]
        public void EdgeValues_WorkCorrectly()
        {
            // Arrange
            var dto = new MachineLearningConfigDto();

            // Act
            dto.MinimumModelQuality = 1.0; // Maximum allowed
            dto.MinimumTrainingData = 10; // Minimum allowed
            dto.TrainingBatchSize = 1000000; // Maximum allowed

            // Assert
            dto.MinimumModelQuality.Should().Be(1.0);
            dto.MinimumTrainingData.Should().Be(10);
            dto.TrainingBatchSize.Should().Be(1000000);
        }

        [Test]
        public void ToString_ReturnsExpectedFormat()
        {
            // Arrange
            var dto = new MachineLearningConfigDto
            {
                MinimumModelQuality = 0.8,
                MinimumTrainingData = 500
            };

            // Act
            var result = dto.ToString();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
        }
    }
} 