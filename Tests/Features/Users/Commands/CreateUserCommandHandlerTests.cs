using AwesomeAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Commands.CreateUser;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users;
using Tests.TestHelpers;

namespace Tests.Features.Users.Commands
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class CreateUserCommandHandlerTests : TestBase
    {
        private CreateUserCommandHandler _handler;
        private UserManager<ApplicationUser> _userManager;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            var services = new ServiceCollection();
            services.AddSingleton(UsersContext);
            services.AddLogging();
            services.AddIdentity<ApplicationUser, IdentityRole<int>>()
                .AddEntityFrameworkStores<UsersContext>()
                .AddDefaultTokenProviders();
            
            var serviceProvider = services.BuildServiceProvider();
            _userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            
            _handler = new CreateUserCommandHandler(_userManager);
        }

        [TearDown]
        public new void TearDown()
        {
            _userManager?.Dispose();
            base.TearDown();
        }

        [Test]
        public async Task Handle_ValidUserData_CreatesAndReturnsUser()
        {
            // Arrange
            var command = new CreateUserCommand("Test", "User", "testuser", "TestPassword123!");
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.UserName.Should().Be("testuser");
            result.FirstName.Should().Be("Test");
            result.LastName.Should().Be("User");
            result.Email.Should().Be("testuser");

            // Verify user was created in the database
            var createdUser = await _userManager.FindByNameAsync("testuser");
            createdUser.Should().NotBeNull();
            createdUser.Id.Should().Be(result.Id);
        }

        [Test]
        public async Task Handle_DuplicateUsername_ThrowsAppException()
        {
            // Arrange
            var existingUser = new ApplicationUser
            {
                UserName = "testuser",
                Email = "existing@example.com",
                FirstName = "Existing",
                LastName = "User"
            };
            
            await _userManager.CreateAsync(existingUser, "TestPassword123!");
            
            var command = new CreateUserCommand("Test", "User", "testuser", "TestPassword123!");
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("Username \"testuser\" is already taken");
        }

        [Test]
        public async Task Handle_WeakPassword_ThrowsAppException()
        {
            // Arrange
            var command = new CreateUserCommand("Test", "User", "testuser", "weak");
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("Failed to create user:*");
        }

        [Test]
        public async Task Handle_EmptyUsername_ThrowsAppException()
        {
            // Arrange
            var command = new CreateUserCommand("Test", "User", "", "TestPassword123!");
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("Failed to create user:*");
        }

        [Test]
        public async Task Handle_EmptyPassword_ThrowsAppException()
        {
            // Arrange
            var command = new CreateUserCommand("Test", "User", "testuser", "");
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("Failed to create user:*");
        }

        [Test]
        public async Task Handle_NullFirstName_CreatesUserWithNullFirstName()
        {
            // Arrange
            var command = new CreateUserCommand(null, "User", "testuser", "TestPassword123!");
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.FirstName.Should().BeNull();
            result.LastName.Should().Be("User");
            result.UserName.Should().Be("testuser");
        }

        [Test]
        public async Task Handle_NullLastName_CreatesUserWithNullLastName()
        {
            // Arrange
            var command = new CreateUserCommand("Test", null, "testuser", "TestPassword123!");
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.FirstName.Should().Be("Test");
            result.LastName.Should().BeNull();
            result.UserName.Should().Be("testuser");
        }

        [Test]
        public async Task Handle_EmailSetToUsername_CreatesUserWithEmailAsUsername()
        {
            // Arrange
            var command = new CreateUserCommand("Test", "User", "testuser", "TestPassword123!");
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be("testuser");
            result.UserName.Should().Be("testuser");
        }

        [Test]
        public async Task Handle_SpecialCharactersInNames_CreatesUserSuccessfully()
        {
            // Arrange
            var command = new CreateUserCommand("José María", "González-Pérez", "testuser", "TestPassword123!");
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.FirstName.Should().Be("José María");
            result.LastName.Should().Be("González-Pérez");
            result.UserName.Should().Be("testuser");
        }
    }
}