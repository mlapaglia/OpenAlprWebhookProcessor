using AwesomeAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Alerts.Commands.AddAlert;
using Tests.TestHelpers;

namespace Tests.Features.Alerts.Commands
{
    [TestFixture]
    public class AddAlertCommandHandlerTests : TestBase
    {
        private AddAlertCommandHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new AddAlertCommandHandler(UnitOfWork);
        }

        [Test]
        public async Task Handle_ValidAlert_AddsAlertToDatabase()
        {
            // Arrange
            var alert = TestDataFactory.CreateTestAlert("TEST123", "Test Description");
            var command = new AddAlertCommand(alert);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbAlerts = await UnitOfWork.Alerts.GetAllAsync();
            dbAlerts.Should().HaveCount(1);
            
            var dbAlert = dbAlerts.First();
            dbAlert.PlateNumber.Should().Be("TEST123");
            dbAlert.Description.Should().Be("Test Description");
            dbAlert.IsStrictMatch.Should().BeFalse();
        }

        [Test]
        public async Task Handle_PlateNumberToUpperCase_SavesUpperCaseNumber()
        {
            // Arrange
            var alert = TestDataFactory.CreateTestAlert("test123", "Test Description");
            var command = new AddAlertCommand(alert);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbAlerts = await UnitOfWork.Alerts.GetAllAsync();
            var dbAlert = dbAlerts.First();
            dbAlert.PlateNumber.Should().Be("TEST123");
        }

        [Test]
        public async Task Handle_StrictMatchAlert_SavesStrictMatchFlag()
        {
            // Arrange
            var alert = TestDataFactory.CreateTestAlert("TEST123", "Test Description", strictMatch: true);
            var command = new AddAlertCommand(alert);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbAlerts = await UnitOfWork.Alerts.GetAllAsync();
            var dbAlert = dbAlerts.First();
            dbAlert.IsStrictMatch.Should().BeTrue();
        }

        [Test]
        public async Task Handle_DuplicateAlert_ThrowsArgumentException()
        {
            // Arrange
            var existingAlert = TestDataFactory.CreateTestDbAlert("TEST123", "Existing Alert");
            await UnitOfWork.Alerts.AddAsync(existingAlert);
            await UnitOfWork.SaveChangesAsync();

            var newAlert = TestDataFactory.CreateTestAlert("TEST123", "New Alert");
            var command = new AddAlertCommand(newAlert);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _handler.Handle(command, GetCancellationToken()));
            
            exception.Message.Should().Be("alert already exists");
        }

        [Test]
        public async Task Handle_DuplicateAlertDifferentCase_ThrowsArgumentException()
        {
            // Arrange
            var existingAlert = TestDataFactory.CreateTestDbAlert("TEST123", "Existing Alert");
            await UnitOfWork.Alerts.AddAsync(existingAlert);
            await UnitOfWork.SaveChangesAsync();

            var newAlert = TestDataFactory.CreateTestAlert("test123", "New Alert");
            var command = new AddAlertCommand(newAlert);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _handler.Handle(command, GetCancellationToken()));

            exception.Message.Should().Be("alert already exists");
        }

        [Test]
        public async Task Handle_MultipleUniqueAlerts_AddsAllAlerts()
        {
            // Arrange
            var alert1 = TestDataFactory.CreateTestAlert("ALERT1", "Description 1");
            var alert2 = TestDataFactory.CreateTestAlert("ALERT2", "Description 2");
            
            var command1 = new AddAlertCommand(alert1);
            var command2 = new AddAlertCommand(alert2);

            // Act
            await _handler.Handle(command1, GetCancellationToken());
            await _handler.Handle(command2, GetCancellationToken());

            // Assert
            var dbAlerts = await UnitOfWork.Alerts.GetAllAsync();
            dbAlerts.Should().HaveCount(2);
            
            var plateNumbers = dbAlerts.Select(a => a.PlateNumber).ToList();
            plateNumbers.Should().Contain("ALERT1");
            plateNumbers.Should().Contain("ALERT2");
        }
    }
} 