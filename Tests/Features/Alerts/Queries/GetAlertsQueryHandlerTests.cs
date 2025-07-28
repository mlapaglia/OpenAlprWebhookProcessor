using AwesomeAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Alerts.Queries.GetAlerts;
using Tests.TestHelpers;

namespace Tests.Features.Alerts.Queries
{
    [TestFixture]
    public class GetAlertsQueryHandlerTests : TestBase
    {
        private GetAlertsQueryHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new GetAlertsQueryHandler(UnitOfWork);
        }

        [Test]
        public async Task Handle_NoAlerts_ReturnsEmptyList()
        {
            // Arrange
            var query = new GetAlertsQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        public async Task Handle_WithAlerts_ReturnsAllAlerts()
        {
            // Arrange
            var alert1 = TestDataFactory.CreateTestDbAlert("ALERT1", "Description 1");
            var alert2 = TestDataFactory.CreateTestDbAlert("ALERT2", "Description 2");
            var alert3 = TestDataFactory.CreateTestDbAlert("ALERT3", "Description 3", strictMatch: true);
            
            await UnitOfWork.Alerts.AddAsync(alert1);
            await UnitOfWork.Alerts.AddAsync(alert2);
            await UnitOfWork.Alerts.AddAsync(alert3);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetAlertsQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().HaveCount(3);
            
            var alertPlateNumbers = result.Select(a => a.PlateNumber).ToList();
            alertPlateNumbers.Should().Contain("ALERT1");
            alertPlateNumbers.Should().Contain("ALERT2");
            alertPlateNumbers.Should().Contain("ALERT3");
        }

        [Test]
        public async Task Handle_WithAlerts_ReturnsCorrectAlertData()
        {
            // Arrange
            var alert = TestDataFactory.CreateTestDbAlert("TEST123", "Test Description", strictMatch: true);
            await UnitOfWork.Alerts.AddAsync(alert);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetAlertsQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().HaveCount(1);
            
            var returnedAlert = result.First();
            returnedAlert.Id.Should().Be(alert.Id);
            returnedAlert.PlateNumber.Should().Be("TEST123");
            returnedAlert.Description.Should().Be("Test Description");
            returnedAlert.StrictMatch.Should().BeTrue();
        }

        [Test]
        public async Task Handle_MixedStrictMatchFlags_ReturnsCorrectFlags()
        {
            // Arrange
            var strictAlert = TestDataFactory.CreateTestDbAlert("STRICT", "Strict Alert", strictMatch: true);
            var nonStrictAlert = TestDataFactory.CreateTestDbAlert("NONSTRICT", "Non-Strict Alert", strictMatch: false);
            
            await UnitOfWork.Alerts.AddAsync(strictAlert);
            await UnitOfWork.Alerts.AddAsync(nonStrictAlert);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetAlertsQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().HaveCount(2);
            
            var strictResult = result.First(a => a.PlateNumber == "STRICT");
            var nonStrictResult = result.First(a => a.PlateNumber == "NONSTRICT");
            
            strictResult.StrictMatch.Should().BeTrue();
            nonStrictResult.StrictMatch.Should().BeFalse();
        }

        [Test]
        public async Task Handle_MultipleCallsWithSameData_ReturnsConsistentResults()
        {
            // Arrange
            var alert1 = TestDataFactory.CreateTestDbAlert("ALERT1", "Description 1");
            var alert2 = TestDataFactory.CreateTestDbAlert("ALERT2", "Description 2");
            
            await UnitOfWork.Alerts.AddAsync(alert1);
            await UnitOfWork.Alerts.AddAsync(alert2);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetAlertsQuery();

            // Act
            var result1 = await _handler.Handle(query, GetCancellationToken());
            var result2 = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result1.Should().HaveCount(2);
            result2.Should().HaveCount(2);
            
            result1.Select(a => a.PlateNumber).Should().BeEquivalentTo(result2.Select(a => a.PlateNumber));
        }
    }
} 