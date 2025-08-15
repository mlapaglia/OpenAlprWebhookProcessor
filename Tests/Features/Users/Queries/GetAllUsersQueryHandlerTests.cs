using AwesomeAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetAllUsers;
using Tests.TestHelpers;

namespace Tests.Features.Users.Queries
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class GetAllUsersQueryHandlerTests : TestBase
    {
        private GetAllUsersQueryHandler _handler;
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
            
            _handler = new GetAllUsersQueryHandler(_userManager);
        }

        [TearDown]
        public new void TearDown()
        {
            _userManager?.Dispose();
            base.TearDown();
        }

        [Test]
        public async Task Handle_NoUsers_ReturnsEmptyList()
        {
            // Arrange
            var query = new GetAllUsersQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task Handle_SingleUser_ReturnsListWithOneUser()
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
            
            var query = new GetAllUsersQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            
            var userDto = result.First();
            userDto.Id.Should().Be(user.Id);
            userDto.Username.Should().Be("testuser");
            userDto.FirstName.Should().Be("Test");
            userDto.LastName.Should().Be("User");
            userDto.TwoFactorEnabled.Should().BeFalse();
        }

        [Test]
        public async Task Handle_MultipleUsers_ReturnsAllUsers()
        {
            // Arrange
            var users = new[]
            {
                new ApplicationUser
                {
                    UserName = "user1",
                    Email = "user1@example.com",
                    FirstName = "First",
                    LastName = "User"
                },
                new ApplicationUser
                {
                    UserName = "user2",
                    Email = "user2@example.com",
                    FirstName = "Second",
                    LastName = "User"
                },
                new ApplicationUser
                {
                    UserName = "user3",
                    Email = "user3@example.com",
                    FirstName = "Third",
                    LastName = "User"
                }
            };

            foreach (var user in users)
            {
                await _userManager.CreateAsync(user, "TestPassword123!");
            }
            
            var query = new GetAllUsersQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            
            result.Should().Contain(u => u.Username == "user1" && u.FirstName == "First");
            result.Should().Contain(u => u.Username == "user2" && u.FirstName == "Second");
            result.Should().Contain(u => u.Username == "user3" && u.FirstName == "Third");
        }

        [Test]
        public async Task Handle_UserWith2FAEnabled_ReturnsTwoFactorEnabledTrue()
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
            
            var query = new GetAllUsersQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            
            var userDto = result.First();
            userDto.TwoFactorEnabled.Should().BeTrue();
        }

        [Test]
        public async Task Handle_UsersWithNullNames_ReturnsUsersWithNullNames()
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
            
            var query = new GetAllUsersQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            
            var userDto = result.First();
            userDto.FirstName.Should().BeNull();
            userDto.LastName.Should().BeNull();
            userDto.Username.Should().Be("testuser");
        }

        [Test]
        public async Task Handle_UsersWithEmptyNames_ReturnsUsersWithEmptyNames()
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
            
            var query = new GetAllUsersQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            
            var userDto = result.First();
            userDto.FirstName.Should().Be("");
            userDto.LastName.Should().Be("");
            userDto.Username.Should().Be("testuser");
        }

        [Test]
        public async Task Handle_MixedTwoFactorSettings_ReturnsCorrectTwoFactorStatuses()
        {
            // Arrange
            var user1 = new ApplicationUser { UserName = "user1", Email = "user1@example.com", FirstName = "User", LastName = "One" };
            var user2 = new ApplicationUser { UserName = "user2", Email = "user2@example.com", FirstName = "User", LastName = "Two" };
            
            await _userManager.CreateAsync(user1, "TestPassword123!");
            await _userManager.CreateAsync(user2, "TestPassword123!");
            
            await _userManager.SetTwoFactorEnabledAsync(user1, true);
            await _userManager.SetTwoFactorEnabledAsync(user2, false);
            
            var query = new GetAllUsersQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            
            var user1Dto = result.First(u => u.Username == "user1");
            var user2Dto = result.First(u => u.Username == "user2");
            
            user1Dto.TwoFactorEnabled.Should().BeTrue();
            user2Dto.TwoFactorEnabled.Should().BeFalse();
        }
    }
}