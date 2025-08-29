using AwesomeAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Commands.Authenticate;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users;
using Tests.TestHelpers;

namespace Tests.Features.Users.Commands
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class AuthenticateCommandHandlerTests : TestBase
    {
        private AuthenticateCommandHandler _handler;
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            // Set up UserManager with the test context
            var services = new ServiceCollection();
            services.AddSingleton(UsersContext);
            services.AddLogging();
            
            // Add HttpContext support for SignInManager
            var httpContext = Substitute.For<HttpContext>();
            var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            httpContextAccessor.HttpContext.Returns(httpContext);
            services.AddSingleton(httpContextAccessor);
            
            // Add authentication services required by SignInManager
            services.AddAuthentication()
                .AddCookie();
            
            // Mock authentication service in HttpContext
            var authService = Substitute.For<IAuthenticationService>();
            var mockServiceProvider = Substitute.For<IServiceProvider>();
            mockServiceProvider.GetService(typeof(IAuthenticationService)).Returns(authService);
            httpContext.RequestServices.Returns(mockServiceProvider);
            
            services.AddIdentity<ApplicationUser, IdentityRole<int>>()
                .AddEntityFrameworkStores<UsersContext>()
                .AddDefaultTokenProviders();
            
            var serviceProvider = services.BuildServiceProvider();
            _userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            _signInManager = serviceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
            
            _handler = new AuthenticateCommandHandler(_userManager, _signInManager, UsersContext);
        }

        [TearDown]
        public new void TearDown()
        {
            _userManager?.Dispose();
            base.TearDown();
        }

        [Test]
        public async Task Handle_ValidCredentials_ReturnsAuthenticateResponse()
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
            
            var command = new AuthenticateCommand("testuser", "TestPassword123!", false);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Username.Should().Be("testuser");
            result.FirstName.Should().Be("Test");
            result.LastName.Should().Be("User");
            result.TwoFactorEnabled.Should().BeFalse();
        }

        [Test]
        public async Task Handle_InvalidUsername_ThrowsAppException()
        {
            // Arrange
            var command = new AuthenticateCommand("nonexistentuser", "password", false);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("Username or password is incorrect");
        }

        [Test]
        public async Task Handle_InvalidPassword_ThrowsAppException()
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
            
            var command = new AuthenticateCommand("testuser", "wrongpassword", false);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("Username or password is incorrect");
        }

        [Test]
        public async Task Handle_UserWith2FAEnabled_ReturnsRequiresTwoFactorTrue()
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
            
            var command = new AuthenticateCommand("testuser", "TestPassword123!", false);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.TwoFactorEnabled.Should().BeTrue();
            result.Id.Should().Be(user.Id);
        }

        [Test]
        public async Task Handle_LockedOutUser_ThrowsAppException()
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
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(30));
            
            var command = new AuthenticateCommand("testuser", "TestPassword123!", false);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("Account locked due to multiple failed attempts");
        }
    }
}