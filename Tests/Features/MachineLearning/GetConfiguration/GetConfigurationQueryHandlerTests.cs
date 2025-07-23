using FluentAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Features.MachineLearning.Queries.GetConfiguration;
using Tests.TestHelpers;

namespace Tests.Features.MachineLearning.GetConfiguration
{
    [TestFixture]
    public class GetConfigurationQueryHandlerTests : TestBase
    {
        [Test]
        public async Task Handle_WithNoExistingData_ReturnsDefaults()
        {
            // Arrange
            var handler = new GetConfigurationQueryHandler(UnitOfWork);
            var query = new GetConfigurationQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.MinimumModelQuality.Should().Be(0.01);
            result.MinimumTrainingData.Should().Be(100);
            result.TrainingBatchSize.Should().Be(50000);
            result.TrainingInterval.Should().Be(TimeSpan.FromHours(6));
            result.ModelFileName.Should().Be("license-plate-prediction-model.zip");
            result.ConfigFolderName.Should().Be("config");
            result.MlModelsFolderName.Should().Be("ml-models");
            result.LastUpdated.Should().BeNull();
            result.UpdatedBy.Should().BeNull();
        }

        [Test]
        public async Task Handle_WithExistingData_ReturnsStoredValues()
        {
            // Arrange
            await SeedTestConfigurationAsync();
            var handler = new GetConfigurationQueryHandler(UnitOfWork);
            var query = new GetConfigurationQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.MinimumModelQuality.Should().Be(0.05);
            result.MinimumTrainingData.Should().Be(200);
            result.TrainingBatchSize.Should().Be(25000);
            result.TrainingInterval.Should().Be(TimeSpan.FromHours(12));
            result.ModelFileName.Should().Be("custom-model.zip");
            result.LastUpdated.Should().NotBeNull();
            result.UpdatedBy.Should().Be("TestUser");
        }

        private async Task SeedTestConfigurationAsync()
        {
            var configs = new[]
            {
                new MachineLearningConfiguration
                {
                    Key = "MinimumModelQuality",
                    Value = "0.05",
                    ValueType = "double",
                    Description = "Test minimum model quality",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedBy = "TestUser"
                },
                new MachineLearningConfiguration
                {
                    Key = "MinimumTrainingData",
                    Value = "200",
                    ValueType = "int",
                    Description = "Test minimum training data",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedBy = "TestUser"
                },
                new MachineLearningConfiguration
                {
                    Key = "TrainingBatchSize",
                    Value = "25000",
                    ValueType = "int",
                    Description = "Test training batch size",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedBy = "TestUser"
                },
                new MachineLearningConfiguration
                {
                    Key = "TrainingInterval",
                    Value = "12:00:00",
                    ValueType = "timespan",
                    Description = "Test training interval",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedBy = "TestUser"
                },
                new MachineLearningConfiguration
                {
                    Key = "ModelFileName",
                    Value = "custom-model.zip",
                    ValueType = "string",
                    Description = "Test model filename",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedBy = "TestUser"
                }
            };

            foreach (var config in configs)
            {
                await UnitOfWork.MachineLearningConfigurations.AddAsync(config);
            }

            await UnitOfWork.SaveChangesAsync();
        }
    }
} 