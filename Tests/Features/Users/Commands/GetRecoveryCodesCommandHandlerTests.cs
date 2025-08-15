using AwesomeAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Commands.GetRecoveryCodes;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users;
using System.Security.Claims;
using Tests.TestHelpers;

namespace Tests.Features.Users.Commands
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class GetRecoveryCodesCommandHandlerTests : TestBase
    {
        private GetRecoveryCodesCommandHandler _handler;
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
            
            _handler = new GetRecoveryCodesCommandHandler(_userManager);
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
            var command = new GetRecoveryCodesCommand(principal);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("User not found");
        }

        [Test]
        public async Task Handle_ValidUser_GeneratesRecoveryCodes()
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
            
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }, "test");
            
            var principal = new ClaimsPrincipal(identity);
            var command = new GetRecoveryCodesCommand(principal);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.RecoveryCodes.Should().NotBeNull();
            result.RecoveryCodes.Should().HaveCount(10); // Default count is 10
            result.RecoveryCodes.Should().OnlyContain(code => !string.IsNullOrWhiteSpace(code));
            result.RecoveryCodes.Should().OnlyHaveUniqueItems(); // All codes should be unique
        }

        [Test]
        public async Task Handle_MultipleCallsForSameUser_GeneratesDifferentCodes()
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
            
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }, "test");
            
            var principal = new ClaimsPrincipal(identity);
            var command = new GetRecoveryCodesCommand(principal);
            var cancellationToken = GetCancellationToken();

            // Act
            var result1 = await _handler.Handle(command, cancellationToken);
            var result2 = await _handler.Handle(command, cancellationToken);

            // Assert
            result1.Should().NotBeNull();
            result2.Should().NotBeNull();
            result1.RecoveryCodes.Should().HaveCount(10);
            result2.RecoveryCodes.Should().HaveCount(10);
            
            // The two sets of recovery codes should be different
            result1.RecoveryCodes.Should().NotBeEquivalentTo(result2.RecoveryCodes);
        }

        [Test]
        public async Task Handle_RecoveryCodesHaveCorrectFormat_AreAlphanumeric()
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
            
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }, "test");
            
            var principal = new ClaimsPrincipal(identity);
            var command = new GetRecoveryCodesCommand(principal);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.RecoveryCodes.Should().OnlyContain(code => 
                code.All(c => char.IsLetterOrDigit(c) || c == '-')); // Recovery codes can contain letters, digits, and dashes
            result.RecoveryCodes.Should().OnlyContain(code => code.Length >= 8); // Should be reasonably long
        }

        [Test]
        public async Task Handle_UserWith2FAEnabled_GeneratesRecoveryCodes()
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
            var command = new GetRecoveryCodesCommand(principal);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.RecoveryCodes.Should().NotBeNull();
            result.RecoveryCodes.Should().HaveCount(10);
        }

        [Test]
        public async Task Handle_UserWith2FADisabled_StillGeneratesRecoveryCodes()
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
            var command = new GetRecoveryCodesCommand(principal);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.RecoveryCodes.Should().NotBeNull();
            result.RecoveryCodes.Should().HaveCount(10);
        }

        [Test]
        public async Task Handle_UnauthenticatedUser_ThrowsAppException()
        {
            // Arrange
            var identity = new ClaimsIdentity(); // Not authenticated
            var principal = new ClaimsPrincipal(identity);
            var command = new GetRecoveryCodesCommand(principal);
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
            
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }, "test");
            
            var principal = new ClaimsPrincipal(identity);
            var command = new GetRecoveryCodesCommand(principal);
            using var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.RecoveryCodes.Should().HaveCount(10);
        }
    }
}