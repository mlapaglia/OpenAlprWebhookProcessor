using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Tests.TestHelpers;

namespace Tests.WebhookProcessor
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class OpenAlprWebsocketClientTests : TestBase
    {
        private ILogger _mockLogger;
        private WebSocket _mockWebSocket;
        private string _testAgentId;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _mockLogger = Substitute.For<ILogger>();
            _mockWebSocket = Substitute.For<WebSocket>();
            _testAgentId = "test-agent-123";
        }

        [TearDown]
        public override void TearDown()
        {
            _mockWebSocket?.Dispose();
            base.TearDown();
        }

        [Test]
        public void Constructor_WithValidParameters_CreatesInstance()
        {
            // Act
            var client = new OpenAlprWebsocketClient(_mockLogger, _testAgentId, _mockWebSocket);

            // Assert
            client.Should().NotBeNull();
        }

        [Test]
        public void Constructor_WithNullLogger_CreatesInstance()
        {
            // Act & Assert - The constructor doesn't validate null logger
            Assert.DoesNotThrow(() => 
                new OpenAlprWebsocketClient(null, _testAgentId, _mockWebSocket));
        }

        [Test]
        public void Constructor_WithNullAgentId_CreatesInstance()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => 
                new OpenAlprWebsocketClient(_mockLogger, null, _mockWebSocket));
        }

        [Test]
        public void Constructor_WithNullWebSocket_CreatesInstance()
        {
            // Act & Assert - The constructor doesn't validate null WebSocket
            Assert.DoesNotThrow(() => 
                new OpenAlprWebsocketClient(_mockLogger, _testAgentId, null));
        }

        [Test]
        public void TryGetAgentResponse_WithNoAvailableResponse_ReturnsFalse()
        {
            // Arrange
            var client = new OpenAlprWebsocketClient(_mockLogger, _testAgentId, _mockWebSocket);
            var transactionId = Guid.NewGuid();

            // Act
            var result = client.TryGetAgentResponse<TestResponse>(transactionId, out var response);

            // Assert
            result.Should().BeFalse();
            response.Should().BeNull();
        }

        [Test]
        public void TryGetAgentResponse_WithValidResponse_ReturnsTrue()
        {
            // Arrange
            var client = new OpenAlprWebsocketClient(_mockLogger, _testAgentId, _mockWebSocket);
            var transactionId = Guid.NewGuid();
            var testResponse = new TestResponse { Message = "Test message", Success = true };
            var jsonResponse = JsonSerializer.Serialize(testResponse);

            // Use reflection to add response to internal dictionary
            var availableResponsesField = typeof(OpenAlprWebsocketClient)
                .GetField("_availableResponses", BindingFlags.NonPublic | BindingFlags.Instance);
            var availableResponses = (ConcurrentDictionary<Guid, string>)availableResponsesField.GetValue(client);
            availableResponses.TryAdd(transactionId, jsonResponse);

            // Act
            var result = client.TryGetAgentResponse<TestResponse>(transactionId, out var response);

            // Assert
            result.Should().BeTrue();
            response.Should().NotBeNull();
            response.Message.Should().Be("Test message");
            response.Success.Should().BeTrue();
        }

        [Test]
        public void TryGetAgentResponse_WithInvalidJson_ReturnsFalse()
        {
            // Arrange
            var client = new OpenAlprWebsocketClient(_mockLogger, _testAgentId, _mockWebSocket);
            var transactionId = Guid.NewGuid();
            var invalidJson = "{ invalid json }";

            // Use reflection to add response to internal dictionary
            var availableResponsesField = typeof(OpenAlprWebsocketClient)
                .GetField("_availableResponses", BindingFlags.NonPublic | BindingFlags.Instance);
            var availableResponses = (ConcurrentDictionary<Guid, string>)availableResponsesField.GetValue(client);
            availableResponses.TryAdd(transactionId, invalidJson);

            // Act
            var result = client.TryGetAgentResponse<TestResponse>(transactionId, out var response);

            // Assert
            result.Should().BeFalse();
            response.Should().BeNull();
            _mockLogger.Received(1).LogError(Arg.Any<Exception>(), "Failed to deserialize AgentStatusResponse");
        }

        [Test]
        public void TryGetImageDownloadResponse_WithNoAvailableResponse_ReturnsFalse()
        {
            // Arrange
            var client = new OpenAlprWebsocketClient(_mockLogger, _testAgentId, _mockWebSocket);
            var transactionId = Guid.NewGuid();

            // Act
            var result = client.TryGetImageDownloadResponse(transactionId, out var stream);

            // Assert
            result.Should().BeFalse();
            stream.Should().BeNull();
        }

        [Test]
        public void TryGetImageDownloadResponse_WithValidResponse_ReturnsTrue()
        {
            // Arrange
            var client = new OpenAlprWebsocketClient(_mockLogger, _testAgentId, _mockWebSocket);
            var transactionId = Guid.NewGuid();
            var testImageData = "test-image-data-base64";
            var imageResponse = new ImageDownloadResponse { Image = testImageData };
            var jsonResponse = JsonSerializer.Serialize(imageResponse);

            // Use reflection to add response to internal dictionary
            var availableResponsesField = typeof(OpenAlprWebsocketClient)
                .GetField("_availableResponses", BindingFlags.NonPublic | BindingFlags.Instance);
            var availableResponses = (ConcurrentDictionary<Guid, string>)availableResponsesField.GetValue(client);
            availableResponses.TryAdd(transactionId, jsonResponse);

            // Act
            var result = client.TryGetImageDownloadResponse(transactionId, out var stream);

            // Assert
            result.Should().BeTrue();
            stream.Should().NotBeNull();
            stream.Should().BeOfType<MemoryStream>();
            
            // Verify stream content
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            content.Should().Be(testImageData);
        }

        [Test]
        public void TryGetImageDownloadResponse_WithInvalidJson_ReturnsFalse()
        {
            // Arrange
            var client = new OpenAlprWebsocketClient(_mockLogger, _testAgentId, _mockWebSocket);
            var transactionId = Guid.NewGuid();
            var invalidJson = "{ invalid json }";

            // Use reflection to add response to internal dictionary
            var availableResponsesField = typeof(OpenAlprWebsocketClient)
                .GetField("_availableResponses", BindingFlags.NonPublic | BindingFlags.Instance);
            var availableResponses = (ConcurrentDictionary<Guid, string>)availableResponsesField.GetValue(client);
            availableResponses.TryAdd(transactionId, invalidJson);

            // Act
            var result = client.TryGetImageDownloadResponse(transactionId, out var stream);

            // Assert
            result.Should().BeFalse();
            stream.Should().BeNull();
            _mockLogger.Received(1).LogError(Arg.Any<Exception>(), "Failed to deserialize AgentStatusResponse");
        }

        [Test]
        public void TransactionIdRegex_WithValidTransactionId_ExtractsCorrectly()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var message = $"{{\"transaction_id\":\"{transactionId}\",\"other\":\"data\"}}";

            // Use reflection to get the private regex method
            var regexMethod = typeof(OpenAlprWebsocketClient)
                .GetMethod("TransactionIdRegex", BindingFlags.NonPublic | BindingFlags.Static);
            var regex = (Regex)regexMethod.Invoke(null, null);

            // Act
            var match = regex.Match(message);

            // Assert
            match.Success.Should().BeTrue();
            match.Groups[1].Value.Should().Be(transactionId.ToString());
        }

        [Test]
        public void TransactionIdRegex_WithNoTransactionId_DoesNotMatch()
        {
            // Arrange
            var message = "{\"other\":\"data\",\"no\":\"transaction_id\"}";

            // Use reflection to get the private regex method
            var regexMethod = typeof(OpenAlprWebsocketClient)
                .GetMethod("TransactionIdRegex", BindingFlags.NonPublic | BindingFlags.Static);
            var regex = (Regex)regexMethod.Invoke(null, null);

            // Act
            var match = regex.Match(message);

            // Assert
            match.Success.Should().BeFalse();
        }

        [Test]
        public void TransactionIdRegex_WithMalformedTransactionId_DoesNotMatch()
        {
            // Arrange
            var message = "{\"transaction_id\":\"not-a-guid\",\"other\":\"data\"}";

            // Use reflection to get the private regex method
            var regexMethod = typeof(OpenAlprWebsocketClient)
                .GetMethod("TransactionIdRegex", BindingFlags.NonPublic | BindingFlags.Static);
            var regex = (Regex)regexMethod.Invoke(null, null);

            // Act
            var match = regex.Match(message);

            // Assert
            match.Success.Should().BeTrue(); // Regex matches, but Guid.Parse would fail
            Assert.Throws<FormatException>(() => Guid.Parse(match.Groups[1].Value));
        }

        [Test]
        public async Task CloseConnectionAsync_WithOpenWebSocket_CallsCloseAsync()
        {
            // Arrange
            var client = new OpenAlprWebsocketClient(_mockLogger, _testAgentId, _mockWebSocket);
            _mockWebSocket.State.Returns(WebSocketState.Open);

            // Act
            await client.CloseConnectionAsync();

            // Assert
            await _mockWebSocket.Received(1).CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "goodbye.",
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task CloseConnectionAsync_WithClosedWebSocket_DoesNotCallCloseAsync()
        {
            // Arrange
            var client = new OpenAlprWebsocketClient(_mockLogger, _testAgentId, _mockWebSocket);
            _mockWebSocket.State.Returns(WebSocketState.Closed);

            // Act
            await client.CloseConnectionAsync();

            // Assert
            await _mockWebSocket.DidNotReceive().CloseAsync(
                Arg.Any<WebSocketCloseStatus>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task CloseConnectionAsync_WithCancellationToken_PassesTokenToWebSocket()
        {
            // Arrange
            var client = new OpenAlprWebsocketClient(_mockLogger, _testAgentId, _mockWebSocket);
            _mockWebSocket.State.Returns(WebSocketState.Open);
            using var cts = new CancellationTokenSource();

            // Act
            await client.CloseConnectionAsync(cts.Token);

            // Assert
            await _mockWebSocket.Received(1).CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "goodbye.",
                cts.Token);
        }

        [Test]
        public void ResponseRemoval_AfterSuccessfulTryGet_RemovesFromInternalDictionary()
        {
            // Arrange
            var client = new OpenAlprWebsocketClient(_mockLogger, _testAgentId, _mockWebSocket);
            var transactionId = Guid.NewGuid();
            var testResponse = new TestResponse { Message = "Test", Success = true };
            var jsonResponse = JsonSerializer.Serialize(testResponse);

            // Use reflection to add response to internal dictionary
            var availableResponsesField = typeof(OpenAlprWebsocketClient)
                .GetField("_availableResponses", BindingFlags.NonPublic | BindingFlags.Instance);
            var availableResponses = (ConcurrentDictionary<Guid, string>)availableResponsesField.GetValue(client);
            availableResponses.TryAdd(transactionId, jsonResponse);

            // Act - First call should succeed
            var result1 = client.TryGetAgentResponse<TestResponse>(transactionId, out var response1);
            
            // Act - Second call should fail because response was removed
            var result2 = client.TryGetAgentResponse<TestResponse>(transactionId, out var response2);

            // Assert
            result1.Should().BeTrue();
            response1.Should().NotBeNull();
            
            result2.Should().BeFalse();
            response2.Should().BeNull();
            
            // Verify response is removed from internal dictionary
            availableResponses.ContainsKey(transactionId).Should().BeFalse();
        }

        [Test]
        public void ConcurrentResponseAccess_MultipleThreads_HandlesCorrectly()
        {
            // Arrange
            var client = new OpenAlprWebsocketClient(_mockLogger, _testAgentId, _mockWebSocket);
            var transactionId = Guid.NewGuid();
            var testResponse = new TestResponse { Message = "Test", Success = true };
            var jsonResponse = JsonSerializer.Serialize(testResponse);

            // Use reflection to add response to internal dictionary
            var availableResponsesField = typeof(OpenAlprWebsocketClient)
                .GetField("_availableResponses", BindingFlags.NonPublic | BindingFlags.Instance);
            var availableResponses = (ConcurrentDictionary<Guid, string>)availableResponsesField.GetValue(client);
            availableResponses.TryAdd(transactionId, jsonResponse);

            // Act - Multiple threads trying to get the same response
            var tasks = new Task<(bool success, TestResponse response)>[10];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    var success = client.TryGetAgentResponse<TestResponse>(transactionId, out var response);
                    return (success, response);
                });
            }

            Task.WaitAll(tasks);

            // Assert - Only one thread should have succeeded
            var successCount = 0;
            var failureCount = 0;
            
            foreach (var task in tasks)
            {
                if (task.Result.success)
                {
                    successCount++;
                    task.Result.response.Should().NotBeNull();
                }
                else
                {
                    failureCount++;
                    task.Result.response.Should().BeNull();
                }
            }

            successCount.Should().Be(1);
            failureCount.Should().Be(9);
        }

        // Helper classes for testing
        private class TestResponse
        {
            public string Message { get; set; }
            public bool Success { get; set; }
        }

        private class ImageDownloadResponse
        {
            [System.Text.Json.Serialization.JsonPropertyName("image")]
            public string Image { get; set; }
        }
    }
}