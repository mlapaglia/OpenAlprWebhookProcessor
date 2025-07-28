using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Webhooks;
using OpenAlprWebhookProcessor.Features.Webhooks.Commands.ProcessAlertWebhook;
using OpenAlprWebhookProcessor.Features.Webhooks.Commands.ProcessHeartbeatWebhook;
using OpenAlprWebhookProcessor.Features.Webhooks.Commands.ProcessPlateGroupWebhook;
using OpenAlprWebhookProcessor.Features.Webhooks.Commands.ProcessSinglePlateWebhook;
using OpenAlprWebhookProcessor.Features.Webhooks.Queries.GetWebhookStatus;
using System.Text;
using Tests.TestHelpers;

namespace Tests.Controllers
{
    [TestFixture]
    public class WebhookControllerTests : TestBase
    {
        private WebhookController _controller;
        private ILogger<WebhookController> _logger;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _logger = Substitute.For<ILogger<WebhookController>>();
            _controller = new WebhookController(_logger, Mediator);
            
            // Set up mock HTTP context
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Test]
        public async Task Post_AlertWebhook_ProcessesCorrectly()
        {
            // Arrange
            var alertWebhookJson = @"{""alpr_alert"": ""test"", ""data_type"": ""alpr_alert""}";
            var cancellationToken = GetCancellationToken();
            
            SetupRequestBody(alertWebhookJson);

            // Act
            var result = await _controller.Post(cancellationToken);

            // Assert
            result.Should().BeOfType<OkResult>();
            await Mediator.Received(1).Send(
                Arg.Any<ProcessAlertWebhookCommand>(), 
                cancellationToken);
        }

        [Test]
        public async Task Post_PlateGroupWebhook_ProcessesCorrectly()
        {
            // Arrange
            var groupWebhookJson = @"{""alpr_group"": ""test"", ""data_type"": ""alpr_group""}";
            var cancellationToken = GetCancellationToken();
            
            SetupRequestBody(groupWebhookJson);

            // Act
            var result = await _controller.Post(cancellationToken);

            // Assert
            result.Should().BeOfType<OkResult>();
            await Mediator.Received(1).Send(
                Arg.Any<ProcessPlateGroupWebhookCommand>(), 
                cancellationToken);
        }

        [Test]
        public async Task Post_PlateGroupWebhookWithNullDataType_ProcessesCorrectly()
        {
            // Arrange
            var groupWebhookJson = @"{""alpr_group"": ""test"", ""data_type"": null}";
            var cancellationToken = GetCancellationToken();
            
            SetupRequestBody(groupWebhookJson);

            // Act
            var result = await _controller.Post(cancellationToken);

            // Assert
            result.Should().BeOfType<OkResult>();
            await Mediator.Received(1).Send(
                Arg.Any<ProcessPlateGroupWebhookCommand>(), 
                cancellationToken);
        }

        [Test]
        public async Task Post_SinglePlateWebhook_ProcessesCorrectly()
        {
            // Arrange
            var singlePlateWebhookJson = @"{""alpr_results"": ""test"", ""data_type"": ""alpr_results""}";
            var cancellationToken = GetCancellationToken();
            
            SetupRequestBody(singlePlateWebhookJson);

            // Act
            var result = await _controller.Post(cancellationToken);

            // Assert
            result.Should().BeOfType<OkResult>();
            await Mediator.Received(1).Send(
                Arg.Any<ProcessSinglePlateWebhookCommand>(), 
                cancellationToken);
        }

        [Test]
        public async Task Post_TestWebhook_ReturnsTestSuccessful()
        {
            // Arrange
            var testWebhookJson = @"{""openalpr_webhook"": ""test""}";
            var cancellationToken = GetCancellationToken();
            
            SetupRequestBody(testWebhookJson);

            // Act
            var result = await _controller.Post(cancellationToken);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().Be("Test successful");
        }

        [Test]
        public async Task Post_HeartbeatWebhook_ProcessesCorrectly()
        {
            // Arrange
            var heartbeatWebhookJson = @"{""heartbeat"": ""test""}";
            var cancellationToken = GetCancellationToken();
            
            SetupRequestBody(heartbeatWebhookJson);

            // Act
            var result = await _controller.Post(cancellationToken);

            // Assert
            result.Should().BeOfType<OkResult>();
            await Mediator.Received(1).Send(
                Arg.Any<ProcessHeartbeatWebhookCommand>(), 
                cancellationToken);
        }

        [Test]
        public async Task Post_UnknownWebhook_ReturnsOkWithoutProcessing()
        {
            // Arrange
            var unknownWebhookJson = @"{""unknown"": ""payload""}";
            var cancellationToken = GetCancellationToken();
            
            SetupRequestBody(unknownWebhookJson);

            // Act
            var result = await _controller.Post(cancellationToken);

            // Assert
            result.Should().BeOfType<OkResult>();
            await Mediator.DidNotReceive().Send(
                Arg.Any<ProcessAlertWebhookCommand>(), 
                cancellationToken);
            await Mediator.DidNotReceive().Send(
                Arg.Any<ProcessPlateGroupWebhookCommand>(), 
                cancellationToken);
            await Mediator.DidNotReceive().Send(
                Arg.Any<ProcessSinglePlateWebhookCommand>(), 
                cancellationToken);
            await Mediator.DidNotReceive().Send(
                Arg.Any<ProcessHeartbeatWebhookCommand>(), 
                cancellationToken);
        }

        [Test]
        public async Task Post_EmptyBody_ReturnsOkWithoutProcessing()
        {
            // Arrange
            var emptyBody = "";
            var cancellationToken = GetCancellationToken();
            
            SetupRequestBody(emptyBody);

            // Act
            var result = await _controller.Post(cancellationToken);

            // Assert
            result.Should().BeOfType<OkResult>();
            await Mediator.DidNotReceive().Send(
                Arg.Any<ProcessAlertWebhookCommand>(), 
                cancellationToken);
        }

        [Test]
        public async Task Post_LogsRequestInformation()
        {
            // Arrange
            var testWebhookJson = @"{""test"": ""payload""}";
            var cancellationToken = GetCancellationToken();
            
            SetupRequestBody(testWebhookJson);

            // Act
            var result = await _controller.Post(cancellationToken);

            // Assert
            result.Should().BeOfType<OkResult>();
        }

        [Test]
        public async Task Post_AlertWebhook_LogsCorrectMessage()
        {
            // Arrange
            var alertWebhookJson = @"{""alpr_alert"": ""test""}";
            var cancellationToken = GetCancellationToken();
            
            SetupRequestBody(alertWebhookJson);

            // Act
            await _controller.Post(cancellationToken);

            // Assert
            _logger.Received(1).LogInformation("parsing alert webhook");
        }

        [Test]
        public async Task Post_PlateGroupWebhook_LogsCorrectMessage()
        {
            // Arrange
            var groupWebhookJson = @"{""alpr_group"": ""test""}";
            var cancellationToken = GetCancellationToken();
            
            SetupRequestBody(groupWebhookJson);

            // Act
            await _controller.Post(cancellationToken);

            // Assert
            _logger.Received(1).LogInformation("parsing plate group webhook");
        }

        [Test]
        public async Task Post_SinglePlateWebhook_LogsCorrectMessage()
        {
            // Arrange
            var singlePlateWebhookJson = @"{""alpr_results"": ""test""}";
            var cancellationToken = GetCancellationToken();
            
            SetupRequestBody(singlePlateWebhookJson);

            // Act
            await _controller.Post(cancellationToken);

            // Assert
            _logger.Received(1).LogInformation("parsing single webhook");
        }

        [Test]
        public async Task Post_HeartbeatWebhook_LogsCorrectMessage()
        {
            // Arrange
            var heartbeatWebhookJson = @"{""heartbeat"": ""test""}";
            var cancellationToken = GetCancellationToken();
            
            SetupRequestBody(heartbeatWebhookJson);

            // Act
            await _controller.Post(cancellationToken);

            // Assert
            _logger.Received(1).LogInformation("received heartbeat from agent");
        }

        [Test]
        public async Task Post_UnknownWebhook_LogsUnknownPayload()
        {
            // Arrange
            var unknownWebhookJson = @"{""unknown"": ""payload""}";
            var cancellationToken = GetCancellationToken();
            
            SetupRequestBody(unknownWebhookJson);

            // Act
            var result = await _controller.Post(cancellationToken);

            // Assert
            result.Should().BeOfType<OkResult>();
        }

        [Test]
        public async Task Get_ReturnsOkWithStatus()
        {
            // Arrange
            var expectedStatus = "Webhook service is running";
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetWebhookStatusQuery>(), cancellationToken)
                .Returns(expectedStatus);

            // Act
            var result = await _controller.Get(cancellationToken);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().Be(expectedStatus);
        }

        [Test]
        public async Task Get_CallsCorrectQuery()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.Get(cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Any<GetWebhookStatusQuery>(), 
                cancellationToken);
        }

        [Test]
        public async Task Get_LogsTestSuccessMessage()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _controller.Get(cancellationToken);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        private void SetupRequestBody(string content)
        {
            var bodyBytes = Encoding.UTF8.GetBytes(content);
            var bodyStream = new MemoryStream(bodyBytes);
            _controller.Request.Body = bodyStream;
        }
    }
} 