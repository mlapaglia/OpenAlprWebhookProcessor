using AwesomeAssertions;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Settings.Queries.GetAgentVideoStreams;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebsocket;
using Tests.TestHelpers;

namespace Tests.Features.Settings.Queries.GetAgentVideoStreams
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class GetAgentVideoStreamsQueryHandlerTests : TestBase
    {
        private GetAgentVideoStreamsQueryHandler _handler;
        private IWebsocketClientOrganizer _websocketClientOrganizer;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _websocketClientOrganizer = Substitute.For<IWebsocketClientOrganizer>();
            _handler = new GetAgentVideoStreamsQueryHandler(UnitOfWork, _websocketClientOrganizer);
        }

        [Test]
        public async Task Handle_WithValidAgentAndVideoStreams_ReturnsCorrectDto()
        {
            var agent = new OpenAlprWebhookProcessor.Data.Agent
            {
                Id = Guid.NewGuid(),
                Uid = "test-agent-123"
            };
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            var videoStreams = new List<VideoStream>
            {
                new VideoStream
                {
                    CameraId = 1,
                    CameraName = "Front Door",
                    Fps = 15.0m,
                    IsStreaming = true,
                    LastPlateRead = 1640995200000,
                    LastUpdate = 1640995260000,
                    TotalPlateReads = 50,
                    Url = "rtsp://camera1/stream"
                },
                new VideoStream
                {
                    CameraId = 2,
                    CameraName = "Back Gate",
                    Fps = 10.0m,
                    IsStreaming = false,
                    LastPlateRead = 1640990000000,
                    LastUpdate = 1640995000000,
                    TotalPlateReads = 25,
                    Url = "rtsp://camera2/stream"
                }
            };

            var agentStatusResponse = new AgentStatusResponse
            {
                AgentEpochMs = 1640995260000,
                Version = "1.0.0",
                AgentStatus = new AgentStatus
                {
                    VideoStreams = videoStreams
                }
            };

            _websocketClientOrganizer.GetAgentStatusAsync("test-agent-123", Arg.Any<System.Threading.CancellationToken>())
                .Returns(agentStatusResponse);

            var query = new GetAgentVideoStreamsQuery();

            var result = await _handler.Handle(query, GetCancellationToken());

            result.Should().NotBeNull();
            result.IsConnected.Should().BeTrue();
            result.VideoStreams.Should().HaveCount(2);
            
            var firstStream = result.VideoStreams[0];
            firstStream.CameraId.Should().Be(1);
            firstStream.CameraName.Should().Be("Front Door");
            firstStream.Fps.Should().Be(15.0m);
            firstStream.IsStreaming.Should().BeTrue();
            firstStream.LastPlateRead.Should().Be(1640995200000);
            firstStream.LastUpdate.Should().Be(1640995260000);
            firstStream.TotalPlateReads.Should().Be(50);
            firstStream.Url.Should().Be("rtsp://camera1/stream");

            var secondStream = result.VideoStreams[1];
            secondStream.CameraId.Should().Be(2);
            secondStream.CameraName.Should().Be("Back Gate");
            secondStream.Fps.Should().Be(10.0m);
            secondStream.IsStreaming.Should().BeFalse();
            secondStream.LastPlateRead.Should().Be(1640990000000);
            secondStream.LastUpdate.Should().Be(1640995000000);
            secondStream.TotalPlateReads.Should().Be(25);
            secondStream.Url.Should().Be("rtsp://camera2/stream");
        }

        [Test]
        public async Task Handle_WithNoAgent_ReturnsDisconnectedStatus()
        {
            var query = new GetAgentVideoStreamsQuery();

            var result = await _handler.Handle(query, GetCancellationToken());

            result.Should().NotBeNull();
            result.IsConnected.Should().BeFalse();
            result.VideoStreams.Should().BeEmpty();
        }

        [Test]
        public async Task Handle_WithAgentButNullUid_ReturnsDisconnectedStatus()
        {
            var agent = new OpenAlprWebhookProcessor.Data.Agent
            {
                Id = Guid.NewGuid(),
                Uid = null
            };
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            var query = new GetAgentVideoStreamsQuery();

            var result = await _handler.Handle(query, GetCancellationToken());

            result.Should().NotBeNull();
            result.IsConnected.Should().BeFalse();
            result.VideoStreams.Should().BeEmpty();
        }

        [Test]
        public async Task Handle_WithAgentButNoWebsocketConnection_ReturnsDisconnectedStatus()
        {
            var agent = new OpenAlprWebhookProcessor.Data.Agent
            {
                Id = Guid.NewGuid(),
                Uid = "test-agent-123"
            };
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            _websocketClientOrganizer.GetAgentStatusAsync("test-agent-123", Arg.Any<System.Threading.CancellationToken>())
                .Returns((AgentStatusResponse)null);

            var query = new GetAgentVideoStreamsQuery();

            var result = await _handler.Handle(query, GetCancellationToken());

            result.Should().NotBeNull();
            result.IsConnected.Should().BeFalse();
            result.VideoStreams.Should().BeEmpty();
        }

        [Test]
        public async Task Handle_WithNullVideoStreams_ReturnsEmptyList()
        {
            var agent = new OpenAlprWebhookProcessor.Data.Agent
            {
                Id = Guid.NewGuid(),
                Uid = "test-agent-123"
            };
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            var agentStatusResponse = new AgentStatusResponse
            {
                AgentEpochMs = 1640995260000,
                Version = "1.0.0",
                AgentStatus = new AgentStatus
                {
                    VideoStreams = null
                }
            };

            _websocketClientOrganizer.GetAgentStatusAsync("test-agent-123", Arg.Any<System.Threading.CancellationToken>())
                .Returns(agentStatusResponse);

            var query = new GetAgentVideoStreamsQuery();

            var result = await _handler.Handle(query, GetCancellationToken());

            result.Should().NotBeNull();
            result.IsConnected.Should().BeTrue();
            result.VideoStreams.Should().BeEmpty();
        }
    }
}