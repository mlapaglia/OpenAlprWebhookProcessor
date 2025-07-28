using AwesomeAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetAgentStatus;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebsocket;
using Tests.TestHelpers;

namespace Tests.Features.Settings.Queries.GetAgentStatus
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class GetAgentStatusQueryHandlerTests : TestBase
    {
        private GetAgentStatusQueryHandler _handler;
        private IWebsocketClientOrganizer _websocketClientOrganizer;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _websocketClientOrganizer = Substitute.For<IWebsocketClientOrganizer>();
            _handler = new GetAgentStatusQueryHandler(UnitOfWork, _websocketClientOrganizer);
        }

        [Test]
        public async Task Handle_WithValidAgentAndStatus_ReturnsCorrectAgentStatusDto()
        {
            // Arrange
            var agent = new OpenAlprWebhookProcessor.Data.Agent
            {
                Id = Guid.NewGuid(),
                Uid = "test-agent-123",
                LastHeartbeatEpochMs = 1640995200000
            };
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            var agentStatusResponse = new AgentStatusResponse
            {
                AgentEpochMs = 1640995260000,
                Version = "1.0.0",
                AgentStatus = new AgentStatus
                {
                    AlprdActive = true,
                    CpuCores = 4,
                    CpuUsagePercent = 25.5m,
                    DaemonUptimeSeconds = 3600,
                    DiskDriveFreeBytes = 1000000000,
                    SystemUptimeSeconds = 86400,
                    AgentHostname = "test-agent-host"
                }
            };

            _websocketClientOrganizer.GetAgentStatusAsync("test-agent-123", Arg.Any<CancellationToken>())
                .Returns(agentStatusResponse);

            var query = new GetAgentStatusQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.IsConnected.Should().BeTrue();
            result.AgentEpochMs.Should().Be(1640995260000);
            result.AlprdActive.Should().BeTrue();
            result.CpuCores.Should().Be(4);
            result.CpuUsagePercent.Should().Be(25.5m);
            result.DaemonUptimeSeconds.Should().Be(3600);
            result.DiskFreeBytes.Should().Be(1000000000);
            result.SystemUptimeSeconds.Should().Be(86400);
            result.Hostname.Should().Be("test-agent-host");
            result.Version.Should().Be("1.0.0");
        }

        [Test]
        public async Task Handle_WithNoAgent_ReturnsDisconnectedStatus()
        {
            // Arrange
            var query = new GetAgentStatusQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.IsConnected.Should().BeFalse();
            result.AgentEpochMs.Should().Be(0);
            result.AlprdActive.Should().BeFalse();
            result.CpuCores.Should().Be(0);
            result.CpuUsagePercent.Should().Be(0);
            result.DaemonUptimeSeconds.Should().Be(0);
            result.DiskFreeBytes.Should().Be(0);
            result.SystemUptimeSeconds.Should().Be(0);
            result.Hostname.Should().BeNull();
            result.Version.Should().BeNull();
            result.LastHeartbeatEpochMs.Should().Be(0);
        }

        [Test]
        public async Task Handle_WithAgentButNullUid_ReturnsDisconnectedStatus()
        {
            // Arrange
            var agent = new OpenAlprWebhookProcessor.Data.Agent
            {
                Id = Guid.NewGuid(),
                Uid = null,
                LastHeartbeatEpochMs = 1640995200000
            };
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetAgentStatusQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.IsConnected.Should().BeFalse();
            await _websocketClientOrganizer.DidNotReceive().GetAgentStatusAsync(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_WithAgentButNullAgentStatus_ReturnsDisconnectedWithHeartbeat()
        {
            // Arrange
            var agent = new OpenAlprWebhookProcessor.Data.Agent
            {
                Id = Guid.NewGuid(),
                Uid = "test-agent-123",
                LastHeartbeatEpochMs = 1640995200000
            };
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            _websocketClientOrganizer.GetAgentStatusAsync("test-agent-123", Arg.Any<CancellationToken>())
                .Returns((AgentStatusResponse)null);

            var query = new GetAgentStatusQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.IsConnected.Should().BeFalse();
            result.LastHeartbeatEpochMs.Should().Be(1640995200000);
            result.AgentEpochMs.Should().Be(0);
            result.AlprdActive.Should().BeFalse();
            result.CpuCores.Should().Be(0);
            result.CpuUsagePercent.Should().Be(0);
            result.DaemonUptimeSeconds.Should().Be(0);
            result.DiskFreeBytes.Should().Be(0);
            result.SystemUptimeSeconds.Should().Be(0);
            result.Hostname.Should().BeNull();
            result.Version.Should().BeNull();
        }

        [Test]
        public async Task Handle_WithValidAgent_CallsWebsocketClientOrganizer()
        {
            // Arrange
            var agent = new OpenAlprWebhookProcessor.Data.Agent
            {
                Id = Guid.NewGuid(),
                Uid = "test-agent-123",
                LastHeartbeatEpochMs = 1640995200000
            };
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetAgentStatusQuery();

            // Act
            await _handler.Handle(query, GetCancellationToken());

            // Assert
            await _websocketClientOrganizer.Received(1).GetAgentStatusAsync(
                "test-agent-123",
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_WebsocketClientOrganizerThrowsException_ExceptionBubbles()
        {
            // Arrange
            var agent = new OpenAlprWebhookProcessor.Data.Agent
            {
                Id = Guid.NewGuid(),
                Uid = "test-agent-123",
                LastHeartbeatEpochMs = 1640995200000
            };
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            _websocketClientOrganizer.GetAgentStatusAsync("test-agent-123", Arg.Any<CancellationToken>())
                .ThrowsAsync(new InvalidOperationException("Test exception"));

            var query = new GetAgentStatusQuery();

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _handler.Handle(query, GetCancellationToken()));
        }

        [Test]
        public async Task Handle_WithAgentStatusHavingZeroValues_ReturnsZeroValues()
        {
            // Arrange
            var agent = new OpenAlprWebhookProcessor.Data.Agent
            {
                Id = Guid.NewGuid(),
                Uid = "test-agent-123",
                LastHeartbeatEpochMs = 1640995200000
            };
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            var agentStatusResponse = new AgentStatusResponse
            {
                AgentEpochMs = 0,
                Version = "",
                AgentStatus = new AgentStatus
                {
                    AlprdActive = false,
                    CpuCores = 0,
                    CpuUsagePercent = 0,
                    DaemonUptimeSeconds = 0,
                    DiskDriveFreeBytes = 0,
                    SystemUptimeSeconds = 0,
                    AgentHostname = ""
                }
            };

            _websocketClientOrganizer.GetAgentStatusAsync("test-agent-123", Arg.Any<CancellationToken>())
                .Returns(agentStatusResponse);

            var query = new GetAgentStatusQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.IsConnected.Should().BeTrue();
            result.AgentEpochMs.Should().Be(0);
            result.AlprdActive.Should().BeFalse();
            result.CpuCores.Should().Be(0);
            result.CpuUsagePercent.Should().Be(0);
            result.DaemonUptimeSeconds.Should().Be(0);
            result.DiskFreeBytes.Should().Be(0);
            result.SystemUptimeSeconds.Should().Be(0);
            result.Hostname.Should().Be("");
            result.Version.Should().Be("");
        }
    }
} 