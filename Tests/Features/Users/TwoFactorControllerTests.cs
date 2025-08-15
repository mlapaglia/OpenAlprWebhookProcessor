using AwesomeAssertions;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetTwoFactorStatus;
using OpenAlprWebhookProcessor.Features.Users.Commands.SetupTwoFactor;
using OpenAlprWebhookProcessor.Features.Users.Commands.EnableTwoFactor;
using OpenAlprWebhookProcessor.Features.Users.Commands.DisableTwoFactor;
using OpenAlprWebhookProcessor.Features.Users.Commands.GetRecoveryCodes;
using System.Security.Claims;
using Tests.TestHelpers;

namespace Tests.Features.Users
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class TwoFactorControllerTests : TestBase
    {
        private TwoFactorController _controller;
        private IMediator _mediator;
        private ClaimsPrincipal _user;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _mediator = Substitute.For<IMediator>();
            _controller = new TwoFactorController(_mediator);
            
            // Setup controller context for User property
            _user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.NameIdentifier, "123")
            }, "test"));
            
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = _user }
            };
        }

        [Test]
        public async Task GetStatus_WithValidUser_ReturnsOkResult()
        {
            // Arrange
            var expectedResponse = new TwoFactorStatusResponse(false, false);
            
            _mediator.Send(Arg.Any<GetTwoFactorStatusQuery>(), Arg.Any<CancellationToken>())
                .Returns(expectedResponse);

            // Act
            var result = await _controller.GetStatus(CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().Be(expectedResponse);
            
            await _mediator.Received(1).Send(
                Arg.Is<GetTwoFactorStatusQuery>(query => query.User == _user), 
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task GetStatus_WithError_ReturnsBadRequest()
        {
            // Arrange
            _mediator.Send(Arg.Any<GetTwoFactorStatusQuery>(), Arg.Any<CancellationToken>())
                .Returns<ValueTask<TwoFactorStatusResponse>>(x => throw new AppException("User not found"));

            // Act
            var result = await _controller.GetStatus(CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = (BadRequestObjectResult)result;
            var response = badRequestResult.Value;
            response.Should().BeEquivalentTo(new { message = "User not found" });
        }

        [Test]
        public async Task Setup_WithValidUser_ReturnsOkResult()
        {
            // Arrange
            var expectedResponse = new SetupTwoFactorResponse("ABCDEFGHIJKLMNOP", "otpauth://totp/TestApp:testuser?secret=ABCDEFGHIJKLMNOP&issuer=TestApp");
            
            _mediator.Send(Arg.Any<SetupTwoFactorCommand>(), Arg.Any<CancellationToken>())
                .Returns(expectedResponse);

            // Act
            var result = await _controller.Setup(CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().Be(expectedResponse);
            
            await _mediator.Received(1).Send(
                Arg.Is<SetupTwoFactorCommand>(cmd => cmd.User == _user), 
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Setup_WithError_ReturnsBadRequest()
        {
            // Arrange
            _mediator.Send(Arg.Any<SetupTwoFactorCommand>(), Arg.Any<CancellationToken>())
                .Returns<ValueTask<SetupTwoFactorResponse>>(x => throw new AppException("Two-factor authentication is already enabled"));

            // Act
            var result = await _controller.Setup(CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = (BadRequestObjectResult)result;
            var response = badRequestResult.Value;
            response.Should().BeEquivalentTo(new { message = "Two-factor authentication is already enabled" });
        }

        [Test]
        public async Task Enable_WithValidCode_ReturnsOkResult()
        {
            // Arrange
            var request = new EnableTwoFactorRequest { Code = "123456" };
            var expectedResponse = new EnableTwoFactorResponse("Two-factor authentication enabled", new[] { "ABC123", "DEF456", "GHI789" });
            
            _mediator.Send(Arg.Any<EnableTwoFactorCommand>(), Arg.Any<CancellationToken>())
                .Returns(expectedResponse);

            // Act
            var result = await _controller.Enable(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().Be(expectedResponse);
            
            await _mediator.Received(1).Send(
                Arg.Is<EnableTwoFactorCommand>(cmd => 
                    cmd.User == _user && cmd.Code == request.Code), 
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Enable_WithInvalidCode_ReturnsBadRequest()
        {
            // Arrange
            var request = new EnableTwoFactorRequest { Code = "000000" };
            
            _mediator.Send(Arg.Any<EnableTwoFactorCommand>(), Arg.Any<CancellationToken>())
                .Returns<ValueTask<EnableTwoFactorResponse>>(x => throw new AppException("Invalid verification code"));

            // Act
            var result = await _controller.Enable(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = (BadRequestObjectResult)result;
            var response = badRequestResult.Value;
            response.Should().BeEquivalentTo(new { message = "Invalid verification code" });
        }

        [Test]
        public async Task Disable_WithValidUser_ReturnsOkResult()
        {
            // Arrange
            _mediator.Send(Arg.Any<DisableTwoFactorCommand>(), Arg.Any<CancellationToken>())
                .Returns(Unit.Value);

            // Act
            var result = await _controller.Disable(CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            var response = okResult.Value;
            response.Should().BeEquivalentTo(new { message = "Two-factor authentication has been disabled" });
            
            await _mediator.Received(1).Send(
                Arg.Is<DisableTwoFactorCommand>(cmd => cmd.User == _user), 
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Disable_WithError_ReturnsBadRequest()
        {
            // Arrange
            _mediator.Send(Arg.Any<DisableTwoFactorCommand>(), Arg.Any<CancellationToken>())
                .Returns<ValueTask<Unit>>(x => throw new AppException("Two-factor authentication is not enabled"));

            // Act
            var result = await _controller.Disable(CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = (BadRequestObjectResult)result;
            var response = badRequestResult.Value;
            response.Should().BeEquivalentTo(new { message = "Two-factor authentication is not enabled" });
        }

        [Test]
        public async Task GetRecoveryCodes_WithValidUser_ReturnsOkResult()
        {
            // Arrange
            var expectedResponse = new GetRecoveryCodesResponse(new[] { "ABC123", "DEF456", "GHI789" });
            
            _mediator.Send(Arg.Any<GetRecoveryCodesCommand>(), Arg.Any<CancellationToken>())
                .Returns(expectedResponse);

            // Act
            var result = await _controller.GetRecoveryCodes(CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().Be(expectedResponse);
            
            await _mediator.Received(1).Send(
                Arg.Is<GetRecoveryCodesCommand>(cmd => cmd.User == _user), 
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task GetRecoveryCodes_WithError_ReturnsBadRequest()
        {
            // Arrange
            _mediator.Send(Arg.Any<GetRecoveryCodesCommand>(), Arg.Any<CancellationToken>())
                .Returns<ValueTask<GetRecoveryCodesResponse>>(x => throw new AppException("Two-factor authentication is not enabled"));

            // Act
            var result = await _controller.GetRecoveryCodes(CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = (BadRequestObjectResult)result;
            var response = badRequestResult.Value;
            response.Should().BeEquivalentTo(new { message = "Two-factor authentication is not enabled" });
        }

        [Test]
        public async Task GetStatus_WithCancellation_PassesCancellationToken()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            
            _mediator.Send(Arg.Any<GetTwoFactorStatusQuery>(), Arg.Any<CancellationToken>())
                .Returns(new TwoFactorStatusResponse(false, false));

            // Act
            await _controller.GetStatus(cancellationToken);

            // Assert
            await _mediator.Received(1).Send(
                Arg.Any<GetTwoFactorStatusQuery>(), 
                Arg.Is<CancellationToken>(ct => ct == cancellationToken));
        }

        [Test]
        public async Task Setup_WithCancellation_PassesCancellationToken()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            
            _mediator.Send(Arg.Any<SetupTwoFactorCommand>(), Arg.Any<CancellationToken>())
                .Returns(new SetupTwoFactorResponse("test-key", "test-uri"));

            // Act
            await _controller.Setup(cancellationToken);

            // Assert
            await _mediator.Received(1).Send(
                Arg.Any<SetupTwoFactorCommand>(), 
                Arg.Is<CancellationToken>(ct => ct == cancellationToken));
        }

        [Test]
        public async Task Enable_WithCancellation_PassesCancellationToken()
        {
            // Arrange
            var request = new EnableTwoFactorRequest { Code = "123456" };
            using var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            
            _mediator.Send(Arg.Any<EnableTwoFactorCommand>(), Arg.Any<CancellationToken>())
                .Returns(new EnableTwoFactorResponse("Enabled", new string[0]));

            // Act
            await _controller.Enable(request, cancellationToken);

            // Assert
            await _mediator.Received(1).Send(
                Arg.Any<EnableTwoFactorCommand>(), 
                Arg.Is<CancellationToken>(ct => ct == cancellationToken));
        }
    }
}