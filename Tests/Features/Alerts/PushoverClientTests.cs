using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Features.Alerts;
using System.Net;
using System.Text;
using Tests.TestHelpers;
namespace Tests.Features.Alerts
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class PushoverClientTests : TestBase
    {
        private PushoverClient _pushoverClient;

        private IHttpClientFactory _httpClientFactory;

        private IServiceProvider _serviceProvider;

        private TestHttpMessageHandler _httpMessageHandler;

        private HttpClient _httpClient;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _httpMessageHandler = new TestHttpMessageHandler();
            _httpClient = new HttpClient(_httpMessageHandler);
            _httpClientFactory = Substitute.For<IHttpClientFactory>();
            _httpClientFactory.CreateClient().Returns(_httpClient);
            
            // Set up service provider
            var services = new ServiceCollection();
            services.AddScoped(provider => UnitOfWork);
            services.AddLogging();
            _serviceProvider = services.BuildServiceProvider();
            
            _pushoverClient = new PushoverClient(_serviceProvider, _httpClientFactory);
        }

        [TearDown]
        public override void TearDown()
        {
            _httpClient?.Dispose();
            _httpMessageHandler?.Dispose();
            (_serviceProvider as IDisposable)?.Dispose();
            base.TearDown();
        }

        [Test]
        public async Task SendAlertAsync_WithDisabledClient_DoesNotSendAlert()
        {
            // Arrange
            var pushoverSettings = CreateTestPushoverSettings(isEnabled: false);
            await SeedPushoverSettingsAsync(pushoverSettings);
            await SeedAgentAsync();

            var alert = CreateTestAlertUpdateRequest();
            var cancellationToken = GetCancellationToken();

            // Act
            await _pushoverClient.SendAlertAsync(alert, cancellationToken);

            // Assert
            _httpMessageHandler.RequestsSent.Should().BeEmpty();
        }

        [Test]
        public async Task SendAlertAsync_WithNonUrgentAlertAndEveryPlateDisabled_DoesNotSendAlert()
        {
            // Arrange
            var pushoverSettings = CreateTestPushoverSettings(
                isEnabled: true, 
                sendEveryPlateEnabled: false);
            await SeedPushoverSettingsAsync(pushoverSettings);
            await SeedAgentAsync();

            var alert = CreateTestAlertUpdateRequest(isUrgent: false);
            var cancellationToken = GetCancellationToken();

            // Act
            await _pushoverClient.SendAlertAsync(alert, cancellationToken);

            // Assert
            _httpMessageHandler.RequestsSent.Should().BeEmpty();
        }

        [Test]
        public async Task SendAlertAsync_WithUrgentAlert_SendsAlert()
        {
            // Arrange
            var pushoverSettings = CreateTestPushoverSettings(
                isEnabled: true,
                sendEveryPlateEnabled: false);
            await SeedPushoverSettingsAsync(pushoverSettings);
            var agent = await SeedAgentAsync();

            var alert = CreateTestAlertUpdateRequest(isUrgent: true);
            var cancellationToken = GetCancellationToken();
            
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, "OK");

            // Act
            await _pushoverClient.SendAlertAsync(alert, cancellationToken);

            // Assert
            _httpMessageHandler.RequestsSent.Should().ContainSingle();
            var request = _httpMessageHandler.RequestsSent[0];
            request.RequestUri.AbsoluteUri.Should().Be("https://api.pushover.net/1/messages.json");
            request.Method.Should().Be(HttpMethod.Post);
            
            var content = request.Content;
            content.Should().Contain($"name=user\r\n\r\n{pushoverSettings.UserKey}");
            content.Should().Contain($"name=token\r\n\r\n{pushoverSettings.ApiToken}");
            content.Should().Contain($"name=message\r\n\r\n<b>{alert.PlateNumber}</b> {alert.Description}");
            content.Should().Contain($"name=url\r\n\r\n{agent.OpenAlprWebServerUrl}/plate/{alert.PlateId}");
            content.Should().Contain("name=title\r\n\r\nopenalpr alert");
        }

        [Test]
        public async Task SendAlertAsync_WithEveryPlateEnabled_SendsAlert()
        {
            // Arrange
            var pushoverSettings = CreateTestPushoverSettings(
                isEnabled: true,
                sendEveryPlateEnabled: true);
            await SeedPushoverSettingsAsync(pushoverSettings);
            await SeedAgentAsync();

            var alert = CreateTestAlertUpdateRequest(isUrgent: false);
            var cancellationToken = GetCancellationToken();
            
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, "OK");

            // Act
            await _pushoverClient.SendAlertAsync(alert, cancellationToken);

            // Assert
            _httpMessageHandler.RequestsSent.Should().ContainSingle();
        }

        [Test]
        public async Task SendAlertAsync_WithPlatePreviewEnabled_IncludesImageAttachment()
        {
            // Arrange
            var pushoverSettings = CreateTestPushoverSettings(
                isEnabled: true,
                sendPlatePreview: true);
            await SeedPushoverSettingsAsync(pushoverSettings);
            await SeedAgentAsync();

            var imageData = Encoding.UTF8.GetBytes("fake-jpeg-data");
            var alert = CreateTestAlertUpdateRequest(isUrgent: true, plateJpeg: imageData);
            var cancellationToken = GetCancellationToken();
            
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, "OK");

            // Act
            await _pushoverClient.SendAlertAsync(alert, cancellationToken);

            // Assert
            _httpMessageHandler.RequestsSent.Should().ContainSingle();
            var request = _httpMessageHandler.RequestsSent[0];
            var content = request.Content;
            content.Should().Contain("Content-Disposition: form-data; name=attachment; filename=attachment.jpg");
        }

        [Test]
        public async Task SendAlertAsync_WithPlatePreviewDisabled_DoesNotIncludeImageAttachment()
        {
            // Arrange
            var pushoverSettings = CreateTestPushoverSettings(
                isEnabled: true,
                sendPlatePreview: false);
            await SeedPushoverSettingsAsync(pushoverSettings);
            await SeedAgentAsync();

            var imageData = Encoding.UTF8.GetBytes("fake-jpeg-data");
            var alert = CreateTestAlertUpdateRequest(isUrgent: true, plateJpeg: imageData);
            var cancellationToken = GetCancellationToken();
            
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, "OK");

            // Act
            await _pushoverClient.SendAlertAsync(alert, cancellationToken);

            // Assert
            _httpMessageHandler.RequestsSent.Should().ContainSingle();
            var request = _httpMessageHandler.RequestsSent[0];
            var content = request.Content;
            content.Should().NotContain("attachment");
        }

        [Test]
        public async Task SendAlertAsync_WithHttpError_ThrowsInvalidOperationException()
        {
            // Arrange
            var pushoverSettings = CreateTestPushoverSettings(isEnabled: true);
            await SeedPushoverSettingsAsync(pushoverSettings);
            await SeedAgentAsync();

            var alert = CreateTestAlertUpdateRequest(isUrgent: true);
            var cancellationToken = GetCancellationToken();
            
            _httpMessageHandler.SetupResponse(HttpStatusCode.BadRequest, "Error response");

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(() =>
                _pushoverClient.SendAlertAsync(alert, cancellationToken));
            exception.Message.Should().Be("failed");
        }

        [Test]
        public async Task SendAlertAsync_WithHttpClientException_ThrowsInvalidOperationException()
        {
            // Arrange
            var pushoverSettings = CreateTestPushoverSettings(isEnabled: true);
            await SeedPushoverSettingsAsync(pushoverSettings);
            await SeedAgentAsync();

            var alert = CreateTestAlertUpdateRequest(isUrgent: true);
            var cancellationToken = GetCancellationToken();
            
            _httpMessageHandler.SetupException(new HttpRequestException("Network error"));

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(() =>
                _pushoverClient.SendAlertAsync(alert, cancellationToken));
            exception.Message.Should().Be("failed");
        }

        [Test]
        public async Task SendAlertAsync_WithNoClientSettings_DoesNotSendAlert()
        {
            // Arrange
            await SeedAgentAsync();
            var alert = CreateTestAlertUpdateRequest(isUrgent: true);
            var cancellationToken = GetCancellationToken();

            // Act
            await _pushoverClient.SendAlertAsync(alert, cancellationToken);

            // Assert
            _httpMessageHandler.RequestsSent.Should().BeEmpty();
        }

        [Test]
        public async Task ShouldSendAllPlatesAsync_WithNoSettings_ReturnsFalse()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _pushoverClient.ShouldSendAllPlatesAsync(cancellationToken);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task ShouldSendAllPlatesAsync_WithSendEveryPlateEnabled_ReturnsTrue()
        {
            // Arrange
            var pushoverSettings = CreateTestPushoverSettings(sendEveryPlateEnabled: true);
            await SeedPushoverSettingsAsync(pushoverSettings);
            
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _pushoverClient.ShouldSendAllPlatesAsync(cancellationToken);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task ShouldSendAllPlatesAsync_WithSendEveryPlateDisabled_ReturnsFalse()
        {
            // Arrange
            var pushoverSettings = CreateTestPushoverSettings(sendEveryPlateEnabled: false);
            await SeedPushoverSettingsAsync(pushoverSettings);
            
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _pushoverClient.ShouldSendAllPlatesAsync(cancellationToken);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task VerifyCredentialsAsync_WithValidCredentials_SendsVerificationRequest()
        {
            // Arrange
            var pushoverSettings = CreateTestPushoverSettings(
                apiToken: "test-api-token",
                userKey: "test-user-key");
            await SeedPushoverSettingsAsync(pushoverSettings);
            
            var cancellationToken = GetCancellationToken();
            
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, "OK");

            // Act
            await _pushoverClient.VerifyCredentialsAsync(cancellationToken);

            // Assert
            _httpMessageHandler.RequestsSent.Should().ContainSingle();
            var request = _httpMessageHandler.RequestsSent[0];
            request.RequestUri.AbsoluteUri.Should().Be($"https://api.pushover.net/1/users/validate.json?token=test-api-token&user=test-user-key");
            request.Method.Should().Be(HttpMethod.Post);
        }

        [Test]
        public async Task VerifyCredentialsAsync_WithInvalidCredentials_DoesNotThrow()
        {
            // Arrange
            var pushoverSettings = CreateTestPushoverSettings();
            await SeedPushoverSettingsAsync(pushoverSettings);
            
            var cancellationToken = GetCancellationToken();
            
            _httpMessageHandler.SetupResponse(HttpStatusCode.BadRequest, "Invalid credentials");

            // Act & Assert
            Assert.DoesNotThrowAsync(() => _pushoverClient.VerifyCredentialsAsync(cancellationToken));
        }

        [Test]
        public async Task VerifyCredentialsAsync_WithHttpException_DoesNotThrow()
        {
            // Arrange
            var pushoverSettings = CreateTestPushoverSettings();
            await SeedPushoverSettingsAsync(pushoverSettings);
            
            var cancellationToken = GetCancellationToken();
            
            _httpMessageHandler.SetupException(new HttpRequestException("Network error"));

            // Act & Assert
            Assert.DoesNotThrowAsync(() => _pushoverClient.VerifyCredentialsAsync(cancellationToken));
        }

        [Test]
        public async Task VerifyCredentialsAsync_WithNoClientSettings_DoesNotSendRequest()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            // Act
            await _pushoverClient.VerifyCredentialsAsync(cancellationToken);

            // Assert
            _httpMessageHandler.RequestsSent.Should().BeEmpty();
        }

        private static Pushover CreateTestPushoverSettings(
            bool isEnabled = true,
            string apiToken = "test-api-token",
            string userKey = "test-user-key",
            bool sendPlatePreview = false,
            bool sendEveryPlateEnabled = false)
        {
            return new Pushover
            {
                Id = Guid.NewGuid(),
                IsEnabled = isEnabled,
                ApiToken = apiToken,
                UserKey = userKey,
                SendPlatePreview = sendPlatePreview,
                SendEveryPlateEnabled = sendEveryPlateEnabled
            };
        }

        private async Task SeedPushoverSettingsAsync(Pushover pushoverSettings)
        {
            Context.PushoverAlertClients.Add(pushoverSettings);
            await Context.SaveChangesAsync();
        }

        private async ValueTask<Agent> SeedAgentAsync(string webServerUrl = "https://test.example.com")
        {
            var agent = new Agent
            {
                Id = Guid.NewGuid(),
                OpenAlprWebServerUrl = webServerUrl,
                Uid = "test-agent-uid"
            };
            
            Context.Agents.Add(agent);
            await Context.SaveChangesAsync();
            return agent;
        }

        private static AlertUpdateRequest CreateTestAlertUpdateRequest(
            bool isUrgent = false,
            string plateNumber = "ABC123",
            string description = "Test alert description",
            byte[] plateJpeg = null)
        {
            return new AlertUpdateRequest
            {
                PlateId = Guid.NewGuid(),
                PlateNumber = plateNumber,
                Description = description,
                IsUrgent = isUrgent,
                PlateJpeg = plateJpeg,
                ReceivedOn = DateTimeOffset.UtcNow
            };
        }
    }

    public class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly List<RequestInfo> _requestsSent = new();

        private HttpResponseMessage _response = new(HttpStatusCode.OK);

        private Exception _exception;

        public IReadOnlyList<RequestInfo> RequestsSent => _requestsSent.AsReadOnly();

        public void SetupResponse(HttpStatusCode statusCode, string content)
        {
            _response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content)
            };
            _exception = null;
        }

        public void SetupException(Exception exception)
        {
            _exception = exception;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, 
            CancellationToken cancellationToken = default)
        {
            // Store request information for verification without interfering with content
            var requestInfo = new RequestInfo
            {
                Method = request.Method,
                RequestUri = request.RequestUri,
                Content = request.Content != null ? await request.Content.ReadAsStringAsync() : null
            };
            
            _requestsSent.Add(requestInfo);

            if (_exception != null)
            {
                throw _exception;
            }

            return _response;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _response?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public class RequestInfo
    {
        public HttpMethod Method { get; set; }

        public Uri RequestUri { get; set; }

        public string Content { get; set; }
    }
}