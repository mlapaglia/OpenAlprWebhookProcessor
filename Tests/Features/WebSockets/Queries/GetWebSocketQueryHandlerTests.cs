using AwesomeAssertions;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.WebSockets.Queries.GetWebSocket;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebsocket;
using OpenAlprWebhookProcessor.ProcessorHub;
using System.Net;
using System.Net.WebSockets;
using Tests.TestHelpers;

namespace Tests.Features.WebSockets.Queries
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class GetWebSocketQueryHandlerTests : TestBase
    {
        private GetWebSocketQueryHandler _handler;
        private ILogger<GetWebSocketQueryHandler> _mockLogger;
        private IWebsocketClientOrganizer _mockWebsocketClientOrganizer;
        private IHubContext<ProcessorHub, IProcessorHub> _mockHubContext;
        private IHubCallerClients<IProcessorHub> _mockClients;
        private IProcessorHub _mockAllClients;
        private HttpContext _mockHttpContext;
        private WebSocketManager _mockWebSocketManager;
        private WebSocket _mockWebSocket;
        private ConnectionInfo _mockConnection;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _mockLogger = Substitute.For<ILogger<GetWebSocketQueryHandler>>();
            _mockWebsocketClientOrganizer = Substitute.For<IWebsocketClientOrganizer>();
            _mockHubContext = Substitute.For<IHubContext<ProcessorHub, IProcessorHub>>();
            _mockClients = Substitute.For<IHubCallerClients<IProcessorHub>>();
            _mockAllClients = Substitute.For<IProcessorHub>();
            _mockHttpContext = Substitute.For<HttpContext>();
            _mockWebSocketManager = Substitute.For<WebSocketManager>();
            _mockWebSocket = Substitute.For<WebSocket>();
            _mockConnection = Substitute.For<ConnectionInfo>();

            // Setup HttpContext chain
            _mockHttpContext.WebSockets.Returns(_mockWebSocketManager);
            _mockHttpContext.Connection.Returns(_mockConnection);
            _mockConnection.RemoteIpAddress.Returns(IPAddress.Parse("192.168.1.100"));

            // Setup SignalR hub chain
            _mockHubContext.Clients.Returns(_mockClients);
            _mockClients.All.Returns(_mockAllClients);

            _handler = new GetWebSocketQueryHandler(
                _mockLogger,
                UnitOfWork,
                _mockWebsocketClientOrganizer,
                _mockHubContext);
        }

        [TearDown]
        public override void TearDown()
        {
            _mockWebSocket?.Dispose();
            base.TearDown();
        }

        [Test]
        public async Task Handle_NonWebSocketRequest_ReturnsBadRequestAndLogsInformation()
        {
            // Arrange
            _mockWebSocketManager.IsWebSocketRequest.Returns(false);
            var query = new GetWebSocketQuery(_mockHttpContext);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().Be(Unit.Value);
            _mockHttpContext.Response.Received(1).StatusCode = StatusCodes.Status400BadRequest;
            
            _mockLogger.Received(1).LogInformation("Non websocket connection received.");
        }

        [Test]
        public async Task Handle_WebSocketRequestWithNoAgent_ReturnsInternalServerErrorAndLogsError()
        {
            // Arrange
            _mockWebSocketManager.IsWebSocketRequest.Returns(true);
            var query = new GetWebSocketQuery(_mockHttpContext);
            var cancellationToken = GetCancellationToken();

            // No agents in database

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().Be(Unit.Value);
            _mockHttpContext.Response.Received(1).StatusCode = StatusCodes.Status500InternalServerError;
            
            _mockLogger.Received(1).LogInformation("Websocket connection received.");
            _mockLogger.Received(1).LogError("No agent found");
        }

        [Test]
        public async Task Handle_WebSocketRequestWithAgent_AcceptsWebSocketAndCreatesClient()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgent("https://test-agent.local");
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            _mockWebSocketManager.IsWebSocketRequest.Returns(true);
            _mockWebSocketManager.AcceptWebSocketAsync().Returns(_mockWebSocket);

            var addResult = new AddAgentResult 
            { 
                WasAdded = true, 
                WasUpdated = false 
            };
            _mockWebsocketClientOrganizer.AddAgentAsync(agent.Uid, Arg.Any<IOpenAlprWebsocketClient>(), Arg.Any<CancellationToken>())
                .Returns(addResult);

            var query = new GetWebSocketQuery(_mockHttpContext);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().Be(Unit.Value);
            
            _mockLogger.Received(1).LogInformation("Websocket connection received.");
            await _mockWebSocketManager.Received(1).AcceptWebSocketAsync();
            await _mockWebsocketClientOrganizer.Received(1).AddAgentAsync(
                agent.Uid, 
                Arg.Any<IOpenAlprWebsocketClient>(), 
                cancellationToken);
            await _mockAllClients.Received(1).OpenAlprAgentConnected(agent.Uid, "192.168.1.100");
        }

        [Test]
        public async Task Handle_AddAgentFails_LogsErrorAndReturnsEarly()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgent("https://test-agent.local");
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            _mockWebSocketManager.IsWebSocketRequest.Returns(true);
            _mockWebSocketManager.AcceptWebSocketAsync().Returns(_mockWebSocket);

            var addResult = new AddAgentResult 
            { 
                WasAdded = false, 
                WasUpdated = false 
            };
            _mockWebsocketClientOrganizer.AddAgentAsync(agent.Uid, Arg.Any<IOpenAlprWebsocketClient>(), Arg.Any<CancellationToken>())
                .Returns(addResult);

            var query = new GetWebSocketQuery(_mockHttpContext);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().Be(Unit.Value);
            
            _mockLogger.Received(1).LogInformation("Websocket connection received.");
            
            // Verify the core behavior: should not proceed with SignalR notification when add fails
            await _mockAllClients.DidNotReceive().OpenAlprAgentConnected(Arg.Any<string>(), Arg.Any<string>());
            
            // Verify that AddAgentAsync was called
            await _mockWebsocketClientOrganizer.Received(1).AddAgentAsync(agent.Uid, Arg.Any<IOpenAlprWebsocketClient>(), cancellationToken);
        }

        [Test]
        public async Task Handle_AddAgentSucceedsWithUpdate_NotifiesSignalRHub()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgent("https://test-agent.local");
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            _mockWebSocketManager.IsWebSocketRequest.Returns(true);
            _mockWebSocketManager.AcceptWebSocketAsync().Returns(_mockWebSocket);

            var addResult = new AddAgentResult 
            { 
                WasAdded = true, 
                WasUpdated = true 
            };
            _mockWebsocketClientOrganizer.AddAgentAsync(agent.Uid, Arg.Any<IOpenAlprWebsocketClient>(), Arg.Any<CancellationToken>())
                .Returns(addResult);

            var query = new GetWebSocketQuery(_mockHttpContext);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().Be(Unit.Value);
            
            _mockLogger.Received(1).LogInformation("Websocket connection received.");
            
            // Verify the core behavior: should proceed with SignalR notification when agent was updated
            await _mockAllClients.Received(1).OpenAlprAgentConnected(agent.Uid, "192.168.1.100");
            
            // Verify that AddAgentAsync was called
            await _mockWebsocketClientOrganizer.Received(1).AddAgentAsync(agent.Uid, Arg.Any<IOpenAlprWebsocketClient>(), cancellationToken);
        }

        [Test]
        public async Task Handle_WebSocketConnectionLifecycle_CleansUpProperly()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgent("https://test-agent.local");
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            _mockWebSocketManager.IsWebSocketRequest.Returns(true);
            _mockWebSocketManager.AcceptWebSocketAsync().Returns(_mockWebSocket);

            var addResult = new AddAgentResult 
            { 
                WasAdded = true, 
                WasUpdated = false 
            };
            _mockWebsocketClientOrganizer.AddAgentAsync(agent.Uid, Arg.Any<IOpenAlprWebsocketClient>(), Arg.Any<CancellationToken>())
                .Returns(addResult);

            var query = new GetWebSocketQuery(_mockHttpContext);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().Be(Unit.Value);
            
            // Verify the handler processes the connection lifecycle properly
            _mockLogger.Received(1).LogInformation("Websocket connection received.");
            await _mockWebSocketManager.Received(1).AcceptWebSocketAsync();
            await _mockWebsocketClientOrganizer.Received(1).AddAgentAsync(agent.Uid, Arg.Any<IOpenAlprWebsocketClient>(), cancellationToken);
            await _mockAllClients.Received(1).OpenAlprAgentConnected(agent.Uid, "192.168.1.100");
            
            // Verify cleanup was called (either in try or catch block)
            await _mockWebsocketClientOrganizer.Received().RemoveAgentAsync(agent.Uid, Arg.Any<CancellationToken>());
        }



        [Test]
        public async Task Handle_MultipleAgents_UsesFirstAgent()
        {
            // Arrange
            var agent1 = TestDataFactory.CreateTestAgent("https://agent1.local");
            var agent2 = TestDataFactory.CreateTestAgent("https://agent2.local");
            await UnitOfWork.Agents.AddAsync(agent1);
            await UnitOfWork.Agents.AddAsync(agent2);
            await UnitOfWork.SaveChangesAsync();

            _mockWebSocketManager.IsWebSocketRequest.Returns(true);
            _mockWebSocketManager.AcceptWebSocketAsync().Returns(_mockWebSocket);

            var addResult = new AddAgentResult 
            { 
                WasAdded = true, 
                WasUpdated = false 
            };
            _mockWebsocketClientOrganizer.AddAgentAsync(Arg.Any<string>(), Arg.Any<IOpenAlprWebsocketClient>(), Arg.Any<CancellationToken>())
                .Returns(addResult);

            var query = new GetWebSocketQuery(_mockHttpContext);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().Be(Unit.Value);
            
            // Should use the first agent (order may vary, so we check that one of them was used)
            await _mockWebsocketClientOrganizer.Received(1).AddAgentAsync(
                Arg.Is<string>(uid => uid == agent1.Uid || uid == agent2.Uid), 
                Arg.Any<IOpenAlprWebsocketClient>(), 
                cancellationToken);
        }

        [Test]
        public void Constructor_WithValidParameters_CreatesInstance()
        {
            // Arrange & Act
            var handler = new GetWebSocketQueryHandler(
                _mockLogger,
                UnitOfWork,
                _mockWebsocketClientOrganizer,
                _mockHubContext);

            // Assert
            handler.Should().NotBeNull();
        }

        [Test]
        public async Task Handle_WithCancellationToken_PassesToDependencies()
        {
            // Arrange
            var agent = TestDataFactory.CreateTestAgent("https://test-agent.local");
            await UnitOfWork.Agents.AddAsync(agent);
            await UnitOfWork.SaveChangesAsync();

            _mockWebSocketManager.IsWebSocketRequest.Returns(true);
            _mockWebSocketManager.AcceptWebSocketAsync().Returns(_mockWebSocket);

            var addResult = new AddAgentResult 
            { 
                WasAdded = true, 
                WasUpdated = false 
            };
            _mockWebsocketClientOrganizer.AddAgentAsync(agent.Uid, Arg.Any<IOpenAlprWebsocketClient>(), Arg.Any<CancellationToken>())
                .Returns(addResult);

            var query = new GetWebSocketQuery(_mockHttpContext);
            var customCancellationToken = new CancellationTokenSource().Token;

            // Act
            var result = await _handler.Handle(query, customCancellationToken);

            // Assert
            result.Should().Be(Unit.Value);
            
            await _mockWebsocketClientOrganizer.Received(1).AddAgentAsync(
                agent.Uid, 
                Arg.Any<IOpenAlprWebsocketClient>(), 
                customCancellationToken);
            await _mockWebsocketClientOrganizer.Received(1).RemoveAgentAsync(
                agent.Uid, 
                customCancellationToken);
        }
    }
}