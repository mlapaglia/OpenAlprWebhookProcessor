using AwesomeAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Commands.DisableTwoFactor;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users;
using System.Security.Claims;
using Mediator;
using Tests.TestHelpers;

namespace Tests.Features.Users.Commands
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class DisableTwoFactorCommandHandlerTests : TestBase
    {
        private DisableTwoFactorCommandHandler _handler;
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
            
            _handler = new DisableTwoFactorCommandHandler(_userManager);
        }

        [TearDown]
        public new void TearDown()
        {
            _userManager?.Dispose();
            base.TearDown();
        }

        [Test]
        public async Task Handle_UserNotFound_ThrowsAppException()
        {
            // Arrange
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "nonexistentuser"),
                new Claim(ClaimTypes.NameIdentifier, "999")
            }, "test");
            
            var principal = new ClaimsPrincipal(identity);
            var command = new DisableTwoFactorCommand(principal);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("User not found");
        }

        [Test]
        public async Task Handle_ValidUser_DisablesTwoFactorAndResetsKey()
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
            await _userManager.SetTwoFactorEnabledAsync(user, true);
            await _userManager.ResetAuthenticatorKeyAsync(user);
            
            // Verify 2FA is initially enabled
            var initialStatus = await _userManager.GetTwoFactorEnabledAsync(user);
            var initialKey = await _userManager.GetAuthenticatorKeyAsync(user);
            initialStatus.Should().BeTrue();
            initialKey.Should().NotBeNull();
            
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }, "test");
            
            var principal = new ClaimsPrincipal(identity);
            var command = new DisableTwoFactorCommand(principal);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().Be(Unit.Value);
            
            // Verify 2FA is disabled and key is reset
            var finalStatus = await _userManager.GetTwoFactorEnabledAsync(user);
            var finalKey = await _userManager.GetAuthenticatorKeyAsync(user);
            finalStatus.Should().BeFalse();
            finalKey.Should().NotBe(initialKey); // Key should be reset
        }

        [Test]
        public async Task Handle_UserWith2FAAlreadyDisabled_StillSucceeds()
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
            // 2FA is disabled by default
            
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }, "test");
            
            var principal = new ClaimsPrincipal(identity);
            var command = new DisableTwoFactorCommand(principal);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().Be(Unit.Value);
            
            // Verify 2FA remains disabled
            var finalStatus = await _userManager.GetTwoFactorEnabledAsync(user);
            finalStatus.Should().BeFalse();
        }

        [Test]
        public async Task Handle_UserWithoutAuthenticatorKey_StillSucceeds()
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
            await _userManager.SetTwoFactorEnabledAsync(user, true);
            // Don't set up authenticator key
            
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }, "test");
            
            var principal = new ClaimsPrincipal(identity);
            var command = new DisableTwoFactorCommand(principal);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().Be(Unit.Value);
            
            // Verify 2FA is disabled
            var finalStatus = await _userManager.GetTwoFactorEnabledAsync(user);
            finalStatus.Should().BeFalse();
        }

        [Test]
        public async Task Handle_MultipleDisableCalls_DoesNotThrow()
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
            await _userManager.SetTwoFactorEnabledAsync(user, true);
            await _userManager.ResetAuthenticatorKeyAsync(user);
            
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }, "test");
            
            var principal = new ClaimsPrincipal(identity);
            var command = new DisableTwoFactorCommand(principal);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () =>
            {
                await _handler.Handle(command, cancellationToken);
                await _handler.Handle(command, cancellationToken);
                await _handler.Handle(command, cancellationToken);
            }).Should().NotThrowAsync();
            
            // Verify 2FA remains disabled
            var finalStatus = await _userManager.GetTwoFactorEnabledAsync(user);
            finalStatus.Should().BeFalse();
        }

        [Test]
        public async Task Handle_UnauthenticatedUser_ThrowsAppException()
        {
            // Arrange
            var identity = new ClaimsIdentity(); // Not authenticated
            var principal = new ClaimsPrincipal(identity);
            var command = new DisableTwoFactorCommand(principal);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("User not found");
        }

        [Test]
        public async Task Handle_WithCancellationToken_CompletesSuccessfully()
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
            await _userManager.SetTwoFactorEnabledAsync(user, true);
            
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }, "test");
            
            var principal = new ClaimsPrincipal(identity);
            var command = new DisableTwoFactorCommand(principal);
            using var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().Be(Unit.Value);
        }
    }
}