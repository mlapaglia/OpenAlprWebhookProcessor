using AwesomeAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Alerts;
using OpenAlprWebhookProcessor.Features.Alerts.Commands.UpsertAlerts;
using Tests.TestHelpers;

namespace Tests.Features.Alerts.Commands
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class UpsertAlertsCommandHandlerTests : TestBase
    {
        private UpsertAlertsCommandHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new UpsertAlertsCommandHandler(UnitOfWork);
        }

        [Test]
        public async Task Handle_NewAlerts_AddsAlertsToDatabase()
        {
            // Arrange
            var alerts = new List<Alert>
            {
                TestDataFactory.CreateTestAlert("ALERT1", "Description 1"),
                TestDataFactory.CreateTestAlert("ALERT2", "Description 2")
            };
            var command = new UpsertAlertsCommand(alerts);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbAlerts = await UnitOfWork.Alerts.GetAllAsync();
            dbAlerts.Should().HaveCount(2);
            
            var plateNumbers = dbAlerts.Select(a => a.PlateNumber).ToList();
            plateNumbers.Should().Contain("ALERT1");
            plateNumbers.Should().Contain("ALERT2");
        }

        [Test]
        public async Task Handle_UpdateExistingAlert_UpdatesAlertInDatabase()
        {
            // Arrange
            var existingAlert = TestDataFactory.CreateTestDbAlert("TEST123", "Original Description");
            await UnitOfWork.Alerts.AddAsync(existingAlert);
            await UnitOfWork.SaveChangesAsync();

            var updatedAlert = TestDataFactory.CreateTestAlert("TEST123", "Updated Description", strictMatch: true);
            updatedAlert.Id = existingAlert.Id;
            
            var command = new UpsertAlertsCommand(new List<Alert> { updatedAlert });

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbAlerts = await UnitOfWork.Alerts.GetAllAsync();
            dbAlerts.Should().HaveCount(1);
            
            var dbAlert = dbAlerts.First();
            dbAlert.PlateNumber.Should().Be("TEST123");
            dbAlert.Description.Should().Be("Updated Description");
            dbAlert.IsStrictMatch.Should().BeTrue();
        }

        [Test]
        public async Task Handle_RemoveAlert_DeletesAlertFromDatabase()
        {
            // Arrange
            var existingAlert1 = TestDataFactory.CreateTestDbAlert("ALERT1", "Description 1");
            var existingAlert2 = TestDataFactory.CreateTestDbAlert("ALERT2", "Description 2");
            await UnitOfWork.Alerts.AddAsync(existingAlert1);
            await UnitOfWork.Alerts.AddAsync(existingAlert2);
            await UnitOfWork.SaveChangesAsync();

            // Only include one alert in the upsert command
            var remainingAlert = TestDataFactory.CreateTestAlert("ALERT1", "Description 1");
            remainingAlert.Id = existingAlert1.Id;
            
            var command = new UpsertAlertsCommand(new List<Alert> { remainingAlert });

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbAlerts = await UnitOfWork.Alerts.GetAllAsync();
            dbAlerts.Should().HaveCount(1);
            dbAlerts.First().PlateNumber.Should().Be("ALERT1");
        }

        [Test]
        public async Task Handle_MixedOperations_HandlesAddUpdateDelete()
        {
            // Arrange
            var existingAlert1 = TestDataFactory.CreateTestDbAlert("EXISTING1", "Original Description");
            var existingAlert2 = TestDataFactory.CreateTestDbAlert("EXISTING2", "Description 2");
            await UnitOfWork.Alerts.AddAsync(existingAlert1);
            await UnitOfWork.Alerts.AddAsync(existingAlert2);
            await UnitOfWork.SaveChangesAsync();

            var alerts = new List<Alert>
            {
                // Update existing alert
                TestDataFactory.CreateTestAlert("EXISTING1", "Updated Description", strictMatch: true),
                // Add new alert
                TestDataFactory.CreateTestAlert("NEW1", "New Description")
                // existingAlert2 is not included, so it will be deleted
            };
            alerts[0].Id = existingAlert1.Id;
            
            var command = new UpsertAlertsCommand(alerts);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbAlerts = await UnitOfWork.Alerts.GetAllAsync();
            dbAlerts.Should().HaveCount(2);
            
            var plateNumbers = dbAlerts.Select(a => a.PlateNumber).ToList();
            plateNumbers.Should().Contain("EXISTING1");
            plateNumbers.Should().Contain("NEW1");
            plateNumbers.Should().NotContain("EXISTING2");

            var updatedAlert = dbAlerts.First(a => a.PlateNumber == "EXISTING1");
            updatedAlert.Description.Should().Be("Updated Description");
            updatedAlert.IsStrictMatch.Should().BeTrue();
        }

        [Test]
        public async Task Handle_EmptyPlateNumber_FiltersOutAlert()
        {
            // Arrange
            var alerts = new List<Alert>
            {
                TestDataFactory.CreateTestAlertWithPlateNumber("VALID123", "Valid Description"),
                TestDataFactory.CreateTestAlertWithPlateNumber("", "Empty Plate Number"),
                TestDataFactory.CreateTestAlertWithPlateNumber(null, "Null Plate Number")
            };
            var command = new UpsertAlertsCommand(alerts);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbAlerts = await UnitOfWork.Alerts.GetAllAsync();
            dbAlerts.Should().HaveCount(1);
            dbAlerts.First().PlateNumber.Should().Be("VALID123");
        }

        [Test]
        public async Task Handle_PlateNumberCaseConversion_SavesUpperCase()
        {
            // Arrange
            var alerts = new List<Alert>
            {
                TestDataFactory.CreateTestAlert("test123", "Test Description")
            };
            var command = new UpsertAlertsCommand(alerts);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbAlerts = await UnitOfWork.Alerts.GetAllAsync();
            dbAlerts.First().PlateNumber.Should().Be("TEST123");
        }

        [Test]
        public async Task Handle_EmptyList_ClearsAllAlerts()
        {
            // Arrange
            var existingAlert1 = TestDataFactory.CreateTestDbAlert("ALERT1", "Description 1");
            var existingAlert2 = TestDataFactory.CreateTestDbAlert("ALERT2", "Description 2");
            await UnitOfWork.Alerts.AddAsync(existingAlert1);
            await UnitOfWork.Alerts.AddAsync(existingAlert2);
            await UnitOfWork.SaveChangesAsync();

            var command = new UpsertAlertsCommand(new List<Alert>());

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbAlerts = await UnitOfWork.Alerts.GetAllAsync();
            dbAlerts.Should().BeEmpty();
        }
    }
} 