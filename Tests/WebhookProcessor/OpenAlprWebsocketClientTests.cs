using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Server.WebhookProcessor.OpenAlprWebsocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.WebhookProcessor
{
    [TestFixture]
    public class OpenAlprWebsocketClientTests
    {
        private ILogger _logger;
        private WebSocket _webSocket;
        private OpenAlprWebsocketClient _client;
        private string _agentId;

        [SetUp]
        public void SetUp()
        {
            _logger = Substitute.For<ILogger>();
            _webSocket = Substitute.For<WebSocket>();
            _agentId = "test-agent";
            _client = new OpenAlprWebsocketClient(
                _logger,
                _agentId,
                _webSocket);
        }

        [TearDown]
        public void TearDown()
        {
            _webSocket.Dispose();
        }

        [Test]
        public async Task SendGetImageRequestAsync_SendsCorrectPayload()
        {
            var transactionId = Guid.NewGuid();
            var cameraId = 123L;
            var cts = new CancellationToken();

            await _client.SendGetImageRequestAsync(
                transactionId,
                cameraId,
                cts);

            await _webSocket
                .Received()
                .SendAsync(
                    Arg.Is<ArraySegment<byte>>(segment =>
                        Encoding.UTF8.GetString(segment)!.Contains(transactionId.ToString()) &&
                        Encoding.UTF8.GetString(segment)!.Contains(cameraId.ToString())),
                    WebSocketMessageType.Binary,
                    true,
                    cts);
        }

        [Test]
        public async Task TryGetImageDownloadResponse_ReturnsTrue_WhenValidResponseExists()
        {
            var transactionId = Guid.NewGuid();
            var expectedResponse = new ImageDownloadResponse()
            {
                Image = "asdf",
                ResponseCode = "asdf",
                TransactionId = transactionId,
            };

            var expectedImage = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(expectedResponse));

            _webSocket.ReceiveAsync(Arg.Any<ArraySegment<byte>>(), Arg.Any<CancellationToken>())
                .Returns(
                    async callInfo =>
                    {
                        var receiveResult = new WebSocketReceiveResult(
                            expectedImage.Length,
                            WebSocketMessageType.Text,
                            true);

                        var buffer = callInfo.Arg<ArraySegment<byte>>();
                        Array.Copy(expectedImage, buffer.Array, expectedImage.Length);
                        return await ValueTask.FromResult(receiveResult);
                    },
                    async callInfo =>
                    {
                        var receiveResult = new WebSocketReceiveResult(
                            0,
                            WebSocketMessageType.Close,
                            true,
                            WebSocketCloseStatus.NormalClosure,
                            string.Empty);

                        var buffer = callInfo.Arg<ArraySegment<byte>>();
                        Array.Copy(expectedImage, buffer.Array, expectedImage.Length);
                        return await ValueTask.FromResult(receiveResult);
                    });

            await _client.ConsumeMessagesAsync(CancellationToken.None);

            var result = _client.TryGetImageDownloadResponse(transactionId, out var downloadedImage);

            Assert.That(result, Is.True);
            Assert.That(downloadedImage, Is.Not.Null);
        }

        //[Test]
        //public void TryGetImageDownloadResponse_ReturnsFalse_WhenDeserializationFails()
        //{
        //}

        //[Test]
        //public async Task SendGetAgentStatusRequestAsync_SendsCorrectPayload()
        //{
        //   
        //}

        //[Test]
        //public void TryGetAgentResponse_ReturnsTrue_WhenValidResponseExists()
        //{
        //    
        //}

        //[Test]
        //public async Task SendSaveMaskRequestAsync_SendsCorrectPayload()
        //{
        //    
        //}

        //[Test]
        //public async Task CloseConnectionAsync_ClosesOnlyWhenOpen()
        //{
        //    
        //}

        //[Test]
        //public async Task CloseConnectionAsync_DoesNothing_WhenNotOpen()
        //{
        //}
    }
}