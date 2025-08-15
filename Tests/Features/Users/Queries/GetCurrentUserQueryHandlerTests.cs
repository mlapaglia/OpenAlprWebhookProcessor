using AwesomeAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetCurrentUser;
using System.Security.Claims;
using Tests.TestHelpers;

namespace Tests.Features.Users.Queries
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class GetCurrentUserQueryHandlerTests : TestBase
    {
        private GetCurrentUserQueryHandler _handler;
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
            
            _handler = new GetCurrentUserQueryHandler(_userManager);
        }

        [TearDown]
        public new void TearDown()
        {
            _userManager?.Dispose();
            base.TearDown();
        }

        [Test]
        public async Task Handle_AuthenticatedUserExists_ReturnsUserDto()
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
            var query = new GetCurrentUserQuery(principal);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
            result.Username.Should().Be("testuser");
            result.FirstName.Should().Be("Test");
            result.LastName.Should().Be("User");
        }

        [Test]
        public async Task Handle_UnauthenticatedUser_ReturnsNull()
        {
            // Arrange
            var identity = new ClaimsIdentity(); // Not authenticated
            var principal = new ClaimsPrincipal(identity);
            var query = new GetCurrentUserQuery(principal);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task Handle_AuthenticatedUserDoesNotExist_ReturnsNull()
        {
            // Arrange
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "nonexistentuser"),
                new Claim(ClaimTypes.NameIdentifier, "999")
            }, "test");
            
            var principal = new ClaimsPrincipal(identity);
            var query = new GetCurrentUserQuery(principal);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task Handle_NullPrincipal_ReturnsNull()
        {
            // Arrange
            var query = new GetCurrentUserQuery(null);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task Handle_AuthenticatedUserWithNullNames_ReturnsUserDtoWithNullNames()
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
            
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }, "test");
            
            var principal = new ClaimsPrincipal(identity);
            var query = new GetCurrentUserQuery(principal);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
            result.Username.Should().Be("testuser");
            result.FirstName.Should().BeNull();
            result.LastName.Should().BeNull();
        }

        [Test]
        public async Task Handle_AuthenticatedUserWithEmptyNames_ReturnsUserDtoWithEmptyNames()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com",
                FirstName = "",
                LastName = ""
            };
            
            await _userManager.CreateAsync(user, "TestPassword123!");
            
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }, "test");
            
            var principal = new ClaimsPrincipal(identity);
            var query = new GetCurrentUserQuery(principal);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
            result.Username.Should().Be("testuser");
            result.FirstName.Should().Be("");
            result.LastName.Should().Be("");
        }
    }
}