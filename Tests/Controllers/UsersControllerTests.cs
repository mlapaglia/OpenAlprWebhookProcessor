using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users;
using OpenAlprWebhookProcessor.Features.Users.Commands.Authenticate;
using OpenAlprWebhookProcessor.Features.Users.Commands.CreateUser;
using OpenAlprWebhookProcessor.Features.Users.Commands.DeleteUser;
using OpenAlprWebhookProcessor.Features.Users.Commands.RefreshToken;
using OpenAlprWebhookProcessor.Features.Users.Commands.RevokeToken;
using OpenAlprWebhookProcessor.Features.Users.Commands.UpdateUser;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users.Queries.CanRegister;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetAllUsers;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetRefreshTokens;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetUserById;
using OpenAlprWebhookProcessor.Features.Users.Register;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tests.TestHelpers;

namespace Tests.Controllers
{
    [TestFixture]
    public class UsersControllerTests : TestBase
    {
        private UsersController _controller;
        private IRequestCookieCollection _mockCookies;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _controller = new UsersController(Mediator);
            
            // Set up mock HTTP context and cookies
            var httpContext = new DefaultHttpContext();
            _mockCookies = Substitute.For<IRequestCookieCollection>();
            httpContext.Request.Cookies = _mockCookies;
            
            // Set up connection with IP address
            httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
            
            var responseFeature = new HttpResponseFeature();
            httpContext.Features.Set<IHttpResponseFeature>(responseFeature);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Test]
        public async Task Authenticate_ValidCredentials_ReturnsOkWithResponse()
        {
            // Arrange
            var request = TestDataFactory.CreateTestAuthenticateRequest("testuser", "password");
            var expectedResponse = TestDataFactory.CreateTestAuthenticateResponse();
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<AuthenticateCommand>(), cancellationToken)
                .Returns(expectedResponse);

            // Act
            var result = await _controller.Authenticate(request, cancellationToken);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().Be(expectedResponse);
        }

        [Test]
        public async Task Authenticate_InvalidCredentials_ReturnsBadRequest()
        {
            // Arrange
            var request = TestDataFactory.CreateTestAuthenticateRequest("testuser", "wrongpassword");
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<AuthenticateCommand>(), cancellationToken)
                .Returns((AuthenticateResponse)null);

            // Act
            var result = await _controller.Authenticate(request, cancellationToken);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task Authenticate_CallsCorrectCommand()
        {
            // Arrange
            var request = TestDataFactory.CreateTestAuthenticateRequest("testuser", "password");
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.Authenticate(request, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<AuthenticateCommand>(cmd => 
                    cmd.Username == "testuser" && 
                    cmd.Password == "password"), 
                cancellationToken);
        }

        [Test]
        public async Task RefreshToken_ValidToken_ReturnsOkWithResponse()
        {
            // Arrange
            var expectedResponse = TestDataFactory.CreateTestAuthenticateResponse();
            var cancellationToken = GetCancellationToken();

            _mockCookies["refreshToken"].Returns("valid-refresh-token");

            Mediator.Send(Arg.Any<RefreshTokenCommand>(), cancellationToken)
                .Returns(expectedResponse);

            // Act
            var result = await _controller.RefreshToken(cancellationToken);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().Be(expectedResponse);
        }

        [Test]
        public async Task RefreshToken_InvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            _mockCookies["refreshToken"].Returns("invalid-token");

            Mediator.Send(Arg.Any<RefreshTokenCommand>(), cancellationToken)
                .Returns((AuthenticateResponse)null);

            // Act
            var result = await _controller.RefreshToken(cancellationToken);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Test]
        public async Task RefreshToken_CallsCorrectCommand()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            _mockCookies["refreshToken"].Returns("test-refresh-token");

            // Act
            await _controller.RefreshToken(cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<RefreshTokenCommand>(cmd => cmd.Token == "test-refresh-token"), 
                cancellationToken);
        }

        [Test]
        public async Task RevokeToken_ValidToken_ReturnsOkWithMessage()
        {
            // Arrange
            var request = TestDataFactory.CreateTestRevokeTokenRequest("test-token");
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<RevokeTokenCommand>(), cancellationToken)
                .Returns(true);

            // Act
            var result = await _controller.RevokeToken(request, cancellationToken);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task RevokeToken_InvalidToken_ReturnsNotFound()
        {
            // Arrange
            var request = TestDataFactory.CreateTestRevokeTokenRequest("invalid-token");
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<RevokeTokenCommand>(), cancellationToken)
                .Returns(false);

            // Act
            var result = await _controller.RevokeToken(request, cancellationToken);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Test]
        public async Task RevokeToken_NoTokenProvided_ReturnsBadRequest()
        {
            // Arrange
            var request = new RevokeTokenRequest { Token = null };
            var cancellationToken = GetCancellationToken();

            _mockCookies["refreshToken"].Returns((string)null);

            // Act
            var result = await _controller.RevokeToken(request, cancellationToken);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task RevokeToken_CallsCorrectCommand()
        {
            // Arrange
            var request = TestDataFactory.CreateTestRevokeTokenRequest("test-token");
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.RevokeToken(request, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<RevokeTokenCommand>(cmd => cmd.Token == "test-token"), 
                cancellationToken);
        }

        [Test]
        public async Task CanRegister_ReturnsCorrectResult()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<CanRegisterQuery>(), cancellationToken)
                .Returns(true);

            // Act
            var result = await _controller.CanRegister(cancellationToken);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task CanRegister_CallsCorrectQuery()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.CanRegister(cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Any<CanRegisterQuery>(), 
                cancellationToken);
        }

        [Test]
        public async Task AddUser_ValidUser_ReturnsOk()
        {
            // Arrange
            var model = TestDataFactory.CreateTestRegisterModel("newuser");
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _controller.AddUser(model, cancellationToken);

            // Assert
            result.Should().BeOfType<OkResult>();
        }

        [Test]
        public async Task AddUser_ExceptionThrown_ReturnsBadRequest()
        {
            // Arrange
            var model = TestDataFactory.CreateTestRegisterModel("existinguser");
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<CreateUserCommand>(), cancellationToken)
                .Returns(Task.FromException<User>(new AppException("Username already exists")));

            // Act
            var result = await _controller.AddUser(model, cancellationToken);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task AddUser_CallsCorrectCommand()
        {
            // Arrange
            var model = TestDataFactory.CreateTestRegisterModel("newuser");
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.AddUser(model, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<CreateUserCommand>(cmd => 
                    cmd.Username == "newuser" && 
                    cmd.FirstName == model.FirstName && 
                    cmd.LastName == model.LastName), 
                cancellationToken);
        }

        [Test]
        public async Task Register_CanRegisterTrue_ReturnsOk()
        {
            // Arrange
            var model = TestDataFactory.CreateTestRegisterModel("newuser");
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<CanRegisterQuery>(), cancellationToken)
                .Returns(true);

            // Act
            var result = await _controller.Register(model, cancellationToken);

            // Assert
            result.Should().BeOfType<OkResult>();
        }

        [Test]
        public async Task Register_CanRegisterFalse_ReturnsForbid()
        {
            // Arrange
            var model = TestDataFactory.CreateTestRegisterModel("newuser");
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<CanRegisterQuery>(), cancellationToken)
                .Returns(false);

            // Act
            var result = await _controller.Register(model, cancellationToken);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Test]
        public async Task Register_ExceptionThrown_ReturnsBadRequest()
        {
            // Arrange
            var model = TestDataFactory.CreateTestRegisterModel("existinguser");
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<CanRegisterQuery>(), cancellationToken)
                .Returns(true);

            Mediator.Send(Arg.Any<CreateUserCommand>(), cancellationToken)
                .Returns(Task.FromException<User>(new AppException("Username already exists")));

            // Act
            var result = await _controller.Register(model, cancellationToken);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task GetAll_ReturnsOkWithUsers()
        {
            // Arrange
            var expectedUsers = new List<User>
            {
                TestDataFactory.CreateTestUser("user1"),
                TestDataFactory.CreateTestUser("user2")
            };
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetAllUsersQuery>(), cancellationToken)
                .Returns(expectedUsers);

            // Act
            var result = await _controller.GetAll(cancellationToken);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().Be(expectedUsers);
        }

        [Test]
        public async Task GetAll_CallsCorrectQuery()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.GetAll(cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Any<GetAllUsersQuery>(), 
                cancellationToken);
        }

        [Test]
        public async Task GetById_ValidId_ReturnsOkWithUser()
        {
            // Arrange
            var userId = 1;
            var expectedUser = TestDataFactory.CreateTestUser();
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetUserByIdQuery>(), cancellationToken)
                .Returns(expectedUser);

            // Act
            var result = await _controller.GetById(userId, cancellationToken);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().Be(expectedUser);
        }

        [Test]
        public async Task GetById_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var userId = 999;
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetUserByIdQuery>(), cancellationToken)
                .Returns((User)null);

            // Act
            var result = await _controller.GetById(userId, cancellationToken);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task GetById_CallsCorrectQuery()
        {
            // Arrange
            var userId = 1;
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.GetById(userId, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<GetUserByIdQuery>(q => q.Id == userId), 
                cancellationToken);
        }

        [Test]
        public async Task DeleteById_ValidId_ReturnsOk()
        {
            // Arrange
            var userId = 1;
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _controller.DeleteById(userId, cancellationToken);

            // Assert
            result.Should().BeOfType<OkResult>();
        }

        [Test]
        public async Task DeleteById_CallsCorrectCommand()
        {
            // Arrange
            var userId = 1;
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.DeleteById(userId, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<DeleteUserCommand>(cmd => cmd.Id == userId), 
                cancellationToken);
        }

        [Test]
        public async Task UpdatedById_ValidUpdate_ReturnsOk()
        {
            // Arrange
            var userId = 1;
            var updateModel = TestDataFactory.CreateTestUpdateModel();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _controller.UpdatedById(userId, updateModel, cancellationToken);

            // Assert
            result.Should().BeOfType<OkResult>();
        }

        [Test]
        public async Task UpdatedById_CallsCorrectCommand()
        {
            // Arrange
            var userId = 1;
            var updateModel = TestDataFactory.CreateTestUpdateModel();
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.UpdatedById(userId, updateModel, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<UpdateUserCommand>(cmd => 
                    cmd.Id == userId && 
                    cmd.FirstName == updateModel.FirstName && 
                    cmd.LastName == updateModel.LastName && 
                    cmd.Username == updateModel.Username), 
                cancellationToken);
        }

        [Test]
        public async Task GetRefreshTokens_ValidId_ReturnsOkWithTokens()
        {
            // Arrange
            var userId = 1;
            var expectedTokens = TestDataFactory.CreateTestRefreshTokens();
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetRefreshTokensQuery>(), cancellationToken)
                .Returns(expectedTokens);

            // Act
            var result = await _controller.GetRefreshTokens(userId, cancellationToken);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().Be(expectedTokens);
        }

        [Test]
        public async Task GetRefreshTokens_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var userId = 999;
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetRefreshTokensQuery>(), cancellationToken)
                .Returns((List<RefreshToken>)null);

            // Act
            var result = await _controller.GetRefreshTokens(userId, cancellationToken);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task GetRefreshTokens_CallsCorrectQuery()
        {
            // Arrange
            var userId = 1;
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.GetRefreshTokens(userId, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<GetRefreshTokensQuery>(q => q.UserId == userId), 
                cancellationToken);
        }
    }
} 