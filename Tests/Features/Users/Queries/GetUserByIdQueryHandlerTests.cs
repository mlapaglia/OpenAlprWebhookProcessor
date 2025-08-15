using AwesomeAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetUserById;
using Tests.TestHelpers;

namespace Tests.Features.Users.Queries
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class GetUserByIdQueryHandlerTests : TestBase
    {
        private GetUserByIdQueryHandler _handler;
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
            
            _handler = new GetUserByIdQueryHandler(_userManager);
        }

        [TearDown]
        public new void TearDown()
        {
            _userManager?.Dispose();
            base.TearDown();
        }

        [Test]
        public async Task Handle_ValidUserId_ReturnsUser()
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
            
            var query = new GetUserByIdQuery(user.Id);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
            result.UserName.Should().Be("testuser");
            result.FirstName.Should().Be("Test");
            result.LastName.Should().Be("User");
            result.Email.Should().Be("test@example.com");
        }

        [Test]
        public async Task Handle_InvalidUserId_ReturnsNull()
        {
            // Arrange
            var query = new GetUserByIdQuery(999);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task Handle_UserWithNullNames_ReturnsUserWithNullNames()
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
            
            var query = new GetUserByIdQuery(user.Id);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.FirstName.Should().BeNull();
            result.LastName.Should().BeNull();
            result.UserName.Should().Be("testuser");
        }

        [Test]
        public async Task Handle_UserWithEmptyNames_ReturnsUserWithEmptyNames()
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
            
            var query = new GetUserByIdQuery(user.Id);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.FirstName.Should().Be("");
            result.LastName.Should().Be("");
            result.UserName.Should().Be("testuser");
        }

        [Test]
        public async Task Handle_ZeroUserId_ReturnsNull()
        {
            // Arrange
            var query = new GetUserByIdQuery(0);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task Handle_NegativeUserId_ReturnsNull()
        {
            // Arrange
            var query = new GetUserByIdQuery(-1);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeNull();
        }
    }
}