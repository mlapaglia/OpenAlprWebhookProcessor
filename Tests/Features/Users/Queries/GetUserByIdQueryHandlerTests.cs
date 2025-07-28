using AwesomeAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetUserById;
using Tests.TestHelpers;

namespace Tests.Features.Users.Queries
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class GetUserByIdQueryHandlerTests : TestBase
    {
        private GetUserByIdQueryHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new GetUserByIdQueryHandler(UsersUnitOfWork);
        }

        [Test]
        public async Task Handle_ExistingUser_ReturnsUser()
        {
            // Arrange
            var expectedUser = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            await UsersUnitOfWork.Users.AddAsync(expectedUser, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var query = new GetUserByIdQuery(expectedUser.Id);

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(expectedUser.Id);
            result.Username.Should().Be(expectedUser.Username);
            result.FirstName.Should().Be(expectedUser.FirstName);
            result.LastName.Should().Be(expectedUser.LastName);
        }

        [Test]
        public async Task Handle_NonExistentUser_ReturnsNull()
        {
            // Arrange
            var query = new GetUserByIdQuery(999);

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task Handle_ValidId_PassesCancellationToken()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var query = new GetUserByIdQuery(user.Id);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
        }

        [Test]
        public async Task Handle_ZeroId_ReturnsNull()
        {
            // Arrange
            var query = new GetUserByIdQuery(0);

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task Handle_NegativeId_ReturnsNull()
        {
            // Arrange
            var query = new GetUserByIdQuery(-1);

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task Handle_MultipleUsersExist_ReturnsCorrectUser()
        {
            // Arrange
            var user1 = TestDataFactory.CreateTestUser("user1", "First1", "Last1");
            var user2 = TestDataFactory.CreateTestUser("user2", "First2", "Last2");
            var user3 = TestDataFactory.CreateTestUser("user3", "First3", "Last3");

            await UsersUnitOfWork.Users.AddAsync(user1, GetCancellationToken());
            await UsersUnitOfWork.Users.AddAsync(user2, GetCancellationToken());
            await UsersUnitOfWork.Users.AddAsync(user3, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var query = new GetUserByIdQuery(user2.Id);

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(user2.Id);
            result.Username.Should().Be("user2");
            result.FirstName.Should().Be("First2");
            result.LastName.Should().Be("Last2");
        }

        [Test]
        public async Task Handle_UserWithAllProperties_ReturnsCompleteUser()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("completeuser", "Complete", "User");
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var query = new GetUserByIdQuery(user.Id);

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
            result.Username.Should().Be(user.Username);
            result.FirstName.Should().Be(user.FirstName);
            result.LastName.Should().Be(user.LastName);
            result.PasswordHash.Should().BeEquivalentTo(user.PasswordHash);
            result.PasswordSalt.Should().BeEquivalentTo(user.PasswordSalt);
        }

        [Test]
        public async Task Handle_DeletedUser_ReturnsNull()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("tobedeleted", "Delete", "Me");
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var userId = user.Id;

            // Delete the user
            UsersUnitOfWork.Users.Delete(user);
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var query = new GetUserByIdQuery(userId);

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task Handle_SameIdMultipleCalls_ReturnsConsistentResult()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("consistentuser", "Consistent", "User");
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var query = new GetUserByIdQuery(user.Id);

            // Act
            var result1 = await _handler.Handle(query, GetCancellationToken());
            var result2 = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result1.Should().NotBeNull();
            result2.Should().NotBeNull();
            result1.Id.Should().Be(result2.Id);
            result1.Username.Should().Be(result2.Username);
            result1.FirstName.Should().Be(result2.FirstName);
            result1.LastName.Should().Be(result2.LastName);
        }

        [Test]
        public async Task Handle_UserWithEmptyFields_ReturnsUserWithEmptyFields()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("emptyuser", "", "");
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var query = new GetUserByIdQuery(user.Id);

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Username.Should().Be("emptyuser");
            result.FirstName.Should().Be("");
            result.LastName.Should().Be("");
        }

        [Test]
        public async Task Handle_LargeId_ReturnsNull()
        {
            // Arrange
            var query = new GetUserByIdQuery(int.MaxValue);

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().BeNull();
        }
    }
} 