using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users;
using OpenAlprWebhookProcessor.Features.Users.Commands.CreateUser;
using OpenAlprWebhookProcessor.Features.Users.Commands.DeleteUser;
using OpenAlprWebhookProcessor.Features.Users.Commands.UpdateUser;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users.Queries.CanRegister;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetAllUsers;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetUserById;
using Tests.TestHelpers;
using Microsoft.AspNetCore.Identity;
using System.Text.Encodings.Web;

namespace Tests.Controllers
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class UsersControllerTests : TestBase
    {
        private UsersController _controller;
        private UserManager<ApplicationUser> _userManager;
        private UrlEncoder _urlEncoder;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _userManager = Substitute.For<UserManager<ApplicationUser>>(
                Substitute.For<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
            _urlEncoder = Substitute.For<UrlEncoder>();
            _controller = new UsersController(Mediator, _userManager, _urlEncoder);
            
            // Set up mock HTTP context
            var httpContext = new DefaultHttpContext();
            
            // Set up connection with IP address
            httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
            
            var responseFeature = new HttpResponseFeature();
            httpContext.Features.Set<IHttpResponseFeature>(responseFeature);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [TearDown]
        public new void TearDown()
        {
            _userManager?.Dispose();
            base.TearDown();
        }

        [Test]
        public async Task CanRegister_ReturnsCorrectResult()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();
            var expectedResult = true;

            Mediator.Send(Arg.Any<CanRegisterQuery>(), cancellationToken)
                .Returns(expectedResult);

            // Act
            var result = await _controller.CanRegister(cancellationToken);

            // Assert
            result.Should().Be(expectedResult);
        }

        [Test]
        public async Task CanRegister_CallsCorrectQuery()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.CanRegister(cancellationToken);

            // Assert
            await Mediator.Received(1).Send(Arg.Any<CanRegisterQuery>(), cancellationToken);
        }

        [Test]
        public async Task AddUser_ValidUser_ReturnsOk()
        {
            // Arrange
            var model = TestDataFactory.CreateTestRegisterModel();
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
            var model = TestDataFactory.CreateTestRegisterModel();
            var cancellationToken = GetCancellationToken();

            Mediator.When(x => x.Send(Arg.Any<CreateUserCommand>(), cancellationToken))
                .Do(x => throw new AppException("Username already exists"));

            // Act
            var result = await _controller.AddUser(model, cancellationToken);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task AddUser_CallsCorrectCommand()
        {
            // Arrange
            var model = TestDataFactory.CreateTestRegisterModel();
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.AddUser(model, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<CreateUserCommand>(cmd => 
                    cmd.FirstName == model.FirstName &&
                    cmd.LastName == model.LastName &&
                    cmd.Username == model.Username &&
                    cmd.Password == model.Password), 
                cancellationToken);
        }

        [Test]
        public async Task Register_CanRegisterTrue_ReturnsOk()
        {
            // Arrange
            var model = TestDataFactory.CreateTestRegisterModel();
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
            var model = TestDataFactory.CreateTestRegisterModel();
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
            var model = TestDataFactory.CreateTestRegisterModel();
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<CanRegisterQuery>(), cancellationToken)
                .Returns(true);

            Mediator.When(x => x.Send(Arg.Any<CreateUserCommand>(), cancellationToken))
                .Do(x => throw new AppException("Username already exists"));

            // Act
            var result = await _controller.Register(model, cancellationToken);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task GetAll_ReturnsOkWithUsers()
        {
            // Arrange
            var expectedUsers = TestDataFactory.CreateTestUserDtoList();
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
            await Mediator.Received(1).Send(Arg.Any<GetAllUsersQuery>(), cancellationToken);
        }

        [Test]
        public async Task GetById_ValidId_ReturnsOkWithUser()
        {
            // Arrange
            var userId = 1;
            var expectedUser = TestDataFactory.CreateTestApplicationUser();
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
                .Returns((ApplicationUser)null);

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
                    cmd.Username == updateModel.Username &&
                    cmd.Password == updateModel.Password), 
                cancellationToken);
        }
    }
}