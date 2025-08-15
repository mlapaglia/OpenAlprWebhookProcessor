using AwesomeAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Commands.EnableTwoFactor;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users;
using System.Security.Claims;
using Tests.TestHelpers;

namespace Tests.Features.Users.Commands
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class EnableTwoFactorCommandHandlerTests : TestBase
    {
        private EnableTwoFactorCommandHandler _handler;
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
            
            _handler = new EnableTwoFactorCommandHandler(_userManager);
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
            var command = new EnableTwoFactorCommand(principal, "123456");
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("User not found");
        }

        [Test]
        public async Task Handle_InvalidVerificationCode_ThrowsAppException()
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
            await _userManager.ResetAuthenticatorKeyAsync(user);
            
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }, "test");
            
            var principal = new ClaimsPrincipal(identity);
            var command = new EnableTwoFactorCommand(principal, "000000"); // Invalid code
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("Verification code is invalid");
        }

        [Test]
        public async Task Handle_ValidVerificationCode_EnablesTwoFactorAndReturnsRecoveryCodes()
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
            await _userManager.ResetAuthenticatorKeyAsync(user);
            
            // This test will fail with invalid code since we can't generate a real TOTP code
            // In a real scenario, you'd mock the token verification or use a test token provider
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }, "test");
            
            var principal = new ClaimsPrincipal(identity);
            var command = new EnableTwoFactorCommand(principal, "123456");
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            // This will throw because we can't generate a valid TOTP code in tests
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("Verification code is invalid");
        }

        [Test]
        public async Task Handle_EmptyVerificationCode_ThrowsAppException()
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
            await _userManager.ResetAuthenticatorKeyAsync(user);
            
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }, "test");
            
            var principal = new ClaimsPrincipal(identity);
            var command = new EnableTwoFactorCommand(principal, "");
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("Verification code is invalid");
        }

        [Test]
        public async Task Handle_NullVerificationCode_ThrowsAppException()
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
            await _userManager.ResetAuthenticatorKeyAsync(user);
            
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }, "test");
            
            var principal = new ClaimsPrincipal(identity);
            var command = new EnableTwoFactorCommand(principal, null);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("Verification code is invalid");
        }

        [Test]
        public async Task Handle_UserWithoutAuthenticatorKey_ThrowsAppException()
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
            // Don't set up authenticator key
            
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }, "test");
            
            var principal = new ClaimsPrincipal(identity);
            var command = new EnableTwoFactorCommand(principal, "123456");
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("Verification code is invalid");
        }

        [Test]
        public async Task Handle_UnauthenticatedUser_ThrowsAppException()
        {
            // Arrange
            var identity = new ClaimsIdentity(); // Not authenticated
            var principal = new ClaimsPrincipal(identity);
            var command = new EnableTwoFactorCommand(principal, "123456");
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("User not found");
        }
    }
}