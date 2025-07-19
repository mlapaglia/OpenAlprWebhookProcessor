using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users;
using OpenAlprWebhookProcessor.Features.Users.Commands.CreateUser;
using OpenAlprWebhookProcessor.Features.Users.Services;
using Tests.TestHelpers;

namespace Tests.Features.Users.Commands
{
    [TestFixture]
    public class CreateUserCommandHandlerTests : TestBase
    {
        private CreateUserCommandHandler _handler;
        private IPasswordService _mockPasswordService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _mockPasswordService = Substitute.For<IPasswordService>();
            
            _handler = new CreateUserCommandHandler(
                UsersUnitOfWork,
                _mockPasswordService);
        }

        [Test]
        public async Task Handle_ValidUser_CreatesUserSuccessfully()
        {
            // Arrange
            var command = new CreateUserCommand("John", "Doe", "johndoe", "password123");
            var passwordHash = new byte[] { 1, 2, 3 };
            var passwordSalt = new byte[] { 4, 5, 6 };
            
            _mockPasswordService.When(x => x.CreatePasswordHash(command.Password, out Arg.Any<byte[]>(), out Arg.Any<byte[]>()))
                .Do(x => {
                    x[1] = passwordHash;
                    x[2] = passwordSalt;
                });

            // Act
            var result = await _handler.Handle(command, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBe(0);
            result.FirstName.Should().Be("John");
            result.LastName.Should().Be("Doe");
            result.Username.Should().Be("johndoe");
            result.PasswordHash.Should().BeEquivalentTo(passwordHash);
            result.PasswordSalt.Should().BeEquivalentTo(passwordSalt);
            result.RefreshTokens.Should().NotBeNull();
            result.RefreshTokens.Should().BeEmpty();

            // Verify user was saved to database
            var savedUser = await UsersUnitOfWork.Users.GetByIdAsync(result.Id, GetCancellationToken());
            savedUser.Should().NotBeNull();
            savedUser.FirstName.Should().Be("John");
            savedUser.LastName.Should().Be("Doe");
            savedUser.Username.Should().Be("johndoe");
            savedUser.PasswordHash.Should().BeEquivalentTo(passwordHash);
            savedUser.PasswordSalt.Should().BeEquivalentTo(passwordSalt);
        }

        [Test]
        public async Task Handle_UsernameAlreadyExists_ThrowsAppException()
        {
            // Arrange
            var existingUser = TestDataFactory.CreateTestUser("existinguser", "Existing", "User");
            await UsersUnitOfWork.Users.AddAsync(existingUser, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new CreateUserCommand("John", "Doe", "existinguser", "password123");

            // Act & Assert
            var exception = Assert.ThrowsAsync<AppException>(() => 
                _handler.Handle(command, GetCancellationToken()));

            exception.Message.Should().Be("Username \"existinguser\" is already taken");
        }

        [Test]
        public void Handle_EmptyPassword_ThrowsAppException()
        {
            // Arrange
            var command = new CreateUserCommand("John", "Doe", "johndoe", "");

            // Act & Assert
            var exception = Assert.ThrowsAsync<AppException>(() => 
                _handler.Handle(command, GetCancellationToken()));

            exception.Message.Should().Be("Password is required");
        }

        [Test]
        public void Handle_NullPassword_ThrowsAppException()
        {
            // Arrange
            var command = new CreateUserCommand("John", "Doe", "johndoe", null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<AppException>(() => 
                _handler.Handle(command, GetCancellationToken()));

            exception.Message.Should().Be("Password is required");
        }

        [Test]
        public void Handle_WhitespacePassword_ThrowsAppException()
        {
            // Arrange
            var command = new CreateUserCommand("John", "Doe", "johndoe", "   ");

            // Act & Assert
            var exception = Assert.ThrowsAsync<AppException>(() => 
                _handler.Handle(command, GetCancellationToken()));

            exception.Message.Should().Be("Password is required");
        }

        [Test]
        public async Task Handle_EmptyFirstName_CreatesUserWithEmptyFirstName()
        {
            // Arrange
            var command = new CreateUserCommand("", "Doe", "johndoe", "password123");
            var passwordHash = new byte[] { 1, 2, 3 };
            var passwordSalt = new byte[] { 4, 5, 6 };
            
            _mockPasswordService.When(x => x.CreatePasswordHash(command.Password, out Arg.Any<byte[]>(), out Arg.Any<byte[]>()))
                .Do(x => {
                    x[1] = passwordHash;
                    x[2] = passwordSalt;
                });

            // Act
            var result = await _handler.Handle(command, GetCancellationToken());

            // Assert
            result.FirstName.Should().Be("");
            
            var savedUser = await UsersUnitOfWork.Users.GetByIdAsync(result.Id, GetCancellationToken());
            savedUser.FirstName.Should().Be("");
        }

        [Test]
        public async Task Handle_EmptyLastName_CreatesUserWithEmptyLastName()
        {
            // Arrange
            var command = new CreateUserCommand("John", "", "johndoe", "password123");
            var passwordHash = new byte[] { 1, 2, 3 };
            var passwordSalt = new byte[] { 4, 5, 6 };
            
            _mockPasswordService.When(x => x.CreatePasswordHash(command.Password, out Arg.Any<byte[]>(), out Arg.Any<byte[]>()))
                .Do(x => {
                    x[1] = passwordHash;
                    x[2] = passwordSalt;
                });

            // Act
            var result = await _handler.Handle(command, GetCancellationToken());

            // Assert
            result.LastName.Should().Be("");
            
            var savedUser = await UsersUnitOfWork.Users.GetByIdAsync(result.Id, GetCancellationToken());
            savedUser.LastName.Should().Be("");
        }

        [Test]
        public async Task Handle_SameUsernameExactCase_ThrowsAppException()
        {
            // Arrange
            var existingUser = TestDataFactory.CreateTestUser("johndoe", "Existing", "User");
            await UsersUnitOfWork.Users.AddAsync(existingUser, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new CreateUserCommand("John", "Doe", "johndoe", "password123");

            // Act & Assert
            var exception = Assert.ThrowsAsync<AppException>(() => 
                _handler.Handle(command, GetCancellationToken()));

            exception.Message.Should().Be("Username \"johndoe\" is already taken");
        }

        [Test]
        public async Task Handle_ValidCommand_CallsPasswordService()
        {
            // Arrange
            var command = new CreateUserCommand("John", "Doe", "johndoe", "password123");
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
            _mockPasswordService.Received(1).CreatePasswordHash(
                command.Password, 
                out Arg.Any<byte[]>(), 
                out Arg.Any<byte[]>());
        }

        [Test]
        public async Task Handle_MultipleUsers_CreatesAllUsersSuccessfully()
        {
            // Arrange
            var passwordHash = new byte[] { 1, 2, 3 };
            var passwordSalt = new byte[] { 4, 5, 6 };
            
            _mockPasswordService.When(x => x.CreatePasswordHash(Arg.Any<string>(), out Arg.Any<byte[]>(), out Arg.Any<byte[]>()))
                .Do(x => {
                    x[1] = passwordHash;
                    x[2] = passwordSalt;
                });

            var command1 = new CreateUserCommand("John", "Doe", "johndoe", "password123");
            var command2 = new CreateUserCommand("Jane", "Smith", "janesmith", "password456");

            // Act
            var result1 = await _handler.Handle(command1, GetCancellationToken());
            var result2 = await _handler.Handle(command2, GetCancellationToken());

            // Assert
            result1.Should().NotBeNull();
            result2.Should().NotBeNull();
            result1.Id.Should().NotBe(result2.Id);
            result1.Username.Should().Be("johndoe");
            result2.Username.Should().Be("janesmith");

            var allUsers = await UsersUnitOfWork.Users.GetAllAsync(GetCancellationToken());
            allUsers.Should().HaveCount(2);
        }

        [Test]
        public async Task Handle_WithCancellationToken_UsesTokenCorrectly()
        {
            // Arrange
            var command = new CreateUserCommand("John", "Doe", "johndoe", "password123");
            var passwordHash = new byte[] { 1, 2, 3 };
            var passwordSalt = new byte[] { 4, 5, 6 };
            var cancellationToken = GetCancellationToken();
            
            _mockPasswordService.When(x => x.CreatePasswordHash(command.Password, out Arg.Any<byte[]>(), out Arg.Any<byte[]>()))
                .Do(x => {
                    x[1] = passwordHash;
                    x[2] = passwordSalt;
                });

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            
            // Verify the user was created successfully with the cancellation token
            var savedUser = await UsersUnitOfWork.Users.GetByIdAsync(result.Id, cancellationToken);
            savedUser.Should().NotBeNull();
        }
    }
} 