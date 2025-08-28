using AwesomeAssertions;
using Mediator;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Alerts.Commands.DeleteAlert;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tests.TestHelpers;

namespace Tests.Features.Alerts.Commands.DeleteAlert
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class DeleteAlertCommandHandlerTests : TestBase
    {
        private DeleteAlertCommandHandler _handler;
        private IUnitOfWork _mockUnitOfWork;
        private IRepository<Alert> _mockAlertRepository;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _mockUnitOfWork = Substitute.For<IUnitOfWork>();
            _mockAlertRepository = Substitute.For<IRepository<Alert>>();
            _mockUnitOfWork.Alerts.Returns(_mockAlertRepository);
            _handler = new DeleteAlertCommandHandler(_mockUnitOfWork);
        }

        [TearDown]
        public override void TearDown()
        {
            _mockUnitOfWork?.Dispose();
            base.TearDown();
        }

        [Test]
        public async Task Handle_ValidAlertId_DeletesAlertSuccessfully()
        {
            // Arrange
            var alertId = Guid.NewGuid();
            var alert = new Alert { Id = alertId };
            var command = new DeleteAlertCommand(alertId);
            var cancellationToken = GetCancellationToken();

            _mockAlertRepository.GetByIdAsync(alertId, cancellationToken)
                .Returns(alert);

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().Be(Unit.Value);
            _mockAlertRepository.Received(1).Delete(alert);
            await _mockUnitOfWork.Received(1).SaveChangesAsync(cancellationToken);
        }

        [Test]
        public async Task Handle_NonExistentAlertId_ThrowsInvalidOperationException()
        {
            // Arrange
            var alertId = Guid.NewGuid();
            var command = new DeleteAlertCommand(alertId);
            var cancellationToken = GetCancellationToken();

            _mockAlertRepository.GetByIdAsync(alertId, cancellationToken)
                .Returns((Alert)null);

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<InvalidOperationException>()
                .WithMessage($"Alert with ID {alertId} not found");

            _mockAlertRepository.DidNotReceive().Delete(Arg.Any<Alert>());
            await _mockUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var alertId = Guid.NewGuid();
            var command = new DeleteAlertCommand(alertId);
            var cancellationToken = GetCancellationToken();
            var expectedException = new Exception("Database error");

            _mockAlertRepository.GetByIdAsync(alertId, cancellationToken)
                .ThrowsAsync(expectedException);

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<Exception>()
                .WithMessage("Database error");
        }

        [Test]
        public async Task Handle_SaveChangesThrowsException_PropagatesException()
        {
            // Arrange
            var alertId = Guid.NewGuid();
            var alert = new Alert { Id = alertId };
            var command = new DeleteAlertCommand(alertId);
            var cancellationToken = GetCancellationToken();
            var expectedException = new Exception("Save failed");

            _mockAlertRepository.GetByIdAsync(alertId, cancellationToken)
                .Returns(alert);
            _mockUnitOfWork.SaveChangesAsync(cancellationToken)
                .ThrowsAsync(expectedException);

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<Exception>()
                .WithMessage("Save failed");

            _mockAlertRepository.Received(1).Delete(alert);
        }
    }
}
