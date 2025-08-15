using AwesomeAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users.Queries.CanRegister;
using Tests.TestHelpers;

namespace Tests.Features.Users.Queries
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class CanRegisterQueryHandlerTests : TestBase
    {
        private CanRegisterQueryHandler _handler;
        private UserManager<ApplicationUser> _userManager;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            // Set up UserManager with the test context
            var services = new ServiceCollection();
            services.AddSingleton(UsersContext); // Add the existing test context
            services.AddLogging(); // Add logging services for UserManager
            services.AddIdentity<ApplicationUser, IdentityRole<int>>()
                .AddEntityFrameworkStores<UsersContext>();
            
            var serviceProvider = services.BuildServiceProvider();
            _userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            
            _handler = new CanRegisterQueryHandler(_userManager);
        }

        [TearDown]
        public new void TearDown()
        {
            _userManager?.Dispose();
            base.TearDown();
        }

        [Test]
        public async Task Handle_NoExistingUsers_ReturnsTrue()
        {
            // Arrange
            var query = new CanRegisterQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task Handle_ExistingUsers_ReturnsFalse()
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
            
            var query = new CanRegisterQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeFalse();
        }
    }
}