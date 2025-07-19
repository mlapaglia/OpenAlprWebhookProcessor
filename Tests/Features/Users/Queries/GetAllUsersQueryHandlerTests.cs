using FluentAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetAllUsers;
using Tests.TestHelpers;

namespace Tests.Features.Users.Queries
{
    [TestFixture]
    public class GetAllUsersQueryHandlerTests : TestBase
    {
        private GetAllUsersQueryHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new GetAllUsersQueryHandler(UsersUnitOfWork);
        }

        [Test]
        public async Task Handle_ValidQuery_ReturnsAllUsers()
        {
            // Arrange
            var users = new List<User>
            {
                TestDataFactory.CreateTestUser("user1", "First1", "Last1"),
                TestDataFactory.CreateTestUser("user2", "First2", "Last2"),
                TestDataFactory.CreateTestUser("user3", "First3", "Last3")
            };

            foreach (var user in users)
            {
                await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            }
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var query = new GetAllUsersQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().Contain(u => u.Username == "user1");
            result.Should().Contain(u => u.Username == "user2");
            result.Should().Contain(u => u.Username == "user3");
        }

        [Test]
        public async Task Handle_EmptyUserList_ReturnsEmptyList()
        {
            // Arrange
            var query = new GetAllUsersQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task Handle_SingleUser_ReturnsSingleUser()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("singleuser", "Single", "User");
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var query = new GetAllUsersQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Should().ContainSingle();
            result.First().Username.Should().Be("singleuser");
            result.First().FirstName.Should().Be("Single");
            result.First().LastName.Should().Be("User");
        }

        [Test]
        public async Task Handle_ValidQuery_ReturnsListNotEnumerable()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var query = new GetAllUsersQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().BeOfType<List<User>>();
            result.Should().BeAssignableTo<IEnumerable<User>>();
        }

        [Test]
        public async Task Handle_ValidQuery_PreservesUserOrder()
        {
            // Arrange
            var user1 = TestDataFactory.CreateTestUser("user1", "First1", "Last1");
            var user2 = TestDataFactory.CreateTestUser("user2", "First2", "Last2");
            var user3 = TestDataFactory.CreateTestUser("user3", "First3", "Last3");

            // Add them in a specific order
            await UsersUnitOfWork.Users.AddAsync(user1, GetCancellationToken());
            await UsersUnitOfWork.Users.AddAsync(user2, GetCancellationToken());
            await UsersUnitOfWork.Users.AddAsync(user3, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var query = new GetAllUsersQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().HaveCount(3);
            result.Should().Contain(u => u.Username == "user1");
            result.Should().Contain(u => u.Username == "user2");
            result.Should().Contain(u => u.Username == "user3");
        }

        [Test]
        public async Task Handle_ValidQuery_PassesCancellationToken()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var query = new GetAllUsersQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().ContainSingle();
            result.First().Username.Should().Be("testuser");
        }

        [Test]
        public async Task Handle_UsersWithDifferentProperties_ReturnsAllCorrectly()
        {
            // Arrange
            var users = new List<User>
            {
                TestDataFactory.CreateTestUser("admin", "Administrator", "User"),
                TestDataFactory.CreateTestUser("guest", "Guest", "Account"),
                TestDataFactory.CreateTestUser("tester", "Quality", "Assurance")
            };

            foreach (var user in users)
            {
                await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            }
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var query = new GetAllUsersQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);

            var admin = result.Should().ContainSingle(u => u.Username == "admin").Subject;
            admin.FirstName.Should().Be("Administrator");
            admin.LastName.Should().Be("User");

            var guest = result.Should().ContainSingle(u => u.Username == "guest").Subject;
            guest.FirstName.Should().Be("Guest");
            guest.LastName.Should().Be("Account");

            var tester = result.Should().ContainSingle(u => u.Username == "tester").Subject;
            tester.FirstName.Should().Be("Quality");
            tester.LastName.Should().Be("Assurance");
        }

        [Test]
        public async Task Handle_AfterUserDeletion_ReturnsRemainingUsers()
        {
            // Arrange
            var user1 = TestDataFactory.CreateTestUser("user1", "First1", "Last1");
            var user2 = TestDataFactory.CreateTestUser("user2", "First2", "Last2");
            
            await UsersUnitOfWork.Users.AddAsync(user1, GetCancellationToken());
            await UsersUnitOfWork.Users.AddAsync(user2, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            // Delete one user
            UsersUnitOfWork.Users.Delete(user1);
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var query = new GetAllUsersQuery();

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Should().ContainSingle();
            result.First().Username.Should().Be("user2");
        }

        [Test]
        public async Task Handle_MultipleQueries_ReturnsConsistentResults()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var query = new GetAllUsersQuery();

            // Act - call multiple times
            var result1 = await _handler.Handle(query, GetCancellationToken());
            var result2 = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result1.Should().HaveCount(result2.Count);
            result1.First().Username.Should().Be(result2.First().Username);
        }
    }
} 