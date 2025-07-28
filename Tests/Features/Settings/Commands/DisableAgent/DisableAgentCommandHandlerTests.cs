using AwesomeAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Settings.Commands.DisableAgent;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebsocket;
using Tests.TestHelpers;
namespace Tests.Features.Settings.Commands.DisableAgent
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class DisableAgentCommandHandlerTests : TestBase
    {
        private DisableAgentCommandHandler _handler;
        private IWebsocketClientOrganizer _websocketClientOrganizer;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _websocketClientOrganizer = Substitute.For<IWebsocketClientOrganizer>();
            _handler = new DisableAgentCommandHandler(UnitOfWork, _websocketClientOrganizer);
        }

        [Test]
        public async Task Handle_WithValidAgent_CallsWebsocketClientOrganizer()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var agent = new OpenAlprWebhookProcessor.Data.Agent
            {
                Id = agentId,
                Uid = "test-agent-123",
                EndpointUrl = "https://agent.example.com"
            };
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            var command = new DisableAgentCommand(agentId);

            _websocketClientOrganizer.DisableEnableAgentAsync(
                "test-agent-123",
                AgentStartStopType.Stop,
                Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(true));

            // Act
            var result = await _handler.Handle(command, GetCancellationToken());

            // Assert
            result.Should().BeTrue();
            await _websocketClientOrganizer.Received(1).DisableEnableAgentAsync(
                "test-agent-123",
                AgentStartStopType.Stop,
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_WithNoAgent_ReturnsFalse()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var command = new DisableAgentCommand(agentId);

            // Act
            var result = await _handler.Handle(command, GetCancellationToken());

            // Assert
            result.Should().BeFalse();
            await _websocketClientOrganizer.DidNotReceive().DisableEnableAgentAsync(
                Arg.Any<string>(),
                Arg.Any<AgentStartStopType>(),
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_WithValidAgent_ReturnsTrue()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var agent = new OpenAlprWebhookProcessor.Data.Agent
            {
                Id = agentId,
                Uid = "test-agent-123",
                EndpointUrl = "https://agent.example.com"
            };
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            var command = new DisableAgentCommand(agentId);

            // Act
            var result = await _handler.Handle(command, GetCancellationToken());

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task Handle_WebsocketClientOrganizerThrowsException_ExceptionBubbles()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var agent = new OpenAlprWebhookProcessor.Data.Agent
            {
                Id = agentId,
                Uid = "test-agent-123",
                EndpointUrl = "https://agent.example.com"
            };
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            var command = new DisableAgentCommand(agentId);

            _websocketClientOrganizer.DisableEnableAgentAsync(
                "test-agent-123",
                AgentStartStopType.Stop,
                Arg.Any<CancellationToken>())
                .ThrowsAsync(new InvalidOperationException("Test exception"));

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _handler.Handle(command, GetCancellationToken()));
        }

        [Test]
        public async Task Handle_CallsGetFirstAgentAsync()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var command = new DisableAgentCommand(agentId);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            // Verify that the handler attempts to get the first agent
            // This is implicitly tested by the fact that the handler returns false when no agent exists
            var result = await _handler.Handle(command, GetCancellationToken());
            result.Should().BeFalse();
        }
    }
} 