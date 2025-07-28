using AwesomeAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Features.MachineLearning;
using OpenAlprWebhookProcessor.Features.MachineLearning.Commands.UpsertConfiguration;
using OpenAlprWebhookProcessor.Features.MachineLearning.Queries.GetConfiguration;
using Tests.TestHelpers;

namespace Tests.Features.MachineLearning.UpsertConfiguration
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class UpsertConfigurationCommandHandlerTests : TestBase
    {
        [Test]
        public async Task Handle_WithNewConfiguration_CreatesAllSettings()
        {
            // Arrange
            var handler = new UpsertConfigurationCommandHandler(UnitOfWork);
            var command = new UpsertConfigurationCommand
            {
                Configuration = new MachineLearningConfigDto
                {
                    MinimumModelQuality = 0.02,
                    MinimumTrainingData = 150,
                    TrainingBatchSize = 30000,
                    TrainingInterval = TimeSpan.FromHours(8),
                    ModelFileName = "new-model.zip",
                    ConfigFolderName = "new-config",
                    MlModelsFolderName = "new-models"
                },
                UpdatedBy = "TestUser"
            };
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();

            // Verify data was actually saved
            var allConfigs = await UnitOfWork.MachineLearningConfigurations.GetAllAsync();
            allConfigs.Should().HaveCount(7); // All 7 configuration keys
            allConfigs.Should().Contain(c => c.Key == "MinimumModelQuality" && c.Value == "0.02");
            allConfigs.Should().Contain(c => c.Key == "MinimumTrainingData" && c.Value == "150");
            allConfigs.Should().Contain(c => c.Key == "TrainingBatchSize" && c.Value == "30000");
        }

        [Test]
        public async Task Handle_WithExistingConfiguration_UpdatesSettings()
        {
            // Arrange
            await SeedTestConfigurationAsync();
            var handler = new UpsertConfigurationCommandHandler(UnitOfWork);
            var command = new UpsertConfigurationCommand
            {
                Configuration = new MachineLearningConfigDto
                {
                    MinimumModelQuality = 0.001, // Changed from 0.05
                    MinimumTrainingData = 500,   // Changed from 200
                    TrainingBatchSize = 75000,   // Changed from 25000
                    TrainingInterval = TimeSpan.FromHours(4),  // Changed from 12
                    ModelFileName = "updated-model.zip",       // Changed
                    ConfigFolderName = "config",              // Same
                    MlModelsFolderName = "ml-models"          // Same
                },
                UpdatedBy = "UpdatedUser"
            };
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();

            // Verify updates in database
            var repo = UnitOfWork.MachineLearningConfigurations;
            (await repo.GetDoubleValueAsync("MinimumModelQuality")).Should().Be(0.001);
            (await repo.GetIntValueAsync("MinimumTrainingData")).Should().Be(500);
            (await repo.GetIntValueAsync("TrainingBatchSize")).Should().Be(75000);
        }

        [Test]
        public async Task RoundTripTest_UpsertThenGet_ReturnsCorrectValues()
        {
            // Arrange
            var testConfig = new MachineLearningConfigDto
            {
                MinimumModelQuality = 0.123,
                MinimumTrainingData = 987,
                TrainingBatchSize = 12345,
                TrainingInterval = TimeSpan.FromHours(3.5),
                ModelFileName = "test-round-trip.zip",
                ConfigFolderName = "test-config",
                MlModelsFolderName = "test-models"
            };

            var upsertHandler = new UpsertConfigurationCommandHandler(UnitOfWork);
            var getHandler = new GetConfigurationQueryHandler(UnitOfWork);

            // Act
            var upsertResult = await upsertHandler.Handle(new UpsertConfigurationCommand
            {
                Configuration = testConfig,
                UpdatedBy = "RoundTripTest"
            }, GetCancellationToken());

            var getResult = await getHandler.Handle(new GetConfigurationQuery(), GetCancellationToken());

            // Assert
            upsertResult.Should().NotBeNull();

            getResult.Should().NotBeNull();
            getResult.MinimumModelQuality.Should().Be(0.123);
            getResult.MinimumTrainingData.Should().Be(987);
            getResult.TrainingBatchSize.Should().Be(12345);
            getResult.TrainingInterval.Should().Be(TimeSpan.FromHours(3.5));
            getResult.ModelFileName.Should().Be("test-round-trip.zip");
            getResult.ConfigFolderName.Should().Be("test-config");
            getResult.MlModelsFolderName.Should().Be("test-models");
            getResult.UpdatedBy.Should().Be("RoundTripTest");
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