using AwesomeAssertions;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users;
using OpenAlprWebhookProcessor.Features.Users.Commands.Authenticate;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users.Services;
using Tests.TestHelpers;

namespace Tests.Features.Users.Commands
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class AuthenticateCommandHandlerTests : TestBase
    {
        private AuthenticateCommandHandler _handler;
        private IJwtService _mockJwtService;
        private IPasswordService _mockPasswordService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _mockJwtService = Substitute.For<IJwtService>();
            _mockPasswordService = Substitute.For<IPasswordService>();
            
            _handler = new AuthenticateCommandHandler(
                UsersUnitOfWork,
                _mockJwtService,
                _mockPasswordService);
        }

        [Test]
        public async Task Handle_ValidCredentials_ReturnsAuthenticateResponse()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new AuthenticateCommand("testuser", "password123", "127.0.0.1");
            var jwtToken = "test-jwt-token";
            var refreshToken = new RefreshToken
            {
                Token = "refresh-token",
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = "127.0.0.1"
            };

            _mockPasswordService.VerifyPasswordHash(command.Password, user.PasswordHash, user.PasswordSalt)
                .Returns(true);
            _mockJwtService.GenerateJwtTokenAsync(user, Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(jwtToken);
            _mockJwtService.GenerateRefreshToken(command.IpAddress)
                .Returns(refreshToken);

            // Act
            var result = await _handler.Handle(command, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
            result.Username.Should().Be(user.Username);
            result.FirstName.Should().Be(user.FirstName);
            result.LastName.Should().Be(user.LastName);
            result.JwtToken.Should().Be(jwtToken);
            result.RefreshToken.Should().Be(refreshToken.Token);

            // Verify refresh token was added to user
            var updatedUser = await UsersUnitOfWork.Users.GetByUsernameAsync("testuser", GetCancellationToken());
            updatedUser.RefreshTokens.Should().ContainSingle();
            updatedUser.RefreshTokens.First().Token.Should().Be(refreshToken.Token);
        }

        [Test]
        public async Task Handle_UserNotFound_ReturnsNull()
        {
            // Arrange
            var command = new AuthenticateCommand("nonexistent", "password123", "127.0.0.1");

            // Act
            var result = await _handler.Handle(command, GetCancellationToken());

            // Assert
            result.Should().BeNull();
            _mockPasswordService.DidNotReceive().VerifyPasswordHash(Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<byte[]>());
            await _mockJwtService.DidNotReceive().GenerateJwtTokenAsync(Arg.Any<User>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_InvalidPassword_ReturnsNull()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new AuthenticateCommand("testuser", "wrongpassword", "127.0.0.1");

            _mockPasswordService.VerifyPasswordHash(command.Password, user.PasswordHash, user.PasswordSalt)
                .Returns(false);

            // Act
            var result = await _handler.Handle(command, GetCancellationToken());

            // Assert
            result.Should().BeNull();
            await _mockJwtService.DidNotReceive().GenerateJwtTokenAsync(Arg.Any<User>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_ValidCredentials_CallsPasswordService()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new AuthenticateCommand("testuser", "password123", "127.0.0.1");

            _mockPasswordService.VerifyPasswordHash(command.Password, user.PasswordHash, user.PasswordSalt)
                .Returns(true);
            _mockJwtService.GenerateJwtTokenAsync(user, Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns("jwt-token");
            _mockJwtService.GenerateRefreshToken(command.IpAddress)
                .Returns(new RefreshToken { Token = "refresh-token", Expires = DateTime.UtcNow.AddDays(7), Created = DateTime.UtcNow, CreatedByIp = command.IpAddress });

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            _mockPasswordService.Received(1).VerifyPasswordHash(command.Password, user.PasswordHash, user.PasswordSalt);
        }

        [Test]
        public async Task Handle_ValidCredentials_CallsJwtService()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new AuthenticateCommand("testuser", "password123", "127.0.0.1");

            _mockPasswordService.VerifyPasswordHash(command.Password, user.PasswordHash, user.PasswordSalt)
                .Returns(true);
            _mockJwtService.GenerateJwtTokenAsync(user, Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns("jwt-token");
            _mockJwtService.GenerateRefreshToken(command.IpAddress)
                .Returns(new RefreshToken { Token = "refresh-token", Expires = DateTime.UtcNow.AddDays(7), Created = DateTime.UtcNow, CreatedByIp = command.IpAddress });

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            await _mockJwtService.Received(1).GenerateJwtTokenAsync(user, Arg.Any<bool>(), Arg.Any<CancellationToken>());
            _mockJwtService.Received(1).GenerateRefreshToken(command.IpAddress);
        }

        [Test]
        public async Task Handle_ValidCredentials_AddsRefreshTokenToUser()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new AuthenticateCommand("testuser", "password123", "127.0.0.1");
            var refreshToken = new RefreshToken
            {
                Token = "refresh-token",
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = "127.0.0.1"
            };

            _mockPasswordService.VerifyPasswordHash(command.Password, user.PasswordHash, user.PasswordSalt)
                .Returns(true);
            _mockJwtService.GenerateJwtTokenAsync(user, Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns("jwt-token");
            _mockJwtService.GenerateRefreshToken(command.IpAddress)
                .Returns(refreshToken);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var updatedUser = await UsersUnitOfWork.Users.GetByUsernameAsync("testuser", GetCancellationToken());
            updatedUser.RefreshTokens.Should().ContainSingle();
            updatedUser.RefreshTokens.First().Token.Should().Be(refreshToken.Token);
            updatedUser.RefreshTokens.First().CreatedByIp.Should().Be("127.0.0.1");
        }

        [Test]
        public async Task Handle_UserWithNullRefreshTokens_InitializesRefreshTokensList()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            user.RefreshTokens = null; // Explicitly set to null
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new AuthenticateCommand("testuser", "password123", "127.0.0.1");
            var refreshToken = new RefreshToken
            {
                Token = "refresh-token",
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = "127.0.0.1"
            };

            _mockPasswordService.VerifyPasswordHash(command.Password, user.PasswordHash, user.PasswordSalt)
                .Returns(true);
            _mockJwtService.GenerateJwtTokenAsync(user, Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns("jwt-token");
            _mockJwtService.GenerateRefreshToken(command.IpAddress)
                .Returns(refreshToken);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var updatedUser = await UsersUnitOfWork.Users.GetByUsernameAsync("testuser", GetCancellationToken());
            updatedUser.RefreshTokens.Should().NotBeNull();
            updatedUser.RefreshTokens.Should().ContainSingle();
        }

        [Test]
        public async Task Handle_WithCancellationToken_UsesTokenCorrectly()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new AuthenticateCommand("testuser", "password123", "127.0.0.1");
            var cancellationToken = GetCancellationToken();

            _mockPasswordService.VerifyPasswordHash(command.Password, user.PasswordHash, user.PasswordSalt)
                .Returns(true);
            _mockJwtService.GenerateJwtTokenAsync(user, Arg.Any<bool>(), cancellationToken)
                .Returns("jwt-token");
            _mockJwtService.GenerateRefreshToken(command.IpAddress)
                .Returns(new RefreshToken { Token = "refresh-token", Expires = DateTime.UtcNow.AddDays(7), Created = DateTime.UtcNow, CreatedByIp = command.IpAddress });

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            await _mockJwtService.Received(1).GenerateJwtTokenAsync(user, Arg.Any<bool>(), cancellationToken);
        }

        [Test]
        public async Task Handle_CaseSensitiveUsername_FindsCorrectUser()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("TestUser", "Test", "User");
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new AuthenticateCommand("TestUser", "password123", "127.0.0.1");

            _mockPasswordService.VerifyPasswordHash(command.Password, user.PasswordHash, user.PasswordSalt)
                .Returns(true);
            _mockJwtService.GenerateJwtTokenAsync(user, Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns("jwt-token");
            _mockJwtService.GenerateRefreshToken(command.IpAddress)
                .Returns(new RefreshToken { Token = "refresh-token", Expires = DateTime.UtcNow.AddDays(7), Created = DateTime.UtcNow, CreatedByIp = command.IpAddress });

            // Act
            var result = await _handler.Handle(command, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Username.Should().Be("TestUser");
        }
    }
} 