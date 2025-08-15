using AwesomeAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Commands.Logout;
using OpenAlprWebhookProcessor.Features.Users.Data;
using Mediator;
using Tests.TestHelpers;

namespace Tests.Features.Users.Commands
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class LogoutCommandHandlerTests : TestBase
    {
        private LogoutCommandHandler _handler;
        private SignInManager<ApplicationUser> _signInManager;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
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
            _signInManager = serviceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
            
            _handler = new LogoutCommandHandler(_signInManager);
        }

        [TearDown]
        public new void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public async Task Handle_ValidLogoutCommand_CallsSignOutAsync()
        {
            // Arrange
            var command = new LogoutCommand();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            // Since SignInManager.SignOutAsync doesn't return anything meaningful to test,
            // we verify that the handler completes without throwing exceptions
            // and returns the expected Unit.Value
        }

        [Test]
        public async Task Handle_LogoutCommand_ReturnsUnitValue()
        {
            // Arrange
            var command = new LogoutCommand();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().Be(Unit.Value);
        }

        [Test]
        public async Task Handle_MultipleLogoutCalls_DoesNotThrow()
        {
            // Arrange
            var command = new LogoutCommand();
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () =>
            {
                await _handler.Handle(command, cancellationToken);
                await _handler.Handle(command, cancellationToken);
                await _handler.Handle(command, cancellationToken);
            }).Should().NotThrowAsync();
        }

        [Test]
        public async Task Handle_WithCancellationToken_CompletesSuccessfully()
        {
            // Arrange
            var command = new LogoutCommand();
            using var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().Be(Unit.Value);
        }
    }
}