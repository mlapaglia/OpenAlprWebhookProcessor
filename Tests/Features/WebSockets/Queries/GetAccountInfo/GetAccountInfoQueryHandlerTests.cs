using AwesomeAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.WebSockets.Queries.GetAccountInfo;
using Tests.TestHelpers;

namespace Tests.Features.WebSockets.Queries.GetAccountInfo
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class GetAccountInfoQueryHandlerTests : TestBase
    {
        private GetAccountInfoQueryHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new GetAccountInfoQueryHandler(UnitOfWork);
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public async Task Handle_NoAgentsExist_ReturnsEmptyAccountInfoResponse()
        {
            // Arrange
            var query = new GetAccountInfoQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.WebsocketsUrl.Should().BeNull();
        }

        [Test]
        public async Task Handle_AgentExists_ReturnsWebsocketUrl()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgent();
            agent.OpenAlprWebServerUrl = "https://openalpr.example.com";

            Context.Agents.Add(agent);
            await Context.SaveChangesAsync();

            var query = new GetAccountInfoQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.WebsocketsUrl.Should().Be("wss://openalpr.example.com/ws");
        }

        [Test]
        public async Task Handle_MultipleAgentsExist_ReturnsOneOfTheAgentWebsocketUrls()
        {
            // Arrange
            var firstAgent = TestDataFactory.CreateTestAgent();
            firstAgent.OpenAlprWebServerUrl = "https://first.example.com";

            var secondAgent = TestDataFactory.CreateTestAgent();
            secondAgent.OpenAlprWebServerUrl = "https://second.example.com";

            Context.Agents.AddRange(firstAgent, secondAgent);
            await Context.SaveChangesAsync();

            var query = new GetAccountInfoQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            // GetFirstAsync() doesn't guarantee order, so we should accept either agent's URL
            result.WebsocketsUrl.Should().BeOneOf("wss://first.example.com/ws", "wss://second.example.com/ws");
        }

        [Test]
        public async Task Handle_AgentWithHttpUrl_DoesNotConvertToWss()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgent();
            agent.OpenAlprWebServerUrl = "http://openalpr.example.com";

            Context.Agents.Add(agent);
            await Context.SaveChangesAsync();

            var query = new GetAccountInfoQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.WebsocketsUrl.Should().Be("http://openalpr.example.com/ws");
        }

        [Test]
        public async Task Handle_AgentWithNullUrl_ThrowsNullReferenceException()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgent();
            agent.OpenAlprWebServerUrl = null;

            Context.Agents.Add(agent);
            await Context.SaveChangesAsync();

            var query = new GetAccountInfoQuery();
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            var exception = Assert.ThrowsAsync<NullReferenceException>(() => 
                _handler.Handle(query, cancellationToken).AsTask());
            
            exception.Should().NotBeNull();
        }
    }
}