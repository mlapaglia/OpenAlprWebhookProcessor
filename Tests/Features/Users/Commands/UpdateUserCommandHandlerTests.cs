using AwesomeAssertions;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users;
using OpenAlprWebhookProcessor.Features.Users.Commands.UpdateUser;
using OpenAlprWebhookProcessor.Features.Users.Services;
using Tests.TestHelpers;

namespace Tests.Features.Users.Commands
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class UpdateUserCommandHandlerTests : TestBase
    {
        private UpdateUserCommandHandler _handler;
        private IPasswordService _mockPasswordService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _mockPasswordService = Substitute.For<IPasswordService>();
            
            _handler = new UpdateUserCommandHandler(
                UsersUnitOfWork,
                _mockPasswordService);
        }

        [Test]
        public async Task Handle_ValidUpdate_UpdatesUserSuccessfully()
        {
            // Arrange
            var existingUser = TestDataFactory.CreateTestUser("originaluser", "Original", "Name");
            await UsersUnitOfWork.Users.AddAsync(existingUser, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new UpdateUserCommand(existingUser.Id, "Updated", "LastName", "updateduser", "newpassword");
            var passwordHash = new byte[] { 7, 8, 9 };
            var passwordSalt = new byte[] { 10, 11, 12 };
            
            _mockPasswordService.When(x => x.CreatePasswordHash(command.Password, out Arg.Any<byte[]>(), out Arg.Any<byte[]>()))
                .Do(x => {
                    x[1] = passwordHash;
                    x[2] = passwordSalt;
                });

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var updatedUser = await UsersUnitOfWork.Users.GetByIdAsync(existingUser.Id, GetCancellationToken());
            updatedUser.Should().NotBeNull();
            updatedUser.FirstName.Should().Be("Updated");
            updatedUser.LastName.Should().Be("LastName");
            updatedUser.Username.Should().Be("updateduser");
            updatedUser.PasswordHash.Should().BeEquivalentTo(passwordHash);
            updatedUser.PasswordSalt.Should().BeEquivalentTo(passwordSalt);
        }

        [Test]
        public void Handle_UserNotFound_ThrowsAppException()
        {
            // Arrange
            var command = new UpdateUserCommand(999, "Updated", "Name", "updateduser", "newpassword");

            // Act & Assert
            var exception = Assert.ThrowsAsync<AppException>(async () => await _handler.Handle(command, GetCancellationToken()));
            
            exception.Message.Should().Be("User not found");
        }

        [Test]
        public async Task Handle_UsernameConflict_ThrowsAppException()
        {
            // Arrange
            var existingUser1 = TestDataFactory.CreateTestUser("user1", "User", "One");
            var existingUser2 = TestDataFactory.CreateTestUser("user2", "User", "Two");
            await UsersUnitOfWork.Users.AddAsync(existingUser1, GetCancellationToken());
            await UsersUnitOfWork.Users.AddAsync(existingUser2, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new UpdateUserCommand(existingUser1.Id, "New", "Name", "user2", null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<AppException>(async () => await _handler.Handle(command, GetCancellationToken()));
            
            exception.Message.Should().Be("Username user2 is already taken");
        }

        [Test]
        public async Task Handle_SameUsername_DoesNotCheckExistence()
        {
            // Arrange
            var existingUser = TestDataFactory.CreateTestUser("keepusername", "Keep", "User");
            await UsersUnitOfWork.Users.AddAsync(existingUser, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new UpdateUserCommand(existingUser.Id, "Updated", "Name", existingUser.Username, "newpassword");
            var passwordHash = new byte[] { 1, 2, 3 };
            var passwordSalt = new byte[] { 4, 5, 6 };
            
            _mockPasswordService.When(x => x.CreatePasswordHash(command.Password, out Arg.Any<byte[]>(), out Arg.Any<byte[]>()))
                .Do(x => {
                    x[1] = passwordHash;
                    x[2] = passwordSalt;
                });

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var updatedUser = await UsersUnitOfWork.Users.GetByIdAsync(existingUser.Id, GetCancellationToken());
            updatedUser.Should().NotBeNull();
            updatedUser.Username.Should().Be("keepusername");
            updatedUser.FirstName.Should().Be("Updated");
            updatedUser.LastName.Should().Be("Name");
        }

        [Test]
        public async Task Handle_EmptyFirstName_DoesNotUpdateFirstName()
        {
            // Arrange
            var existingUser = TestDataFactory.CreateTestUser("testuser", "Original", "Name");
            await UsersUnitOfWork.Users.AddAsync(existingUser, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new UpdateUserCommand(existingUser.Id, "", "NewLast", "testuser", null);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var updatedUser = await UsersUnitOfWork.Users.GetByIdAsync(existingUser.Id, GetCancellationToken());
            updatedUser.Should().NotBeNull();
            updatedUser.FirstName.Should().Be("Original"); // Should remain unchanged
            updatedUser.LastName.Should().Be("NewLast");
        }

        [Test]
        public async Task Handle_EmptyLastName_DoesNotUpdateLastName()
        {
            // Arrange
            var existingUser = TestDataFactory.CreateTestUser("testuser", "First", "Original");
            await UsersUnitOfWork.Users.AddAsync(existingUser, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new UpdateUserCommand(existingUser.Id, "NewFirst", "", "testuser", null);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var updatedUser = await UsersUnitOfWork.Users.GetByIdAsync(existingUser.Id, GetCancellationToken());
            updatedUser.Should().NotBeNull();
            updatedUser.FirstName.Should().Be("NewFirst");
            updatedUser.LastName.Should().Be("Original"); // Should remain unchanged
        }

        [Test]
        public async Task Handle_EmptyPassword_DoesNotUpdatePassword()
        {
            // Arrange
            var existingUser = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            var originalHash = existingUser.PasswordHash;
            var originalSalt = existingUser.PasswordSalt;
            await UsersUnitOfWork.Users.AddAsync(existingUser, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new UpdateUserCommand(existingUser.Id, "Updated", "User", "testuser", "");

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var updatedUser = await UsersUnitOfWork.Users.GetByIdAsync(existingUser.Id, GetCancellationToken());
            updatedUser.Should().NotBeNull();
            updatedUser.FirstName.Should().Be("Updated");
            updatedUser.PasswordHash.Should().BeEquivalentTo(originalHash); // Should remain unchanged
            updatedUser.PasswordSalt.Should().BeEquivalentTo(originalSalt); // Should remain unchanged
        }

        [Test]
        public async Task Handle_NullPassword_DoesNotUpdatePassword()
        {
            // Arrange
            var existingUser = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            var originalHash = existingUser.PasswordHash;
            var originalSalt = existingUser.PasswordSalt;
            await UsersUnitOfWork.Users.AddAsync(existingUser, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new UpdateUserCommand(existingUser.Id, "Updated", "User", "testuser", null);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var updatedUser = await UsersUnitOfWork.Users.GetByIdAsync(existingUser.Id, GetCancellationToken());
            updatedUser.Should().NotBeNull();
            updatedUser.FirstName.Should().Be("Updated");
            updatedUser.PasswordHash.Should().BeEquivalentTo(originalHash); // Should remain unchanged
            updatedUser.PasswordSalt.Should().BeEquivalentTo(originalSalt); // Should remain unchanged
        }

        [Test]
        public async Task Handle_ValidPasswordUpdate_UpdatesPassword()
        {
            // Arrange
            var existingUser = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            await UsersUnitOfWork.Users.AddAsync(existingUser, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new UpdateUserCommand(existingUser.Id, "Test", "User", "testuser", "newpassword");
            var passwordHash = new byte[] { 5, 6, 7 };
            var passwordSalt = new byte[] { 8, 9, 10 };
            
            _mockPasswordService.When(x => x.CreatePasswordHash(command.Password, out Arg.Any<byte[]>(), out Arg.Any<byte[]>()))
                .Do(x => {
                    x[1] = passwordHash;
                    x[2] = passwordSalt;
                });

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var updatedUser = await UsersUnitOfWork.Users.GetByIdAsync(existingUser.Id, GetCancellationToken());
            updatedUser.Should().NotBeNull();
            updatedUser.PasswordHash.Should().BeEquivalentTo(passwordHash);
            updatedUser.PasswordSalt.Should().BeEquivalentTo(passwordSalt);
            
            _mockPasswordService.Received(1).CreatePasswordHash(
                command.Password, 
                out Arg.Any<byte[]>(), 
                out Arg.Any<byte[]>());
        }

        [Test]
        public async Task Handle_WithCancellationToken_UsesTokenCorrectly()
        {
            // Arrange
            var existingUser = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            await UsersUnitOfWork.Users.AddAsync(existingUser, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new UpdateUserCommand(existingUser.Id, "Updated", "User", "newusername", null);
            var cancellationToken = GetCancellationToken();

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            var updatedUser = await UsersUnitOfWork.Users.GetByIdAsync(existingUser.Id, cancellationToken);
            updatedUser.Should().NotBeNull();
            updatedUser.Username.Should().Be("newusername");
            updatedUser.FirstName.Should().Be("Updated");
        }

        [Test]
        public async Task Handle_UpdateMultipleFields_UpdatesCorrectly()
        {
            // Arrange
            var existingUser = TestDataFactory.CreateTestUser("olduser", "Old", "Name");
            await UsersUnitOfWork.Users.AddAsync(existingUser, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new UpdateUserCommand(existingUser.Id, "New", "LastName", "newuser", "newpassword");
            var passwordHash = new byte[] { 11, 12, 13 };
            var passwordSalt = new byte[] { 14, 15, 16 };
            
            _mockPasswordService.When(x => x.CreatePasswordHash(command.Password, out Arg.Any<byte[]>(), out Arg.Any<byte[]>()))
                .Do(x => {
                    x[1] = passwordHash;
                    x[2] = passwordSalt;
                });

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var updatedUser = await UsersUnitOfWork.Users.GetByIdAsync(existingUser.Id, GetCancellationToken());
            updatedUser.Should().NotBeNull();
            updatedUser.FirstName.Should().Be("New");
            updatedUser.LastName.Should().Be("LastName");
            updatedUser.Username.Should().Be("newuser");
            updatedUser.PasswordHash.Should().BeEquivalentTo(passwordHash);
            updatedUser.PasswordSalt.Should().BeEquivalentTo(passwordSalt);
        }
    }
} 