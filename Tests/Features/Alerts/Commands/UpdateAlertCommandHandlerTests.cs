using AwesomeAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Alerts.Commands.UpdateAlert;
using System;
using System.Threading.Tasks;
using Tests.TestHelpers;

namespace Tests.Features.Alerts.Commands
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class UpdateAlertCommandHandlerTests : TestBase
    {
        private UpdateAlertCommandHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new UpdateAlertCommandHandler(UnitOfWork);
        }

        [Test]
        public async Task Handle_ValidAlert_UpdatesAlertInDatabase()
        {
            // Arrange
            var existingDbAlert = TestDataFactory.CreateTestDbAlert("ORIGINAL123", "Original Description", false);
            await UnitOfWork.Alerts.AddAsync(existingDbAlert);
            await UnitOfWork.SaveChangesAsync();

            var updatedAlert = TestDataFactory.CreateTestAlert("UPDATED456", "Updated Description", true);
            updatedAlert.Id = existingDbAlert.Id;
            
            var command = new UpdateAlertCommand(updatedAlert);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbAlert = await UnitOfWork.Alerts.GetByIdAsync(existingDbAlert.Id);
            dbAlert.Should().NotBeNull();
            dbAlert.PlateNumber.Should().Be("UPDATED456");
            dbAlert.Description.Should().Be("Updated Description");
            dbAlert.IsStrictMatch.Should().BeTrue();
        }

        [Test]
        public async Task Handle_PlateNumberToUpperCase_SavesUpperCaseNumber()
        {
            // Arrange
            var existingDbAlert = TestDataFactory.CreateTestDbAlert("ORIGINAL123", "Original Description");
            await UnitOfWork.Alerts.AddAsync(existingDbAlert);
            await UnitOfWork.SaveChangesAsync();

            var updatedAlert = TestDataFactory.CreateTestAlert("updated456", "Updated Description");
            updatedAlert.Id = existingDbAlert.Id;
            
            var command = new UpdateAlertCommand(updatedAlert);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbAlert = await UnitOfWork.Alerts.GetByIdAsync(existingDbAlert.Id);
            dbAlert.PlateNumber.Should().Be("UPDATED456");
        }

        [Test]
        public async Task Handle_StrictMatchUpdate_UpdatesStrictMatchFlag()
        {
            // Arrange
            var existingDbAlert = TestDataFactory.CreateTestDbAlert("TEST123", "Description", false);
            await UnitOfWork.Alerts.AddAsync(existingDbAlert);
            await UnitOfWork.SaveChangesAsync();

            var updatedAlert = TestDataFactory.CreateTestAlert("TEST123", "Description", true);
            updatedAlert.Id = existingDbAlert.Id;
            
            var command = new UpdateAlertCommand(updatedAlert);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbAlert = await UnitOfWork.Alerts.GetByIdAsync(existingDbAlert.Id);
            dbAlert.IsStrictMatch.Should().BeTrue();
        }

        [Test]
        public void Handle_NonExistentAlert_ThrowsInvalidOperationException()
        {
            // Arrange
            var nonExistentAlert = TestDataFactory.CreateTestAlert("TEST123", "Description");
            var command = new UpdateAlertCommand(nonExistentAlert);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _handler.Handle(command, GetCancellationToken()));

            exception.Message.Should().Be($"Alert with ID {nonExistentAlert.Id} not found");
        }

        [Test]
        public async Task Handle_UpdatesOnlySpecifiedAlert_LeavesOtherAlertsUnchanged()
        {
            // Arrange
            var alert1 = TestDataFactory.CreateTestDbAlert("ALERT1", "Description 1", false);
            var alert2 = TestDataFactory.CreateTestDbAlert("ALERT2", "Description 2", false);
            await UnitOfWork.Alerts.AddAsync(alert1);
            await UnitOfWork.Alerts.AddAsync(alert2);
            await UnitOfWork.SaveChangesAsync();

            var updatedAlert = TestDataFactory.CreateTestAlert("UPDATED", "Updated Description", true);
            updatedAlert.Id = alert1.Id;
            
            var command = new UpdateAlertCommand(updatedAlert);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbAlert1 = await UnitOfWork.Alerts.GetByIdAsync(alert1.Id);
            var dbAlert2 = await UnitOfWork.Alerts.GetByIdAsync(alert2.Id);
            
            dbAlert1.PlateNumber.Should().Be("UPDATED");
            dbAlert1.Description.Should().Be("Updated Description");
            dbAlert1.IsStrictMatch.Should().BeTrue();
            
            dbAlert2.PlateNumber.Should().Be("ALERT2");
            dbAlert2.Description.Should().Be("Description 2");
            dbAlert2.IsStrictMatch.Should().BeFalse();
        }

        [Test]
        public async Task Handle_AllFieldsUpdate_UpdatesAllFields()
        {
            // Arrange
            var existingDbAlert = TestDataFactory.CreateTestDbAlert("OLD123", "Old Description", false);
            await UnitOfWork.Alerts.AddAsync(existingDbAlert);
            await UnitOfWork.SaveChangesAsync();

            var updatedAlert = TestDataFactory.CreateTestAlert("new456", "New Description", true);
            updatedAlert.Id = existingDbAlert.Id;
            
            var command = new UpdateAlertCommand(updatedAlert);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var dbAlert = await UnitOfWork.Alerts.GetByIdAsync(existingDbAlert.Id);
            dbAlert.Should().NotBeNull();
            dbAlert.PlateNumber.Should().Be("NEW456");
            dbAlert.Description.Should().Be("New Description");
            dbAlert.IsStrictMatch.Should().BeTrue();
        }
    }
}
