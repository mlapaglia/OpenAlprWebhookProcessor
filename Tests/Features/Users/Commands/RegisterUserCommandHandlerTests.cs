using AwesomeAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Commands.RegisterUser;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users;
using Tests.TestHelpers;

namespace Tests.Features.Users.Commands
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class RegisterUserCommandHandlerTests : TestBase
    {
        private RegisterUserCommandHandler _handler;
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
            
            _handler = new RegisterUserCommandHandler(_userManager);
        }

        [TearDown]
        public new void TearDown()
        {
            _userManager?.Dispose();
            base.TearDown();
        }

        [Test]
        public async Task Handle_ValidUserData_CreatesUserSuccessfully()
        {
            // Arrange
            var command = new RegisterUserCommand("testuser", "TestPassword123!", "Test", "User");
            var cancellationToken = GetCancellationToken();

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            var createdUser = await _userManager.FindByNameAsync("testuser");
            createdUser.Should().NotBeNull();
            createdUser.UserName.Should().Be("testuser");
            createdUser.FirstName.Should().Be("Test");
            createdUser.LastName.Should().Be("User");
            createdUser.Email.Should().Be("testuser");
        }

        [Test]
        public async Task Handle_WeakPassword_ThrowsAppException()
        {
            // Arrange
            var command = new RegisterUserCommand("testuser", "weak", "Test", "User");
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("Registration failed*");
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
            
            var command = new RegisterUserCommand("testuser", "TestPassword123!", "Test", "User");
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("Registration failed*");
        }

        [Test]
        public async Task Handle_EmptyUsername_ThrowsAppException()
        {
            // Arrange
            var command = new RegisterUserCommand("", "TestPassword123!", "Test", "User");
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("Registration failed*");
        }

        [Test]
        public async Task Handle_EmptyPassword_ThrowsAppException()
        {
            // Arrange
            var command = new RegisterUserCommand("testuser", "", "Test", "User");
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("Registration failed*");
        }

        [Test]
        public async Task Handle_NullFirstName_CreatesUserWithNullFirstName()
        {
            // Arrange
            var command = new RegisterUserCommand("testuser", "TestPassword123!", null, "User");
            var cancellationToken = GetCancellationToken();

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            var createdUser = await _userManager.FindByNameAsync("testuser");
            createdUser.Should().NotBeNull();
            createdUser.FirstName.Should().BeNull();
            createdUser.LastName.Should().Be("User");
        }

        [Test]
        public async Task Handle_NullLastName_CreatesUserWithNullLastName()
        {
            // Arrange
            var command = new RegisterUserCommand("testuser", "TestPassword123!", "Test", null);
            var cancellationToken = GetCancellationToken();

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            var createdUser = await _userManager.FindByNameAsync("testuser");
            createdUser.Should().NotBeNull();
            createdUser.FirstName.Should().Be("Test");
            createdUser.LastName.Should().BeNull();
        }
    }
}