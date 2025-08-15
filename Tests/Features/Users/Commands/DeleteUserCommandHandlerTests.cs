using AwesomeAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Commands.DeleteUser;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users;
using Mediator;
using Tests.TestHelpers;

namespace Tests.Features.Users.Commands
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class DeleteUserCommandHandlerTests : TestBase
    {
        private DeleteUserCommandHandler _handler;
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
            
            _handler = new DeleteUserCommandHandler(_userManager);
        }

        [TearDown]
        public new void TearDown()
        {
            _userManager?.Dispose();
            base.TearDown();
        }

        [Test]
        public async Task Handle_ValidUserId_DeletesUserSuccessfully()
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
            
            var command = new DeleteUserCommand(user.Id);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().Be(Unit.Value);
            
            // Verify user was deleted
            var deletedUser = await _userManager.FindByIdAsync(user.Id.ToString());
            deletedUser.Should().BeNull();
        }

        [Test]
        public async Task Handle_InvalidUserId_ThrowsAppException()
        {
            // Arrange
            var command = new DeleteUserCommand(999);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("User not found");
        }

        [Test]
        public async Task Handle_ZeroUserId_ThrowsAppException()
        {
            // Arrange
            var command = new DeleteUserCommand(0);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("User not found");
        }

        [Test]
        public async Task Handle_NegativeUserId_ThrowsAppException()
        {
            // Arrange
            var command = new DeleteUserCommand(-1);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("User not found");
        }

        [Test]
        public async Task Handle_DeleteNonExistentUser_ThrowsAppException()
        {
            // Arrange
            var command = new DeleteUserCommand(12345);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("User not found");
        }

        [Test]
        public async Task Handle_DeleteUserTwice_SecondCallThrowsAppException()
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
            
            var command = new DeleteUserCommand(user.Id);
            var cancellationToken = GetCancellationToken();

            // Act - Delete user first time
            await _handler.Handle(command, cancellationToken);

            // Act & Assert - Try to delete again
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("User not found");
        }

        [Test]
        public async Task Handle_DeleteUserWithSpecialCharacters_DeletesSuccessfully()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "user@domain.com",
                Email = "user@domain.com",
                FirstName = "José María",
                LastName = "González-Pérez"
            };
            
            await _userManager.CreateAsync(user, "TestPassword123!");
            
            var command = new DeleteUserCommand(user.Id);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().Be(Unit.Value);
            
            // Verify user was deleted
            var deletedUser = await _userManager.FindByIdAsync(user.Id.ToString());
            deletedUser.Should().BeNull();
        }

        [Test]
        public async Task Handle_DeleteUserWithNullNames_DeletesSuccessfully()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com",
                FirstName = null,
                LastName = null
            };
            
            await _userManager.CreateAsync(user, "TestPassword123!");
            
            var command = new DeleteUserCommand(user.Id);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().Be(Unit.Value);
            
            // Verify user was deleted
            var deletedUser = await _userManager.FindByIdAsync(user.Id.ToString());
            deletedUser.Should().BeNull();
        }
    }
}