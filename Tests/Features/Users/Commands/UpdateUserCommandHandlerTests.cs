using AwesomeAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Commands.UpdateUser;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users;
using Mediator;
using Tests.TestHelpers;

namespace Tests.Features.Users.Commands
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class UpdateUserCommandHandlerTests : TestBase
    {
        private UpdateUserCommandHandler _handler;
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
            
            _handler = new UpdateUserCommandHandler(_userManager);
        }

        [TearDown]
        public new void TearDown()
        {
            _userManager?.Dispose();
            base.TearDown();
        }

        [Test]
        public async Task Handle_ValidUserUpdate_UpdatesUserSuccessfully()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User"
            };
            
            await _userManager.CreateAsync(user, "TestPassword123!");
            
            var command = new UpdateUserCommand(user.Id, "NewFirst", "NewLast", "newusername", null);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().Be(Unit.Value);
            
            var updatedUser = await _userManager.FindByIdAsync(user.Id.ToString());
            updatedUser.Should().NotBeNull();
            updatedUser.UserName.Should().Be("newusername");
            updatedUser.FirstName.Should().Be("NewFirst");
            updatedUser.LastName.Should().Be("NewLast");
        }

        [Test]
        public async Task Handle_InvalidUserId_ThrowsAppException()
        {
            // Arrange
            var command = new UpdateUserCommand(999, "NewFirst", "NewLast", "newusername", null);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("User not found");
        }

        [Test]
        public async Task Handle_DuplicateUsername_ThrowsAppException()
        {
            // Arrange
            var user1 = new ApplicationUser { UserName = "user1", Email = "user1@example.com", FirstName = "User", LastName = "One" };
            var user2 = new ApplicationUser { UserName = "user2", Email = "user2@example.com", FirstName = "User", LastName = "Two" };
            
            await _userManager.CreateAsync(user1, "TestPassword123!");
            await _userManager.CreateAsync(user2, "TestPassword123!");
            
            var command = new UpdateUserCommand(user2.Id, "NewFirst", "NewLast", "user1", null);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("Username user1 is already taken");
        }

        [Test]
        public async Task Handle_UpdateWithSameUsername_DoesNotThrowException()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User"
            };
            
            await _userManager.CreateAsync(user, "TestPassword123!");
            
            var command = new UpdateUserCommand(user.Id, "NewFirst", "NewLast", "testuser", null);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().Be(Unit.Value);
            
            var updatedUser = await _userManager.FindByIdAsync(user.Id.ToString());
            updatedUser.Should().NotBeNull();
            updatedUser.UserName.Should().Be("testuser");
            updatedUser.FirstName.Should().Be("NewFirst");
            updatedUser.LastName.Should().Be("NewLast");
        }

        [Test]
        public async Task Handle_UpdateWithEmptyUsername_DoesNotUpdateUsername()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User"
            };
            
            await _userManager.CreateAsync(user, "TestPassword123!");
            
            var command = new UpdateUserCommand(user.Id, "NewFirst", "NewLast", "", null);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().Be(Unit.Value);
            
            var updatedUser = await _userManager.FindByIdAsync(user.Id.ToString());
            updatedUser.Should().NotBeNull();
            updatedUser.UserName.Should().Be("testuser"); // Should remain unchanged
            updatedUser.FirstName.Should().Be("NewFirst");
            updatedUser.LastName.Should().Be("NewLast");
        }

        [Test]
        public async Task Handle_UpdateWithNullUsername_DoesNotUpdateUsername()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User"
            };
            
            await _userManager.CreateAsync(user, "TestPassword123!");
            
            var command = new UpdateUserCommand(user.Id, "NewFirst", "NewLast", null, null);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().Be(Unit.Value);
            
            var updatedUser = await _userManager.FindByIdAsync(user.Id.ToString());
            updatedUser.Should().NotBeNull();
            updatedUser.UserName.Should().Be("testuser"); // Should remain unchanged
            updatedUser.FirstName.Should().Be("NewFirst");
            updatedUser.LastName.Should().Be("NewLast");
        }

        [Test]
        public async Task Handle_UpdateWithPassword_UpdatesPasswordSuccessfully()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User"
            };
            
            await _userManager.CreateAsync(user, "TestPassword123!");
            
            var command = new UpdateUserCommand(user.Id, "Test", "User", "testuser", "NewPassword123!");
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().Be(Unit.Value);
            
            // Verify password was updated by checking password
            var updatedUser = await _userManager.FindByIdAsync(user.Id.ToString());
            var passwordValid = await _userManager.CheckPasswordAsync(updatedUser, "NewPassword123!");
            passwordValid.Should().BeTrue();
        }

        [Test]
        public async Task Handle_UpdateWithEmptyFirstName_DoesNotUpdateFirstName()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User"
            };
            
            await _userManager.CreateAsync(user, "TestPassword123!");
            
            var command = new UpdateUserCommand(user.Id, "", "NewLast", "testuser", null);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().Be(Unit.Value);
            
            var updatedUser = await _userManager.FindByIdAsync(user.Id.ToString());
            updatedUser.Should().NotBeNull();
            updatedUser.FirstName.Should().Be("Test"); // Should remain unchanged
            updatedUser.LastName.Should().Be("NewLast");
        }

        [Test]
        public async Task Handle_UpdateWithEmptyLastName_DoesNotUpdateLastName()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User"
            };
            
            await _userManager.CreateAsync(user, "TestPassword123!");
            
            var command = new UpdateUserCommand(user.Id, "NewFirst", "", "testuser", null);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().Be(Unit.Value);
            
            var updatedUser = await _userManager.FindByIdAsync(user.Id.ToString());
            updatedUser.Should().NotBeNull();
            updatedUser.FirstName.Should().Be("NewFirst");
            updatedUser.LastName.Should().Be("User"); // Should remain unchanged
        }
    }
}