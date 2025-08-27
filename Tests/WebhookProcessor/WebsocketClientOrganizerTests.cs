using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor.OpenAlprWebsocket;
using Tests.TestHelpers;

namespace Tests.WebhookProcessor
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class WebsocketClientOrganizerTests : TestBase
    {
        private WebsocketClientOrganizer _organizer;
        private ILogger<WebsocketClientOrganizer> _logger;
        private IOptions<WebsocketClientOrganizerConfiguration> _options;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _logger = Substitute.For<ILogger<WebsocketClientOrganizer>>();
            
            var config = new WebsocketClientOrganizerConfiguration
            {
                TimeoutMilliseconds = 1
            };

            _options = Options.Create(config);
            _options.Value.TimeoutMilliseconds = 1;

            _organizer = new WebsocketClientOrganizer(_options, _logger);
        }

        [Test]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            var act = () => new WebsocketClientOrganizer(_options, null);
            
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }

        [Test]
        public async Task AddAgentAsync_NullAgentId_ThrowsArgumentException()
        {
            var client = CreateMockClient();
            
            var act = async () => await _organizer.AddAgentAsync(null, client, CancellationToken.None);
            
            await act.Should().ThrowExactlyAsync<ArgumentNullException>()
                .WithParameterName("agentId");
        }

        [Test]
        public async Task AddAgentAsync_EmptyAgentId_ThrowsArgumentException()
        {
            var client = CreateMockClient();
            
            var act = async () => await _organizer.AddAgentAsync("", client, CancellationToken.None);
            
            await act.Should().ThrowExactlyAsync<ArgumentException>()
                .WithParameterName("agentId");
        }

        [Test]
        public async Task AddAgentAsync_NullClient_ThrowsArgumentNullException()
        {
            var act = async () => await _organizer.AddAgentAsync("agent1", null, CancellationToken.None);
            
            await act.Should().ThrowExactlyAsync<ArgumentNullException>()
                .WithParameterName("webSocketClient");
        }

        [Test]
        public async Task AddAgentAsync_NewAgent_AddedSuccessfully()
        {
            var client = CreateMockClient();
            const string agentId = "agent1";

            var result = await _organizer.AddAgentAsync(agentId, client, CancellationToken.None);

            result.Should().NotBeNull();
            result.WasAdded.Should().BeTrue();
            result.WasUpdated.Should().BeFalse();
            result.UpdateWasCleanDisconnect.Should().BeFalse();

            var connectedClients = _organizer.GetConnectedClients();
            connectedClients.Should().ContainKey(agentId);
            connectedClients[agentId].Should().Be(client);
        }

        [Test]
        public async Task AddAgentAsync_ExistingAgent_ReplacesWithCleanDisconnect()
        {
            var oldClient = CreateMockClient();
            var newClient = CreateMockClient();
            const string agentId = "agent1";

            await _organizer.AddAgentAsync(agentId, oldClient, CancellationToken.None);
            
            var result = await _organizer.AddAgentAsync(agentId, newClient, CancellationToken.None);

            result.Should().NotBeNull();
            result.WasAdded.Should().BeTrue();
            result.WasUpdated.Should().BeTrue();
            result.UpdateWasCleanDisconnect.Should().BeTrue();

            await oldClient.Received(1).CloseConnectionAsync(Arg.Any<CancellationToken>());
            
            var connectedClients = _organizer.GetConnectedClients();
            connectedClients[agentId].Should().Be(newClient);
        }

        [Test]
        public async Task AddAgentAsync_ExistingAgentDisconnectFails_StillReplacesAgent()
        {
            var oldClient = CreateMockClient();
            var newClient = CreateMockClient();
            const string agentId = "agent1";

            oldClient.CloseConnectionAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromException(new Exception("Disconnect failed")));

            await _organizer.AddAgentAsync(agentId, oldClient, CancellationToken.None);
            
            var result = await _organizer.AddAgentAsync(agentId, newClient, CancellationToken.None);

            result.Should().NotBeNull();
            result.WasAdded.Should().BeTrue();
            result.WasUpdated.Should().BeTrue();
            result.UpdateWasCleanDisconnect.Should().BeFalse();
            
            var connectedClients = _organizer.GetConnectedClients();
            connectedClients[agentId].Should().Be(newClient);
        }

        [Test]
        public async Task RemoveAgentAsync_NullAgentId_ThrowsArgumentException()
        {
            var act = async () => await _organizer.RemoveAgentAsync(null, CancellationToken.None);
            
            await act.Should().ThrowExactlyAsync<ArgumentException>()
                .WithParameterName("agentId");
        }

        [Test]
        public async Task RemoveAgentAsync_EmptyAgentId_ThrowsArgumentException()
        {
            var act = async () => await _organizer.RemoveAgentAsync("", CancellationToken.None);
            
            await act.Should().ThrowExactlyAsync<ArgumentException>()
                .WithParameterName("agentId");
        }

        [Test]
        public async Task RemoveAgentAsync_ExistingAgent_RemovesAndDisconnects()
        {
            var client = CreateMockClient();
            const string agentId = "agent1";

            await _organizer.AddAgentAsync(agentId, client, CancellationToken.None);
            
            await _organizer.RemoveAgentAsync(agentId, CancellationToken.None);

            await client.Received(1).CloseConnectionAsync(Arg.Any<CancellationToken>());
            
            var connectedClients = _organizer.GetConnectedClients();
            connectedClients.Should().NotContainKey(agentId);
        }

        [Test]
        public async Task RemoveAgentAsync_NonExistentAgent_DoesNotThrow()
        {
            const string agentId = "nonexistent";
            
            var act = async () => await _organizer.RemoveAgentAsync(agentId, CancellationToken.None);

            await act.Should().NotThrowAsync();
        }

        [Test]
        public async Task RemoveAgentAsync_DisconnectFails_ThrowsException()
        {
            var client = CreateMockClient();
            const string agentId = "agent1";
            var exception = new Exception("Disconnect failed");

            client.CloseConnectionAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromException(exception));

            await _organizer.AddAgentAsync(agentId, client, CancellationToken.None);
            
            var act = async () => await _organizer.RemoveAgentAsync(agentId, CancellationToken.None);
            
            await act.Should().ThrowExactlyAsync<Exception>()
                .WithMessage("Disconnect failed");
        }

        [Test]
        public async Task GetAgentStatusAsync_NullAgentId_ThrowsArgumentException()
        {
            var act = async () => await _organizer.GetAgentStatusAsync(null, CancellationToken.None);
            
            await act.Should().ThrowExactlyAsync<ArgumentException>()
                .WithParameterName("agentId");
        }

        [Test]
        public async Task GetAgentStatusAsync_EmptyAgentId_ThrowsArgumentException()
        {
            var act = async () => await _organizer.GetAgentStatusAsync("", CancellationToken.None);
            
            await act.Should().ThrowExactlyAsync<ArgumentException>()
                .WithParameterName("agentId");
        }

        [Test]
        public async Task GetAgentStatusAsync_NonExistentAgent_ReturnsNull()
        {
            const string agentId = "nonexistent";
            
            var result = await _organizer.GetAgentStatusAsync(agentId, CancellationToken.None);
            
            result.Should().BeNull();
        }

        [Test]
        public async Task GetAgentStatusAsync_ExistingAgentResponds_ReturnsStatus()
        {
            var client = CreateMockClient();
            const string agentId = "agent1";
            var expectedResponse = new AgentStatusResponse { Type = "status" };

            client.TryGetAgentResponse<AgentStatusResponse>(Arg.Any<Guid>(), out Arg.Any<AgentStatusResponse>())
                .Returns(x =>
                {
                    x[1] = expectedResponse;
                    return true;
                });

            await _organizer.AddAgentAsync(agentId, client, CancellationToken.None);
            
            var result = await _organizer.GetAgentStatusAsync(agentId, CancellationToken.None);
            
            result.Should().Be(expectedResponse);
            await client.Received(1).SendGetAgentStatusRequestAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task GetAgentStatusAsync_AgentDoesNotRespond_ReturnsNull()
        {
            var client = CreateMockClient();
            const string agentId = "agent1";

            client.TryGetAgentResponse<AgentStatusResponse>(Arg.Any<Guid>(), out Arg.Any<AgentStatusResponse>())
                .Returns(false);

            await _organizer.AddAgentAsync(agentId, client, CancellationToken.None);
            
            var result = await _organizer.GetAgentStatusAsync(agentId, CancellationToken.None);
            
            result.Should().BeNull();
        }

        [Test]
        public async Task DisableEnableAgentAsync_NullAgentId_ThrowsArgumentException()
        {
            var act = async () => await _organizer.DisableEnableAgentAsync(null, AgentStartStopType.Start, CancellationToken.None);
            
            await act.Should().ThrowExactlyAsync<ArgumentException>()
                .WithParameterName("agentId");
        }

        [Test]
        public async Task DisableEnableAgentAsync_EmptyAgentId_ThrowsArgumentException()
        {
            var act = async () => await _organizer.DisableEnableAgentAsync("", AgentStartStopType.Start, CancellationToken.None);
            
            await act.Should().ThrowExactlyAsync<ArgumentException>()
                .WithParameterName("agentId");
        }

        [Test]
        public async Task DisableEnableAgentAsync_NonExistentAgent_ReturnsFalse()
        {
            const string agentId = "nonexistent";
            
            var result = await _organizer.DisableEnableAgentAsync(agentId, AgentStartStopType.Start, CancellationToken.None);
            
            result.Should().BeFalse();
        }

        [Test]
        public async Task DisableEnableAgentAsync_ExistingAgentResponds_ReturnsSuccess()
        {
            var client = CreateMockClient();
            const string agentId = "agent1";
            var expectedResponse = new AgentStartStopResponse { Success = true };

            client.TryGetAgentResponse<AgentStartStopResponse>(Arg.Any<Guid>(), out Arg.Any<AgentStartStopResponse>())
                .Returns(x =>
                {
                    x[1] = expectedResponse;
                    return true;
                });

            await _organizer.AddAgentAsync(agentId, client, CancellationToken.None);
            
            var result = await _organizer.DisableEnableAgentAsync(agentId, AgentStartStopType.Start, CancellationToken.None);
            
            result.Should().BeTrue();
            await client.Received(1).SendAgentStartStopRequestAsync(
                Arg.Any<Guid>(), 
                AgentStartStopType.Start, 
                agentId, 
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task DisableEnableAgentAsync_AgentDoesNotRespond_ReturnsFalse()
        {
            var client = CreateMockClient();
            const string agentId = "agent1";

            client.TryGetAgentResponse<AgentStartStopResponse>(Arg.Any<Guid>(), out Arg.Any<AgentStartStopResponse>())
                .Returns(false);

            await _organizer.AddAgentAsync(agentId, client, CancellationToken.None);
            
            var result = await _organizer.DisableEnableAgentAsync(agentId, AgentStartStopType.Stop, CancellationToken.None);
            
            result.Should().BeFalse();
        }

        [Test]
        public async Task UpsertCameraMaskAsync_NullAgentId_ThrowsArgumentException()
        {
            var act = async () => await _organizer.UpsertCameraMaskAsync(null, "mask", "camera", CancellationToken.None);
            
            await act.Should().ThrowExactlyAsync<ArgumentException>()
                .WithParameterName("agentId");
        }

        [Test]
        public async Task UpsertCameraMaskAsync_EmptyAgentId_ThrowsArgumentException()
        {
            var act = async () => await _organizer.UpsertCameraMaskAsync("", "mask", "camera", CancellationToken.None);
            
            await act.Should().ThrowExactlyAsync<ArgumentException>()
                .WithParameterName("agentId");
        }

        [Test]
        public async Task UpsertCameraMaskAsync_NullMaskImage_ThrowsArgumentException()
        {
            var act = async () => await _organizer.UpsertCameraMaskAsync("agent1", null, "camera", CancellationToken.None);
            
            await act.Should().ThrowExactlyAsync<ArgumentException>()
                .WithParameterName("maskImage");
        }

        [Test]
        public async Task UpsertCameraMaskAsync_EmptyMaskImage_ThrowsArgumentException()
        {
            var act = async () => await _organizer.UpsertCameraMaskAsync("agent1", "", "camera", CancellationToken.None);
            
            await act.Should().ThrowExactlyAsync<ArgumentException>()
                .WithParameterName("maskImage");
        }

        [Test]
        public async Task UpsertCameraMaskAsync_NullOpenAlprName_ThrowsArgumentException()
        {
            var act = async () => await _organizer.UpsertCameraMaskAsync("agent1", "mask", null, CancellationToken.None);
            
            await act.Should().ThrowExactlyAsync<ArgumentException>()
                .WithParameterName("openAlprName");
        }

        [Test]
        public async Task UpsertCameraMaskAsync_EmptyOpenAlprName_ThrowsArgumentException()
        {
            var act = async () => await _organizer.UpsertCameraMaskAsync("agent1", "mask", "", CancellationToken.None);
            
            await act.Should().ThrowExactlyAsync<ArgumentException>()
                .WithParameterName("openAlprName");
        }

        [Test]
        public async Task UpsertCameraMaskAsync_NonExistentAgent_ReturnsFalse()
        {
            const string agentId = "nonexistent";
            
            var result = await _organizer.UpsertCameraMaskAsync(agentId, "mask", "camera", CancellationToken.None);
            
            result.Should().BeFalse();
        }

        [Test]
        public async Task UpsertCameraMaskAsync_ExistingAgentResponds_ReturnsTrue()
        {
            var client = CreateMockClient();
            const string agentId = "agent1";
            const string maskImage = "base64mask";
            const string cameraName = "camera1";
            var expectedResponse = new AgentStatusResponse { Type = "status" };

            client.TryGetAgentResponse<AgentStatusResponse>(Arg.Any<Guid>(), out Arg.Any<AgentStatusResponse>())
                .Returns(x =>
                {
                    x[1] = expectedResponse;
                    return true;
                });

            await _organizer.AddAgentAsync(agentId, client, CancellationToken.None);
            
            var result = await _organizer.UpsertCameraMaskAsync(agentId, maskImage, cameraName, CancellationToken.None);
            
            result.Should().BeTrue();
            await client.Received(1).SendSaveMaskRequestAsync(
                Arg.Any<Guid>(), 
                maskImage, 
                cameraName, 
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task UpsertCameraMaskAsync_AgentDoesNotRespond_ReturnsFalse()
        {
            var client = CreateMockClient();
            const string agentId = "agent1";

            client.TryGetAgentResponse<AgentStatusResponse>(Arg.Any<Guid>(), out Arg.Any<AgentStatusResponse>())
                .Returns(false);

            await _organizer.AddAgentAsync(agentId, client, CancellationToken.None);
            
            var result = await _organizer.UpsertCameraMaskAsync(agentId, "mask", "camera", CancellationToken.None);
            
            result.Should().BeFalse();
        }

        [Test]
        public void GetConnectedClients_ReturnsReadOnlyDictionary()
        {
            var client1 = CreateMockClient();
            var client2 = CreateMockClient();
            const string agentId1 = "agent1";
            const string agentId2 = "agent2";

            _organizer.AddAgentAsync(agentId1, client1, CancellationToken.None).Wait();
            _organizer.AddAgentAsync(agentId2, client2, CancellationToken.None).Wait();

            var connectedClients = _organizer.GetConnectedClients();

            connectedClients.Should().HaveCount(2);
            connectedClients.Should().ContainKey(agentId1);
            connectedClients.Should().ContainKey(agentId2);
            connectedClients[agentId1].Should().Be(client1);
            connectedClients[agentId2].Should().Be(client2);

            connectedClients.Should().BeAssignableTo<IReadOnlyDictionary<string, IOpenAlprWebsocketClient>>();
        }

        [Test]
        public async Task DisconnectAllClientsAsync_DisconnectsAllClients()
        {
            var client1 = CreateMockClient();
            var client2 = CreateMockClient();
            var client3 = CreateMockClient();
            const string agentId1 = "agent1";
            const string agentId2 = "agent2";
            const string agentId3 = "agent3";

            await _organizer.AddAgentAsync(agentId1, client1, CancellationToken.None);
            await _organizer.AddAgentAsync(agentId2, client2, CancellationToken.None);
            await _organizer.AddAgentAsync(agentId3, client3, CancellationToken.None);

            await _organizer.DisconnectAllClientsAsync(CancellationToken.None);

            await client1.Received(1).CloseConnectionAsync(Arg.Any<CancellationToken>());
            await client2.Received(1).CloseConnectionAsync(Arg.Any<CancellationToken>());
            await client3.Received(1).CloseConnectionAsync(Arg.Any<CancellationToken>());

            var connectedClients = _organizer.GetConnectedClients();
            connectedClients.Should().BeEmpty();
        }

        [Test]
        public async Task DisconnectAllClientsAsync_OneClientFailsToDisconnect_ContinuesWithOthers()
        {
            var client1 = CreateMockClient();
            var client2 = CreateMockClient();
            const string agentId1 = "agent1";
            const string agentId2 = "agent2";

            client1.CloseConnectionAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromException(new Exception("Disconnect failed")));

            await _organizer.AddAgentAsync(agentId1, client1, CancellationToken.None);
            await _organizer.AddAgentAsync(agentId2, client2, CancellationToken.None);

            await _organizer.DisconnectAllClientsAsync(CancellationToken.None);

            var connectedClients = _organizer.GetConnectedClients();
            connectedClients.Should().BeEmpty();
        }

        [Test]
        public async Task GetCameraSnapshotAsync_NonExistentAgent_ReturnsNull()
        {
            const string agentId = "nonexistent";
            const long cameraId = 123;
            
            var result = await _organizer.GetCameraSnapshotAsync(agentId, cameraId, CancellationToken.None);
            
            result.Should().BeNull();
        }

        [Test]
        public async Task GetCameraSnapshotAsync_ExistingAgentResponds_ReturnsImageDownloadResponse()
        {
            var client = CreateMockClient();
            const string agentId = "agent1";
            const long cameraId = 456;
            var expectedResponse = new ImageDownloadResponse 
            { 
                Image = "base64imagedata",
                ResponseCode = "success",
                TransactionId = Guid.NewGuid()
            };

            client.TryGetAgentResponse<ImageDownloadResponse>(Arg.Any<Guid>(), out Arg.Any<ImageDownloadResponse>())
                .Returns(x =>
                {
                    x[1] = expectedResponse;
                    return true;
                });

            await _organizer.AddAgentAsync(agentId, client, CancellationToken.None);
            
            var result = await _organizer.GetCameraSnapshotAsync(agentId, cameraId, CancellationToken.None);
            
            result.Should().Be(expectedResponse);
            result.Image.Should().Be("base64imagedata");
            result.ResponseCode.Should().Be("success");
            await client.Received(1).SendGetImageRequestAsync(
                Arg.Any<Guid>(), 
                cameraId, 
                null, 
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task GetCameraSnapshotAsync_AgentDoesNotRespond_ReturnsNull()
        {
            var client = CreateMockClient();
            const string agentId = "agent1";
            const long cameraId = 789;

            client.TryGetAgentResponse<ImageDownloadResponse>(Arg.Any<Guid>(), out Arg.Any<ImageDownloadResponse>())
                .Returns(false);

            await _organizer.AddAgentAsync(agentId, client, CancellationToken.None);
            
            var result = await _organizer.GetCameraSnapshotAsync(agentId, cameraId, CancellationToken.None);
            
            result.Should().BeNull();
            await client.Received(1).SendGetImageRequestAsync(
                Arg.Any<Guid>(), 
                cameraId, 
                null, 
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task GetCameraSnapshotAsync_SendImageRequestThrows_PropagatesException()
        {
            var client = CreateMockClient();
            const string agentId = "agent1";
            const long cameraId = 101;
            var expectedException = new Exception("Send request failed");

            client.SendGetImageRequestAsync(Arg.Any<Guid>(), Arg.Any<long>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromException(expectedException));

            await _organizer.AddAgentAsync(agentId, client, CancellationToken.None);
            
            var act = async () => await _organizer.GetCameraSnapshotAsync(agentId, cameraId, CancellationToken.None);
            
            await act.Should().ThrowExactlyAsync<Exception>()
                .WithMessage("Send request failed");
        }

        [Test]
        public async Task GetCameraSnapshotAsync_ValidParameters_CallsCorrectMethods()
        {
            var client = CreateMockClient();
            const string agentId = "agent1";
            const long cameraId = 555;
            var transactionId = Guid.NewGuid();
            var expectedResponse = new ImageDownloadResponse 
            { 
                Image = "testimage",
                ResponseCode = "ok",
                TransactionId = transactionId
            };

            client.TryGetAgentResponse<ImageDownloadResponse>(Arg.Any<Guid>(), out Arg.Any<ImageDownloadResponse>())
                .Returns(x =>
                {
                    x[1] = expectedResponse;
                    return true;
                });

            await _organizer.AddAgentAsync(agentId, client, CancellationToken.None);
            
            var result = await _organizer.GetCameraSnapshotAsync(agentId, cameraId, CancellationToken.None);
            
            result.Should().NotBeNull();
            await client.Received(1).SendGetImageRequestAsync(
                Arg.Any<Guid>(), 
                cameraId, 
                null, 
                Arg.Any<CancellationToken>());
            
            client.Received().TryGetAgentResponse<ImageDownloadResponse>(
                Arg.Any<Guid>(), 
                out Arg.Any<ImageDownloadResponse>());
        }

        [Test]
        public async Task GetCameraSnapshotAsync_ResponseTimeout_ReturnsNull()
        {
            var client = CreateMockClient();
            const string agentId = "agent1";
            const long cameraId = 999;

            // Configure client to never return a response (simulating timeout)
            client.TryGetAgentResponse<ImageDownloadResponse>(Arg.Any<Guid>(), out Arg.Any<ImageDownloadResponse>())
                .Returns(false);

            await _organizer.AddAgentAsync(agentId, client, CancellationToken.None);
            
            var result = await _organizer.GetCameraSnapshotAsync(agentId, cameraId, CancellationToken.None);
            
            result.Should().BeNull();
        }

        [Test]
        public async Task GetCameraSnapshotAsync_CancellationRequested_ThrowsOperationCanceledException()
        {
            var client = CreateMockClient();
            const string agentId = "agent1";
            const long cameraId = 777;
            var cts = new CancellationTokenSource();

            // Cancel the token immediately
            cts.Cancel();

            await _organizer.AddAgentAsync(agentId, client, CancellationToken.None);
            
            var act = async () => await _organizer.GetCameraSnapshotAsync(agentId, cameraId, cts.Token);
            
            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        private static IOpenAlprWebsocketClient CreateMockClient()
        {
            var client = Substitute.For<IOpenAlprWebsocketClient>();
            client.CloseConnectionAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            return client;
        }
    }
} 