using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.ProcessorHub;
using System.Security.Claims;
using Tests.TestHelpers;

namespace Tests.SignalRHub
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class ProcessorHubTests : TestBase
    {
        private OpenAlprWebhookProcessor.ProcessorHub.ProcessorHub _hub;
        private ILogger<OpenAlprWebhookProcessor.ProcessorHub.ProcessorHub> _logger;
        private HubCallerContext _hubContext;
        private ClaimsPrincipal _user;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _logger = Substitute.For<ILogger<OpenAlprWebhookProcessor.ProcessorHub.ProcessorHub>>();
            _hubContext = Substitute.For<HubCallerContext>();

            // Setup authenticated user identity
            _user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "TestUser")
            }, "TestAuth")); // Authentication type is required for IsAuthenticated to be true

            // Setup hub context with minimal required setup
            _hubContext.ConnectionId.Returns("test-connection-id");
            _hubContext.User.Returns(_user);

            _hub = new OpenAlprWebhookProcessor.ProcessorHub.ProcessorHub(_logger);
            
            // Use reflection to set the protected Context property
            var contextProperty = typeof(Hub).GetProperty("Context");
            contextProperty?.SetValue(_hub, _hubContext);
        }

        [TearDown]
        public override void TearDown()
        {
            // Clear static connections after each test
            var connectionsField = typeof(OpenAlprWebhookProcessor.ProcessorHub.ProcessorHub)
                .GetField("_connections", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var connectionDict = connectionsField?.GetValue(null) as System.Collections.Concurrent.ConcurrentDictionary<string, SignalRConnectionInfo>;
            connectionDict?.Clear();

            _hub?.Dispose();
            base.TearDown();
        }

        [Test]
        public void Constructor_WithValidLogger_InitializesCorrectly()
        {
            // Act & Assert
            _hub.Should().NotBeNull();
        }

        [Test]
        public void Constructor_WithNullLogger_AcceptsNullLogger()
        {
            // Act & Assert - Constructor should not throw, but accept null logger
            var hub = new OpenAlprWebhookProcessor.ProcessorHub.ProcessorHub(null);
            hub.Should().NotBeNull();
        }

        [Test]
        public async Task OnConnectedAsync_WithBasicSetup_AddsConnectionToStaticCollection()
        {
            // Arrange
            _hubContext.GetHttpContext().Returns((HttpContext)null); // Simulate null context

            // Act
            await _hub.OnConnectedAsync();

            // Assert
            var connections = OpenAlprWebhookProcessor.ProcessorHub.ProcessorHub.GetAllConnections();
            connections.Should().HaveCount(1);
            
            var connection = connections[0];
            connection.ConnectionId.Should().Be("test-connection-id");
            connection.UserId.Should().Be("TestUser");
            connection.UserAgent.Should().Be("Unknown"); // Default when no context
            connection.IpAddress.Should().Be("Unknown"); // Default when no context
            connection.Transport.Should().Be("Long Polling"); // Default transport
            connection.ConnectedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Test]
        public async Task OnDisconnectedAsync_WithExistingConnection_RemovesConnectionFromStaticCollection()
        {
            // Arrange
            await _hub.OnConnectedAsync(); // First connect
            var connectionsBeforeDisconnect = OpenAlprWebhookProcessor.ProcessorHub.ProcessorHub.GetAllConnections();
            connectionsBeforeDisconnect.Should().HaveCount(1);

            // Act
            await _hub.OnDisconnectedAsync(null);

            // Assert
            var connectionsAfterDisconnect = OpenAlprWebhookProcessor.ProcessorHub.ProcessorHub.GetAllConnections();
            connectionsAfterDisconnect.Should().HaveCount(0);
        }

        [Test]
        public async Task OnDisconnectedAsync_WithException_RemovesConnectionAndHandlesException()
        {
            // Arrange
            await _hub.OnConnectedAsync(); // First connect
            var testException = new InvalidOperationException("Test connection error");

            // Act
            await _hub.OnDisconnectedAsync(testException);

            // Assert
            var connections = OpenAlprWebhookProcessor.ProcessorHub.ProcessorHub.GetAllConnections();
            connections.Should().HaveCount(0);
        }

        [Test]
        public async Task OnDisconnectedAsync_WithNonExistentConnection_HandlesGracefully()
        {
            // Arrange - no prior connection

            // Act & Assert - should not throw
            await _hub.OnDisconnectedAsync(null);
        }

        [Test]
        public void GetAllConnections_WithNoConnections_ReturnsEmptyArray()
        {
            // Act
            var connections = OpenAlprWebhookProcessor.ProcessorHub.ProcessorHub.GetAllConnections();

            // Assert
            connections.Should().NotBeNull();
            connections.Should().HaveCount(0);
        }

        [Test]
        public async Task GetAllConnections_ReturnsArrayOfConnections()
        {
            // Arrange
            await _hub.OnConnectedAsync();

            // Act
            var connections = OpenAlprWebhookProcessor.ProcessorHub.ProcessorHub.GetAllConnections();

            // Assert
            connections.Should().HaveCount(1);
            connections[0].Should().NotBeNull();
        }

        [Test]
        public async Task ConnectionLifecycle_ConnectAndDisconnect_ManagesConnectionsProperly()
        {
            // Arrange & Act - Connect
            await _hub.OnConnectedAsync();
            var connectionsAfterConnect = OpenAlprWebhookProcessor.ProcessorHub.ProcessorHub.GetAllConnections();

            // Assert connection was added
            connectionsAfterConnect.Should().HaveCount(1);
            connectionsAfterConnect[0].ConnectionId.Should().Be("test-connection-id");
            connectionsAfterConnect[0].UserId.Should().Be("TestUser");

            // Act - Disconnect
            await _hub.OnDisconnectedAsync(null);
            var connectionsAfterDisconnect = OpenAlprWebhookProcessor.ProcessorHub.ProcessorHub.GetAllConnections();

            // Assert connection was removed
            connectionsAfterDisconnect.Should().HaveCount(0);
        }

        [Test]
        public async Task OnConnectedAsync_LogsDebugInformation()
        {
            // Act
            await _hub.OnConnectedAsync();

            // Assert - Verify that logging was called (we can't easily verify exact message due to string interpolation)
            _logger.ReceivedCalls().Should().NotBeEmpty();
        }

        [Test]
        public async Task OnDisconnectedAsync_LogsDebugInformation()
        {
            // Arrange
            await _hub.OnConnectedAsync();

            // Act
            await _hub.OnDisconnectedAsync(null);

            // Assert - Verify that logging was called
            _logger.ReceivedCalls().Should().NotBeEmpty();
        }
    }
}