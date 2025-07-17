using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users;
using OpenAlprWebhookProcessor.Features.Users.Commands.UpdateUser;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Users.Services;
using Tests.TestHelpers;

namespace Tests.Features.Users.Commands
{
    [TestFixture]
    public class UpdateUserCommandHandlerTests : TestBase
    {
        private UpdateUserCommandHandler _handler;
        private IUsersUnitOfWork _mockUsersUnitOfWork;
        private IUserRepository _mockUserRepository;
        private IPasswordService _mockPasswordService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _mockUsersUnitOfWork = Substitute.For<IUsersUnitOfWork>();
            _mockUserRepository = Substitute.For<IUserRepository>();
            _mockPasswordService = Substitute.For<IPasswordService>();
            
            _mockUsersUnitOfWork.Users.Returns(_mockUserRepository);
            
            _handler = new UpdateUserCommandHandler(
                _mockUsersUnitOfWork,
                _mockPasswordService);
        }

        [TearDown]
        public override void TearDown()
        {
            _mockUsersUnitOfWork.Dispose();
        }

        [Test]
        public async Task Handle_ValidUpdate_UpdatesUserSuccessfully()
        {
            // Arrange
            var existingUser = TestDataFactory.CreateTestUser();
            var command = new UpdateUserCommand(existingUser.Id, "Updated", "Name", "updateduser", "newpassword");
            var passwordHash = new byte[] { 7, 8, 9 };
            var passwordSalt = new byte[] { 10, 11, 12 };
            
            _mockUserRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
                .Returns(existingUser);
            _mockUserRepository.UsernameExistsAsync(command.Username, Arg.Any<CancellationToken>())
                .Returns(false);
            _mockPasswordService.When(x => x.CreatePasswordHash(command.Password, out Arg.Any<byte[]>(), out Arg.Any<byte[]>()))
                .Do(x => {
                    x[1] = passwordHash;
                    x[2] = passwordSalt;
                });

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            existingUser.FirstName.Should().Be(command.FirstName);
            existingUser.LastName.Should().Be(command.LastName);
            existingUser.Username.Should().Be(command.Username);
            existingUser.PasswordHash.Should().BeEquivalentTo(passwordHash);
            existingUser.PasswordSalt.Should().BeEquivalentTo(passwordSalt);
            
            _mockUserRepository.Received(1).Update(existingUser);
            await _mockUsersUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_UserNotFound_ThrowsAppException()
        {
            // Arrange
            var command = new UpdateUserCommand(999, "Updated", "Name", "updateduser", "newpassword");
            
            _mockUserRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
                .Returns((User)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<AppException>(
                () => _handler.Handle(command, CancellationToken.None));
            
            exception.Message.Should().Be("User not found");
            _mockUserRepository.DidNotReceive().Update(Arg.Any<User>());
        }

        [Test]
        public async Task Handle_UsernameConflict_ThrowsAppException()
        {
            // Arrange
            var existingUser = TestDataFactory.CreateTestUser();
            var command = new UpdateUserCommand(existingUser.Id, "New", "Name", "existinguser", null);
            
            _mockUserRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
                .Returns(existingUser);
            
            _mockUserRepository.UsernameExistsAsync(command.Username, Arg.Any<CancellationToken>())
                .Returns(true);

            // Act & Assert
            var exception = Assert.ThrowsAsync<AppException>(
                () => _handler.Handle(command, CancellationToken.None));
            
            exception.Message.Should().Be("Username existinguser is already taken");
        }

        [Test]
        public async Task Handle_SameUsername_DoesNotCheckExistence()
        {
            // Arrange
            var existingUser = TestDataFactory.CreateTestUser();
            var command = new UpdateUserCommand(existingUser.Id, "Updated", "Name", existingUser.Username, "newpassword");
            
            _mockUserRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
                .Returns(existingUser);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            await _mockUserRepository.DidNotReceive().UsernameExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_EmptyUsername_DoesNotUpdateUsername()
        {
            // Arrange
            var existingUser = TestDataFactory.CreateTestUser();
            var originalUsername = existingUser.Username;
            var command = new UpdateUserCommand(existingUser.Id, "Updated", "Name", "", "newpassword");
            
            _mockUserRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
                .Returns(existingUser);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            existingUser.Username.Should().Be(originalUsername);
            await _mockUserRepository.DidNotReceive().UsernameExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_EmptyFirstName_DoesNotUpdateFirstName()
        {
            // Arrange
            var existingUser = TestDataFactory.CreateTestUser();
            var originalFirstName = existingUser.FirstName;
            var command = new UpdateUserCommand(existingUser.Id, "", "Name", "updateduser", "newpassword");
            
            _mockUserRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
                .Returns(existingUser);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            existingUser.FirstName.Should().Be(originalFirstName);
        }

        [Test]
        public async Task Handle_EmptyLastName_DoesNotUpdateLastName()
        {
            // Arrange
            var existingUser = TestDataFactory.CreateTestUser();
            var originalLastName = existingUser.LastName;
            var command = new UpdateUserCommand(existingUser.Id, "Updated", "", "updateduser", "newpassword");
            
            _mockUserRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
                .Returns(existingUser);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            existingUser.LastName.Should().Be(originalLastName);
        }

        [Test]
        public async Task Handle_EmptyPassword_DoesNotUpdatePassword()
        {
            // Arrange
            var existingUser = TestDataFactory.CreateTestUser();
            var originalPasswordHash = existingUser.PasswordHash;
            var originalPasswordSalt = existingUser.PasswordSalt;
            var command = new UpdateUserCommand(existingUser.Id, "Updated", "Name", "updateduser", "");
            
            _mockUserRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
                .Returns(existingUser);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            existingUser.PasswordHash.Should().BeEquivalentTo(originalPasswordHash);
            existingUser.PasswordSalt.Should().BeEquivalentTo(originalPasswordSalt);
            _mockPasswordService.DidNotReceive().CreatePasswordHash(Arg.Any<string>(), out Arg.Any<byte[]>(), out Arg.Any<byte[]>());
        }

        [Test]
        public async Task Handle_NullPassword_DoesNotUpdatePassword()
        {
            // Arrange
            var existingUser = TestDataFactory.CreateTestUser();
            var originalPasswordHash = existingUser.PasswordHash;
            var originalPasswordSalt = existingUser.PasswordSalt;
            var command = new UpdateUserCommand(existingUser.Id, "Updated", "Name", "updateduser", null);
            
            _mockUserRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
                .Returns(existingUser);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            existingUser.PasswordHash.Should().BeEquivalentTo(originalPasswordHash);
            existingUser.PasswordSalt.Should().BeEquivalentTo(originalPasswordSalt);
            _mockPasswordService.DidNotReceive().CreatePasswordHash(Arg.Any<string>(), out Arg.Any<byte[]>(), out Arg.Any<byte[]>());
        }

        [Test]
        public async Task Handle_WhitespacePassword_DoesNotUpdatePassword()
        {
            // Arrange
            var existingUser = TestDataFactory.CreateTestUser();
            var originalPasswordHash = existingUser.PasswordHash;
            var originalPasswordSalt = existingUser.PasswordSalt;
            var command = new UpdateUserCommand(existingUser.Id, "Updated", "Name", "updateduser", "   ");
            
            _mockUserRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
                .Returns(existingUser);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            existingUser.PasswordHash.Should().BeEquivalentTo(originalPasswordHash);
            existingUser.PasswordSalt.Should().BeEquivalentTo(originalPasswordSalt);
            _mockPasswordService.DidNotReceive().CreatePasswordHash(Arg.Any<string>(), out Arg.Any<byte[]>(), out Arg.Any<byte[]>());
        }

        [Test]
        public async Task Handle_ValidPassword_UpdatesPassword()
        {
            // Arrange
            var existingUser = TestDataFactory.CreateTestUser();
            var command = new UpdateUserCommand(existingUser.Id, "Updated", "Name", "updateduser", "newpassword");
            var passwordHash = new byte[] { 7, 8, 9 };
            var passwordSalt = new byte[] { 10, 11, 12 };
            
            _mockUserRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
                .Returns(existingUser);
            _mockPasswordService.When(x => x.CreatePasswordHash(command.Password, out Arg.Any<byte[]>(), out Arg.Any<byte[]>()))
                .Do(x => {
                    x[1] = passwordHash;
                    x[2] = passwordSalt;
                });

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            existingUser.PasswordHash.Should().BeEquivalentTo(passwordHash);
            existingUser.PasswordSalt.Should().BeEquivalentTo(passwordSalt);
            _mockPasswordService.Received(1).CreatePasswordHash(
                command.Password, 
                out Arg.Any<byte[]>(), 
                out Arg.Any<byte[]>());
        }
    }
} 