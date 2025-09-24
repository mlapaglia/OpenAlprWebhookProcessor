using AwesomeAssertions;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users;
using OpenAlprWebhookProcessor.Features.Users.Commands.Authenticate;
using OpenAlprWebhookProcessor.Features.Users.Commands.VerifyTwoFactor;
using OpenAlprWebhookProcessor.Features.Users.Commands.RegisterUser;
using OpenAlprWebhookProcessor.Features.Users.Commands.Logout;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetCurrentUser;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetAllUsers;
using OpenAlprWebhookProcessor.Features.Users.Queries.CanRegister;
using OpenAlprWebhookProcessor.Features.Users.Commands.RegisterPasskey;
using OpenAlprWebhookProcessor.Features.Users.Commands.CompletePasskeyRegistration;
using OpenAlprWebhookProcessor.Features.Users.Commands.AuthenticatePasskey;
using OpenAlprWebhookProcessor.Features.Users.Commands.CompletePasskeyAuthentication;
using OpenAlprWebhookProcessor.Features.Users.Commands.DeletePasskey;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetUserPasskeys;
using System.Security.Claims;
using Tests.TestHelpers;
using Fido2NetLib;

namespace Tests.Features.Users
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class AuthControllerTests : TestBase
    {
        private AuthController _controller;
        private IMediator _mediator;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _mediator = Substitute.For<IMediator>();
            _controller = new AuthController(_mediator);
            
            // Setup controller context for User property
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.NameIdentifier, "123")
            }));
            
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Test]
        public async Task Authenticate_WithValidCredentials_ReturnsOkResult()
        {
            // Arrange
            var request = new AuthenticateRequest 
            { 
                Username = "testuser", 
                Password = "password123", 
                RememberMe = false 
            };
            
            var expectedResponse = new UserDto
            {
                Username = "testuser",
                FirstName = "Test",
                LastName = "User"
            };
            
            _mediator.Send(Arg.Any<AuthenticateCommand>(), Arg.Any<CancellationToken>())
                .Returns(expectedResponse);

            // Act
            var result = await _controller.Authenticate(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().Be(expectedResponse);
            
            await _mediator.Received(1).Send(
                Arg.Is<AuthenticateCommand>(cmd => 
                    cmd.Username == request.Username && 
                    cmd.Password == request.Password && 
                    cmd.RememberMe == request.RememberMe), 
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Authenticate_WithInvalidCredentials_ReturnsBadRequest()
        {
            // Arrange
            var request = new AuthenticateRequest 
            { 
                Username = "testuser", 
                Password = "wrongpassword", 
                RememberMe = false 
            };
            
            _mediator.Send(Arg.Any<AuthenticateCommand>(), Arg.Any<CancellationToken>())
                .Returns<ValueTask<UserDto>>(x => throw new AppException("Username or password is incorrect"));

            // Act
            var result = await _controller.Authenticate(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = (BadRequestObjectResult)result;
            var response = badRequestResult.Value;
            response.Should().BeEquivalentTo(new { message = "Username or password is incorrect" });
        }

        [Test]
        public async Task VerifyTwoFactor_WithValidCode_ReturnsOkResult()
        {
            // Arrange
            var request = new Verify2FARequest 
            { 
                UserId = "123", 
                Code = "123456", 
                RememberMe = false 
            };
            
            var expectedResponse = new AuthenticateResponse
            {
                Success = true,
                Username = "testuser",
                FirstName = "Test",
                LastName = "User"
            };
            
            _mediator.Send(Arg.Any<VerifyTwoFactorCommand>(), Arg.Any<CancellationToken>())
                .Returns(expectedResponse);

            // Act
            var result = await _controller.VerifyTwoFactor(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().Be(expectedResponse);
            
            await _mediator.Received(1).Send(
                Arg.Is<VerifyTwoFactorCommand>(cmd => 
                    cmd.UserId == request.UserId && 
                    cmd.Code == request.Code && 
                    cmd.RememberMe == request.RememberMe), 
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task VerifyTwoFactor_WithInvalidCode_ReturnsBadRequest()
        {
            // Arrange
            var request = new Verify2FARequest 
            { 
                UserId = "123", 
                Code = "000000", 
                RememberMe = false 
            };
            
            _mediator.Send(Arg.Any<VerifyTwoFactorCommand>(), Arg.Any<CancellationToken>())
                .Returns<ValueTask<AuthenticateResponse>>(x => throw new AppException("Invalid verification code"));

            // Act
            var result = await _controller.VerifyTwoFactor(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = (BadRequestObjectResult)result;
            var response = badRequestResult.Value;
            response.Should().BeEquivalentTo(new { message = "Invalid verification code" });
        }

        [Test]
        public async Task Register_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var request = new RegisterRequest 
            { 
                Username = "newuser", 
                Password = "password123", 
                FirstName = "New",
                LastName = "User"
            };
            
            // Mock canRegister to return true (registration allowed)
            _mediator.Send(Arg.Any<CanRegisterQuery>(), Arg.Any<CancellationToken>())
                .Returns(true);
                
            _mediator.Send(Arg.Any<RegisterUserCommand>(), Arg.Any<CancellationToken>())
                .Returns(Unit.Value);

            // Act
            var result = await _controller.Register(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            var response = okResult.Value;
            response.Should().BeEquivalentTo(new { message = "Registration successful" });
            
            await _mediator.Received(1).Send(
                Arg.Is<CanRegisterQuery>(query => true), 
                Arg.Any<CancellationToken>());
                
            await _mediator.Received(1).Send(
                Arg.Is<RegisterUserCommand>(cmd => 
                    cmd.Username == request.Username && 
                    cmd.Password == request.Password && 
                    cmd.FirstName == request.FirstName &&
                    cmd.LastName == request.LastName), 
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Register_WithExistingUsername_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterRequest 
            { 
                Username = "existinguser", 
                Password = "password123", 
                FirstName = "New",
                LastName = "User"
            };
            
            // Mock canRegister to return true (registration allowed)
            _mediator.Send(Arg.Any<CanRegisterQuery>(), Arg.Any<CancellationToken>())
                .Returns(true);
                
            _mediator.Send(Arg.Any<RegisterUserCommand>(), Arg.Any<CancellationToken>())
                .Returns<ValueTask<Unit>>(x => throw new AppException("Username already exists"));

            // Act
            var result = await _controller.Register(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = (BadRequestObjectResult)result;
            var response = badRequestResult.Value;
            response.Should().BeEquivalentTo(new { message = "Username already exists" });
        }

        [Test]
        public async Task Register_WhenRegistrationNotAllowed_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterRequest 
            { 
                Username = "newuser", 
                Password = "password123", 
                FirstName = "New",
                LastName = "User"
            };
            
            // Mock canRegister to return false (registration not allowed)
            _mediator.Send(Arg.Any<CanRegisterQuery>(), Arg.Any<CancellationToken>())
                .Returns(false);

            // Act
            var result = await _controller.Register(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = (BadRequestObjectResult)result;
            var response = badRequestResult.Value;
            response.Should().BeEquivalentTo(new { message = "Registration is not allowed. A user already exists." });
            
            // Verify that RegisterUserCommand was never called
            await _mediator.DidNotReceive().Send(
                Arg.Any<RegisterUserCommand>(), 
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Logout_ReturnsOkResult()
        {
            // Arrange
            _mediator.Send(Arg.Any<LogoutCommand>(), Arg.Any<CancellationToken>())
                .Returns(Unit.Value);

            // Act
            var result = await _controller.Logout(CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            var response = okResult.Value;
            response.Should().BeEquivalentTo(new { message = "Logged out successfully" });
            
            await _mediator.Received(1).Send(
                Arg.Any<LogoutCommand>(), 
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task GetCurrentUser_WithValidUser_ReturnsOkResult()
        {
            // Arrange
            var expectedUser = new UserDto
            {
                Id = 123,
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                TwoFactorEnabled = false
            };
            
            _mediator.Send(Arg.Any<GetCurrentUserQuery>(), Arg.Any<CancellationToken>())
                .Returns(expectedUser);

            // Act
            var result = await _controller.GetCurrentUser(CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().Be(expectedUser);
            
            await _mediator.Received(1).Send(
                Arg.Is<GetCurrentUserQuery>(query => query.User == _controller.User), 
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task GetCurrentUser_WithNoUser_ReturnsUnauthorized()
        {
            // Arrange
            _mediator.Send(Arg.Any<GetCurrentUserQuery>(), Arg.Any<CancellationToken>())
                .Returns((UserDto)null);

            // Act
            var result = await _controller.GetCurrentUser(CancellationToken.None);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Test]
        public async Task Authenticate_WithCancellation_PassesCancellationToken()
        {
            // Arrange
            var request = new AuthenticateRequest 
            { 
                Username = "testuser", 
                Password = "password123", 
                RememberMe = false 
            };
            
            using var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            
            _mediator.Send(Arg.Any<AuthenticateCommand>(), Arg.Any<CancellationToken>())
                .Returns(new UserDto { Id = 123 });

            // Act
            await _controller.Authenticate(request, cancellationToken);

            // Assert
            await _mediator.Received(1).Send(
                Arg.Any<AuthenticateCommand>(), 
                Arg.Is<CancellationToken>(ct => ct == cancellationToken));
        }

        [Test]
        public async Task RegisterPasskey_WithValidRequest_ReturnsOkResult()
        {
            // Arrange
            var request = new RegisterPasskeyRequest("MyPasskey");
            var expectedResponse = new RegisterPasskeyResponse(new CredentialCreateOptions
            {
                Rp = new PublicKeyCredentialRpEntity("test.com", "Test", null),
                User = new Fido2User { DisplayName = "Test User", Name = "testuser", Id = new byte[] { 1 } },
                Challenge = new byte[] { 1, 2, 3, 4, 5 },
                PubKeyCredParams = new List<PubKeyCredParam>()
            });
            
            _mediator.Send(Arg.Any<RegisterPasskeyCommand>(), Arg.Any<CancellationToken>())
                .Returns(expectedResponse);

            // Act
            var result = await _controller.RegisterPasskey(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().Be(expectedResponse);
            
            await _mediator.Received(1).Send(
                Arg.Is<RegisterPasskeyCommand>(cmd => 
                    cmd.User == _controller.User && 
                    cmd.Name == request.Name), 
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task RegisterPasskey_WithException_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterPasskeyRequest("MyPasskey");
            
            _mediator.Send(Arg.Any<RegisterPasskeyCommand>(), Arg.Any<CancellationToken>())
                .Returns<ValueTask<RegisterPasskeyResponse>>(x => throw new AppException("Passkey registration failed"));

            // Act
            var result = await _controller.RegisterPasskey(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = (BadRequestObjectResult)result;
            var response = badRequestResult.Value;
            response.Should().BeEquivalentTo(new { message = "Passkey registration failed" });
        }

        [Test]
        public async Task CompletePasskeyRegistration_WithValidRequest_ReturnsOkResult()
        {
            // Arrange
            var request = new CompletePasskeyRegistrationRequest("attestationResponse", "MyPasskey");
            var expectedResponse = new CompletePasskeyRegistrationResponse("Passkey registered successfully", true);
            
            _mediator.Send(Arg.Any<CompletePasskeyRegistrationCommand>(), Arg.Any<CancellationToken>())
                .Returns(expectedResponse);

            // Act
            var result = await _controller.CompletePasskeyRegistration(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().Be(expectedResponse);
            
            await _mediator.Received(1).Send(
                Arg.Is<CompletePasskeyRegistrationCommand>(cmd => 
                    cmd.User == _controller.User && 
                    cmd.AttestationResponse == request.AttestationResponse &&
                    cmd.Name == request.Name), 
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task CompletePasskeyRegistration_WithException_ReturnsBadRequest()
        {
            // Arrange
            var request = new CompletePasskeyRegistrationRequest("attestationResponse", "MyPasskey");
            
            _mediator.Send(Arg.Any<CompletePasskeyRegistrationCommand>(), Arg.Any<CancellationToken>())
                .Returns<ValueTask<CompletePasskeyRegistrationResponse>>(x => throw new AppException("Registration failed"));

            // Act
            var result = await _controller.CompletePasskeyRegistration(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = (BadRequestObjectResult)result;
            var response = badRequestResult.Value;
            response.Should().BeEquivalentTo(new { message = "Registration failed" });
        }

        [Test]
        public async Task AuthenticatePasskey_WithValidRequest_ReturnsOkResult()
        {
            // Arrange
            var request = new AuthenticatePasskeyRequest("testuser");
            var expectedResponse = new AuthenticatePasskeyResponse(new AssertionOptions());
            
            _mediator.Send(Arg.Any<AuthenticatePasskeyCommand>(), Arg.Any<CancellationToken>())
                .Returns(expectedResponse);

            // Act
            var result = await _controller.AuthenticatePasskey(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().Be(expectedResponse);
            
            await _mediator.Received(1).Send(
                Arg.Is<AuthenticatePasskeyCommand>(cmd => 
                    cmd.Username == request.Username), 
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task AuthenticatePasskey_WithException_ReturnsBadRequest()
        {
            // Arrange
            var request = new AuthenticatePasskeyRequest("testuser");
            
            _mediator.Send(Arg.Any<AuthenticatePasskeyCommand>(), Arg.Any<CancellationToken>())
                .Returns<ValueTask<AuthenticatePasskeyResponse>>(x => throw new AppException("Authentication failed"));

            // Act
            var result = await _controller.AuthenticatePasskey(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = (BadRequestObjectResult)result;
            var response = badRequestResult.Value;
            response.Should().BeEquivalentTo(new { message = "Authentication failed" });
        }

        [Test]
        public async Task CompletePasskeyAuthentication_WithValidRequest_ReturnsOkResult()
        {
            // Arrange
            var request = new CompletePasskeyAuthenticationRequest("testuser", "assertionResponse", true);
            var expectedResponse = new UserDto
            {
                Id = 123,
                Username = "testuser",
                FirstName = "Test",
                LastName = "User"
            };
            
            _mediator.Send(Arg.Any<CompletePasskeyAuthenticationCommand>(), Arg.Any<CancellationToken>())
                .Returns(expectedResponse);

            // Act
            var result = await _controller.CompletePasskeyAuthentication(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().Be(expectedResponse);
            
            await _mediator.Received(1).Send(
                Arg.Is<CompletePasskeyAuthenticationCommand>(cmd => 
                    cmd.Username == request.Username && 
                    cmd.AssertionResponse == request.AssertionResponse &&
                    cmd.RememberMe == request.RememberMe), 
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task CompletePasskeyAuthentication_WithException_ReturnsBadRequest()
        {
            // Arrange
            var request = new CompletePasskeyAuthenticationRequest("testuser", "assertionResponse", false);
            
            _mediator.Send(Arg.Any<CompletePasskeyAuthenticationCommand>(), Arg.Any<CancellationToken>())
                .Returns<ValueTask<UserDto>>(x => throw new AppException("Authentication failed"));

            // Act
            var result = await _controller.CompletePasskeyAuthentication(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = (BadRequestObjectResult)result;
            var response = badRequestResult.Value;
            response.Should().BeEquivalentTo(new { message = "Authentication failed" });
        }

        [Test]
        public async Task GetPasskeys_WithValidRequest_ReturnsOkResult()
        {
            // Arrange
            var expectedResponse = new GetUserPasskeysResponse(new List<PasskeyDto>
            {
                new PasskeyDto(1, "MyPasskey1", DateTime.Now, "guid1"),
                new PasskeyDto(2, "MyPasskey2", DateTime.Now.AddDays(-1), "guid2")
            });
            
            _mediator.Send(Arg.Any<GetUserPasskeysQuery>(), Arg.Any<CancellationToken>())
                .Returns(expectedResponse);

            // Act
            var result = await _controller.GetPasskeys(CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().Be(expectedResponse);
            
            await _mediator.Received(1).Send(
                Arg.Is<GetUserPasskeysQuery>(query => 
                    query.User == _controller.User), 
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task GetPasskeys_WithException_ReturnsBadRequest()
        {
            // Arrange
            _mediator.Send(Arg.Any<GetUserPasskeysQuery>(), Arg.Any<CancellationToken>())
                .Returns<ValueTask<GetUserPasskeysResponse>>(x => throw new AppException("Failed to retrieve passkeys"));

            // Act
            var result = await _controller.GetPasskeys(CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = (BadRequestObjectResult)result;
            var response = badRequestResult.Value;
            response.Should().BeEquivalentTo(new { message = "Failed to retrieve passkeys" });
        }

        [Test]
        public async Task DeletePasskey_WithValidId_ReturnsOkResult()
        {
            // Arrange
            const int passkeyId = 123;
            var expectedResponse = new DeletePasskeyResponse("Passkey deleted successfully", true);
            
            _mediator.Send(Arg.Any<DeletePasskeyCommand>(), Arg.Any<CancellationToken>())
                .Returns(expectedResponse);

            // Act
            var result = await _controller.DeletePasskey(passkeyId, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().Be(expectedResponse);
            
            await _mediator.Received(1).Send(
                Arg.Is<DeletePasskeyCommand>(cmd => 
                    cmd.User == _controller.User && 
                    cmd.PasskeyId == passkeyId), 
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task DeletePasskey_WithException_ReturnsBadRequest()
        {
            // Arrange
            const int passkeyId = 123;
            
            _mediator.Send(Arg.Any<DeletePasskeyCommand>(), Arg.Any<CancellationToken>())
                .Returns<ValueTask<DeletePasskeyResponse>>(x => throw new AppException("Passkey not found"));

            // Act
            var result = await _controller.DeletePasskey(passkeyId, CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = (BadRequestObjectResult)result;
            var response = badRequestResult.Value;
            response.Should().BeEquivalentTo(new { message = "Passkey not found" });
        }
    }
}