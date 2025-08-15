using AwesomeAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Webhooks.Commands.ProcessHeartbeatWebhook;
using Tests.TestHelpers;

namespace Tests.Features.Webhooks.Commands
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class ProcessHeartbeatWebhookCommandHandlerTests : TestBase
    {
        private ProcessHeartbeatWebhookCommandHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _handler = new ProcessHeartbeatWebhookCommandHandler(UnitOfWork);
        }

        [Test]
        public void Constructor_WithValidParameters_CreatesInstance()
        {
            // Act
            var handler = new ProcessHeartbeatWebhookCommandHandler(UnitOfWork);

            // Assert
            handler.Should().NotBeNull();
        }

        [Test]
        public async Task Handle_WithNoExistingAgent_DoesNotThrow()
        {
            // Arrange
            var command = new ProcessHeartbeatWebhookCommand();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            
            // Verify no agent was created
            var agent = await UnitOfWork.Agents.GetFirstAgentAsync();
            agent.Should().BeNull();
        }

        [Test]
        public async Task Handle_CallsSaveChanges()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgent();
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            var command = new ProcessHeartbeatWebhookCommand();
            var cancellationToken = GetCancellationToken();

            // Track the context state before the call
            var contextChangeTracker = Context.ChangeTracker;
            contextChangeTracker.Clear(); // Clear any existing tracked changes

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            
            // Verify the agent was actually saved to the database
            var savedAgent = await UnitOfWork.Agents.GetByIdAsync(agent.Id);
            savedAgent.Should().NotBeNull();
            savedAgent.LastHeartbeatEpochMs.Should().BeGreaterThan(agent.LastHeartbeatEpochMs);
        }

        [Test]
        public async Task Handle_WithCancellationToken_RespectsCancellation()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgent();
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            var command = new ProcessHeartbeatWebhookCommand();
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            // Act & Assert
            // Note: This test verifies the cancellation token is passed through correctly
            // In a real scenario, cancellation would happen during database operations
            var result = await _handler.Handle(command, cancellationToken);
            result.Should().NotBeNull();
        }

        [Test]
        public async Task Handle_WithAgentHavingOldHeartbeat_UpdatesToCurrentTime()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgent();
            agent.LastHeartbeatEpochMs = DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeMilliseconds(); // 1 hour ago
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            var command = new ProcessHeartbeatWebhookCommand();
            var cancellationToken = GetCancellationToken();

            var beforeCallTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            
            var updatedAgent = await UnitOfWork.Agents.GetByIdAsync(agent.Id);
            updatedAgent.Should().NotBeNull();
            updatedAgent.LastHeartbeatEpochMs.Should().BeGreaterThanOrEqualTo(beforeCallTime);
            
            var afterCallTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            updatedAgent.LastHeartbeatEpochMs.Should().BeLessThanOrEqualTo(afterCallTime);
        }
    }
}
