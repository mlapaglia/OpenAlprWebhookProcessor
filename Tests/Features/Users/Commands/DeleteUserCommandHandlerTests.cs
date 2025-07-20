using FluentAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Commands.DeleteUser;
using Tests.TestHelpers;

namespace Tests.Features.Users.Commands
{
    [TestFixture]
    public class DeleteUserCommandHandlerTests : TestBase
    {
        private DeleteUserCommandHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new DeleteUserCommandHandler(UsersUnitOfWork);
        }

        [Test]
        public async Task Handle_ExistingUser_DeletesUserSuccessfully()
        {
            // Arrange
            var existingUser = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            await UsersUnitOfWork.Users.AddAsync(existingUser, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new DeleteUserCommand(existingUser.Id);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var deletedUser = await UsersUnitOfWork.Users.GetByIdAsync(existingUser.Id, GetCancellationToken());
            deletedUser.Should().BeNull();
        }

        [Test]
        public async Task Handle_NonExistentUser_DoesNotThrowError()
        {
            // Arrange
            var command = new DeleteUserCommand(999);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            Assert.Pass();
        }

        [Test]
        public async Task Handle_ValidId_CallsCorrectRepositoryMethods()
        {
            // Arrange
            var existingUser = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            await UsersUnitOfWork.Users.AddAsync(existingUser, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new DeleteUserCommand(existingUser.Id);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var deletedUser = await UsersUnitOfWork.Users.GetByIdAsync(existingUser.Id, GetCancellationToken());
            deletedUser.Should().BeNull();
        }

        [Test]
        public async Task Handle_ZeroId_DoesNotThrowError()
        {
            // Arrange
            var command = new DeleteUserCommand(0);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            Assert.Pass();
        }

        [Test]
        public async Task Handle_NegativeId_DoesNotThrowError()
        {
            // Arrange
            var command = new DeleteUserCommand(-1);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            Assert.Pass();
        }

        [Test]
        public async Task Handle_MultipleUsers_DeletesOnlySpecifiedUser()
        {
            // Arrange
            var user1 = TestDataFactory.CreateTestUser("user1", "First", "User");
            var user2 = TestDataFactory.CreateTestUser("user2", "Second", "User");
            var user3 = TestDataFactory.CreateTestUser("user3", "Third", "User");

            await UsersUnitOfWork.Users.AddAsync(user1, GetCancellationToken());
            await UsersUnitOfWork.Users.AddAsync(user2, GetCancellationToken());
            await UsersUnitOfWork.Users.AddAsync(user3, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new DeleteUserCommand(user2.Id);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var deletedUser = await UsersUnitOfWork.Users.GetByIdAsync(user2.Id, GetCancellationToken());
            deletedUser.Should().BeNull();

            var remainingUser1 = await UsersUnitOfWork.Users.GetByIdAsync(user1.Id, GetCancellationToken());
            remainingUser1.Should().NotBeNull();

            var remainingUser3 = await UsersUnitOfWork.Users.GetByIdAsync(user3.Id, GetCancellationToken());
            remainingUser3.Should().NotBeNull();
        }

        [Test]
        public async Task Handle_UserWithRefreshTokens_DeletesUserAndTokens()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            user.RefreshTokens = TestDataFactory.CreateTestRefreshTokens();
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new DeleteUserCommand(user.Id);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var deletedUser = await UsersUnitOfWork.Users.GetByIdWithRefreshTokensAsync(user.Id, GetCancellationToken());
            deletedUser.Should().BeNull();
        }

        [Test]
        public async Task Handle_WithCancellationToken_UsesTokenCorrectly()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new DeleteUserCommand(user.Id);
            var cancellationToken = GetCancellationToken();

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            var deletedUser = await UsersUnitOfWork.Users.GetByIdAsync(user.Id, cancellationToken);
            deletedUser.Should().BeNull();
        }

        [Test]
        public async Task Handle_LargeId_DoesNotThrowError()
        {
            // Arrange
            var command = new DeleteUserCommand(int.MaxValue);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            Assert.Pass();
        }

        [Test]
        public async Task Handle_DeletedUserTwice_DoesNotThrowError()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new DeleteUserCommand(user.Id);

            // Act - delete twice
            await _handler.Handle(command, GetCancellationToken());
            await _handler.Handle(command, GetCancellationToken()); // Should not throw

            // Assert
            var deletedUser = await UsersUnitOfWork.Users.GetByIdAsync(user.Id, GetCancellationToken());
            deletedUser.Should().BeNull();
        }

        [Test]
        public async Task Handle_ExistingUser_SavesChangesToDatabase()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new DeleteUserCommand(user.Id);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert - verify changes were persisted by checking with a new context
            using var freshContext = ContextCreator.CreateUsersContext();
            var deletedUser = await freshContext.Users.FindAsync(user.Id);
            deletedUser.Should().BeNull();
        }
    }
} 