using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users;
using OpenAlprWebhookProcessor.Features.Users.Commands.CreateUser;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Users.Services;
using Tests.TestHelpers;

namespace Tests.Features.Users.Commands
{
    [TestFixture]
    public class CreateUserCommandHandlerTests : TestBase
    {
        private CreateUserCommandHandler _handler;
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
            
            _handler = new CreateUserCommandHandler(
                _mockUsersUnitOfWork,
                _mockPasswordService);
        }

        [TearDown]
        public override void TearDown()
        {
            _mockUsersUnitOfWork.Dispose();
        }

        [Test]
        public async Task Handle_ValidUser_CreatesUserSuccessfully()
        {
            // Arrange
            var command = new CreateUserCommand("John", "Doe", "johndoe", "password123");
            var passwordHash = new byte[] { 1, 2, 3 };
            var passwordSalt = new byte[] { 4, 5, 6 };
            
            _mockUserRepository.UsernameExistsAsync(command.Username, Arg.Any<CancellationToken>())
                .Returns(false);
            _mockPasswordService.When(x => x.CreatePasswordHash(command.Password, out Arg.Any<byte[]>(), out Arg.Any<byte[]>()))
                .Do(x => {
                    x[1] = passwordHash;
                    x[2] = passwordSalt;
                });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.FirstName.Should().Be(command.FirstName);
            result.LastName.Should().Be(command.LastName);
            result.Username.Should().Be(command.Username);
            result.PasswordHash.Should().BeEquivalentTo(passwordHash);
            result.PasswordSalt.Should().BeEquivalentTo(passwordSalt);
            result.RefreshTokens.Should().NotBeNull();
            result.RefreshTokens.Should().BeEmpty();
            
            await _mockUserRepository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
            await _mockUsersUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_EmptyPassword_ThrowsAppException()
        {
            // Arrange
            var command = new CreateUserCommand("John", "Doe", "johndoe", "");

            // Act & Assert
            var exception = Assert.ThrowsAsync<AppException>(
                () => _handler.Handle(command, CancellationToken.None));
            
            exception.Message.Should().Be("Password is required");
            await _mockUserRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_NullPassword_ThrowsAppException()
        {
            // Arrange
            var command = new CreateUserCommand("John", "Doe", "johndoe", null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<AppException>(
                () => _handler.Handle(command, CancellationToken.None));
            
            exception.Message.Should().Be("Password is required");
            await _mockUserRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_WhitespacePassword_ThrowsAppException()
        {
            // Arrange
            var command = new CreateUserCommand("John", "Doe", "johndoe", "   ");

            // Act & Assert
            var exception = Assert.ThrowsAsync<AppException>(
                () => _handler.Handle(command, CancellationToken.None));
            
            exception.Message.Should().Be("Password is required");
            await _mockUserRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_ExistingUsername_ThrowsAppException()
        {
            // Arrange
            var command = new CreateUserCommand("John", "Doe", "existinguser", "password123");
            
            _mockUserRepository.UsernameExistsAsync(command.Username, Arg.Any<CancellationToken>())
                .Returns(true);

            // Act & Assert
            var exception = Assert.ThrowsAsync<AppException>(
                () => _handler.Handle(command, CancellationToken.None));
            
            exception.Message.Should().Be("Username \"existinguser\" is already taken");
            await _mockUserRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_ValidUser_CallsPasswordServiceCorrectly()
        {
            // Arrange
            var command = new CreateUserCommand("John", "Doe", "johndoe", "password123");
            
            _mockUserRepository.UsernameExistsAsync(command.Username, Arg.Any<CancellationToken>())
                .Returns(false);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockPasswordService.Received(1).CreatePasswordHash(
                command.Password, 
                out Arg.Any<byte[]>(), 
                out Arg.Any<byte[]>());
        }

        [Test]
        public async Task Handle_ValidUser_ChecksUsernameExistence()
        {
            // Arrange
            var command = new CreateUserCommand("John", "Doe", "johndoe", "password123");
            
            _mockUserRepository.UsernameExistsAsync(command.Username, Arg.Any<CancellationToken>())
                .Returns(false);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            await _mockUserRepository.Received(1).UsernameExistsAsync(command.Username, Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_CreatedUser_HasCorrectProperties()
        {
            // Arrange
            var command = new CreateUserCommand("John", "Doe", "johndoe", "password123");
            var passwordHash = new byte[] { 1, 2, 3 };
            var passwordSalt = new byte[] { 4, 5, 6 };
            User capturedUser = null;
            
            _mockUserRepository.UsernameExistsAsync(command.Username, Arg.Any<CancellationToken>())
                .Returns(false);
            _mockPasswordService.When(x => x.CreatePasswordHash(command.Password, out Arg.Any<byte[]>(), out Arg.Any<byte[]>()))
                .Do(x => {
                    x[1] = passwordHash;
                    x[2] = passwordSalt;
                });
            
            await _mockUserRepository.AddAsync(Arg.Do<User>(user => capturedUser = user), Arg.Any<CancellationToken>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            capturedUser.Should().NotBeNull();
            capturedUser.FirstName.Should().Be(command.FirstName);
            capturedUser.LastName.Should().Be(command.LastName);
            capturedUser.Username.Should().Be(command.Username);
            capturedUser.PasswordHash.Should().BeEquivalentTo(passwordHash);
            capturedUser.PasswordSalt.Should().BeEquivalentTo(passwordSalt);
            capturedUser.RefreshTokens.Should().NotBeNull();
            capturedUser.RefreshTokens.Should().BeEmpty();
        }
    }
} 