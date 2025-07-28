using AwesomeAssertions;
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

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new CanRegisterQueryHandler(UsersUnitOfWork);
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
            var existingUser = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            await UsersUnitOfWork.Users.AddAsync(existingUser, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var query = new CanRegisterQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task Handle_SingleUser_ReturnsFalse()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("singleuser", "Single", "User");
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var query = new CanRegisterQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task Handle_MultipleUsers_ReturnsFalse()
        {
            // Arrange
            var users = new List<User>
            {
                TestDataFactory.CreateTestUser("user1", "User", "One"),
                TestDataFactory.CreateTestUser("user2", "User", "Two"),
                TestDataFactory.CreateTestUser("user3", "User", "Three")
            };

            foreach (var user in users)
            {
                await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            }
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var query = new CanRegisterQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task Handle_EmptyDatabaseAfterClear_ReturnsTrue()
        {
            // Arrange - first add a user
            var user = TestDataFactory.CreateTestUser("tempuser", "Temp", "User");
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            // Delete the user
            UsersUnitOfWork.Users.Delete(user);
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var query = new CanRegisterQuery();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task Handle_WithCancellationToken_UsesTokenCorrectly()
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
        public async Task Handle_ConsistentResults_SameQueryReturnsSameResult()
        {
            // Arrange
            var query = new CanRegisterQuery();
            var cancellationToken = GetCancellationToken();

            // Act - call multiple times
            var result1 = await _handler.Handle(query, cancellationToken);
            var result2 = await _handler.Handle(query, cancellationToken);

            // Assert
            result1.Should().Be(result2);
            result1.Should().BeTrue(); // No users exist
        }

        [Test]
        public async Task Handle_AfterAddingUser_StateChangesCorrectly()
        {
            // Arrange
            var query = new CanRegisterQuery();
            var cancellationToken = GetCancellationToken();

            // Act - before adding user
            var resultBeforeUser = await _handler.Handle(query, cancellationToken);

            // Add a user
            var user = TestDataFactory.CreateTestUser("newuser", "New", "User");
            await UsersUnitOfWork.Users.AddAsync(user, cancellationToken);
            await UsersUnitOfWork.SaveChangesAsync(cancellationToken);

            // Act - after adding user
            var resultAfterUser = await _handler.Handle(query, cancellationToken);

            // Assert
            resultBeforeUser.Should().BeTrue();
            resultAfterUser.Should().BeFalse();
        }

        [Test]
        public async Task Handle_DatabaseTransactionConsistency_WorksCorrectly()
        {
            // Arrange
            var query = new CanRegisterQuery();
            var cancellationToken = GetCancellationToken();

            // Act & Assert - Initial state
            var initialResult = await _handler.Handle(query, cancellationToken);
            initialResult.Should().BeTrue();

            // Add users in the same transaction
            var user1 = TestDataFactory.CreateTestUser("user1", "First", "User");
            var user2 = TestDataFactory.CreateTestUser("user2", "Second", "User");

            await UsersUnitOfWork.Users.AddAsync(user1, cancellationToken);
            await UsersUnitOfWork.Users.AddAsync(user2, cancellationToken);
            await UsersUnitOfWork.SaveChangesAsync(cancellationToken);

            // Check result after transaction
            var finalResult = await _handler.Handle(query, cancellationToken);
            finalResult.Should().BeFalse();

            // Verify the users exist in database
            var allUsers = await UsersUnitOfWork.Users.GetAllAsync(cancellationToken);
            allUsers.Should().HaveCount(2);
        }
    }
} 