using AwesomeAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Commands.SetupTwoFactor;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Tests.TestHelpers;

namespace Tests.Features.Users.Commands
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class SetupTwoFactorCommandHandlerTests : TestBase
    {
        private SetupTwoFactorCommandHandler _handler;
        private UserManager<ApplicationUser> _userManager;
        private UrlEncoder _urlEncoder;

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
            _urlEncoder = UrlEncoder.Default;
            
            _handler = new SetupTwoFactorCommandHandler(_userManager, _urlEncoder);
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
            var command = new SetupTwoFactorCommand(principal);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("User not found");
        }

        [Test]
        public async Task Handle_ValidUser_ReturnsSetupResponse()
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
            var command = new SetupTwoFactorCommand(principal);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.SharedKey.Should().NotBeNullOrWhiteSpace();
            result.QrCodeUri.Should().NotBeNullOrWhiteSpace();
            result.QrCodeUri.Should().StartWith("otpauth://totp/");
            result.QrCodeUri.Should().Contain("OpenALPR%20Webhook%20Processor");
            result.QrCodeUri.Should().Contain("test@example.com");
        }

        [Test]
        public async Task Handle_ValidUserWithUsernameAsEmail_UsesUsernameInQrCode()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = null, // No email set
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
            var command = new SetupTwoFactorCommand(principal);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.SharedKey.Should().NotBeNullOrWhiteSpace();
            result.QrCodeUri.Should().NotBeNullOrWhiteSpace();
            result.QrCodeUri.Should().Contain("testuser");
        }

        [Test]
        public async Task Handle_SharedKeyIsFormatted_ContainsSpaces()
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
            var command = new SetupTwoFactorCommand(principal);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.SharedKey.Should().Contain(" "); // Should be formatted with spaces
            result.SharedKey.Should().Match(key => key.All(c => char.IsLetterOrDigit(c) || c == ' ')); // Only alphanumeric and spaces
        }

        [Test]
        public async Task Handle_MultipleCallsForSameUser_GeneratesNewKey()
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
            var command = new SetupTwoFactorCommand(principal);
            var cancellationToken = GetCancellationToken();

            // Act
            var result1 = await _handler.Handle(command, cancellationToken);
            var result2 = await _handler.Handle(command, cancellationToken);

            // Assert
            result1.Should().NotBeNull();
            result2.Should().NotBeNull();
            result1.SharedKey.Should().NotBe(result2.SharedKey); // Should generate new key each time
            result1.QrCodeUri.Should().NotBe(result2.QrCodeUri);
        }

        [Test]
        public async Task Handle_UnauthenticatedUser_ThrowsAppException()
        {
            // Arrange
            var identity = new ClaimsIdentity(); // Not authenticated
            var principal = new ClaimsPrincipal(identity);
            var command = new SetupTwoFactorCommand(principal);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("User not found");
        }
    }
}