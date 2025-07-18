using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Commands.Authenticate;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Users.Services;
using Tests.TestHelpers;

namespace Tests.Features.Users.Commands
{
    [TestFixture]
    public class AuthenticateCommandHandlerTests : TestBase
    {
        private AuthenticateCommandHandler _handler;
        private IUsersUnitOfWork _mockUsersUnitOfWork;
        private IUserRepository _mockUserRepository;
        private IJwtService _mockJwtService;
        private IPasswordService _mockPasswordService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _mockUsersUnitOfWork = Substitute.For<IUsersUnitOfWork>();
            _mockUserRepository = Substitute.For<IUserRepository>();
            _mockJwtService = Substitute.For<IJwtService>();
            _mockPasswordService = Substitute.For<IPasswordService>();
            
            _mockUsersUnitOfWork.Users.Returns(_mockUserRepository);
            
            _handler = new AuthenticateCommandHandler(
                _mockUsersUnitOfWork,
                _mockJwtService,
                _mockPasswordService);
        }

        [TearDown]
        public override void TearDown()
        {
            _mockUsersUnitOfWork.Dispose();
        }

        [Test]
        public async Task Handle_ValidCredentials_ReturnsAuthenticateResponse()
        {
            // Arrange
            var command = new AuthenticateCommand("testuser", "password123", "127.0.0.1");
            var user = TestDataFactory.CreateTestUser();
            var expectedToken = "jwt-token";
            var expectedRefreshToken = new OpenAlprWebhookProcessor.Features.Users.RefreshToken
            {
                Token = "refresh-token",
                Expires = System.DateTime.UtcNow.AddDays(7),
                Created = System.DateTime.UtcNow,
                CreatedByIp = "127.0.0.1"
            };
            
            _mockUserRepository.GetByUsernameAsync(command.Username, Arg.Any<CancellationToken>())
                .Returns(user);
            _mockPasswordService.VerifyPasswordHash(command.Password, user.PasswordHash, user.PasswordSalt)
                .Returns(true);
            _mockJwtService.GenerateJwtTokenAsync(user, Arg.Any<CancellationToken>())
                .Returns(expectedToken);
            _mockJwtService.GenerateRefreshToken(command.IpAddress)
                .Returns(expectedRefreshToken);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
            result.Username.Should().Be(user.Username);
            result.FirstName.Should().Be(user.FirstName);
            result.LastName.Should().Be(user.LastName);
            result.JwtToken.Should().Be(expectedToken);
            result.RefreshToken.Should().Be(expectedRefreshToken.Token);
            
            _mockUserRepository.Received(1).Update(user);
            await _mockUsersUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_UserNotFound_ReturnsNull()
        {
            // Arrange
            var command = new AuthenticateCommand("nonexistent", "password123", "127.0.0.1");
            
            _mockUserRepository.GetByUsernameAsync(command.Username, Arg.Any<CancellationToken>())
                .Returns((User)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeNull();
            _mockPasswordService.DidNotReceive().VerifyPasswordHash(Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<byte[]>());
            await _mockJwtService.DidNotReceive().GenerateJwtTokenAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_InvalidPassword_ReturnsNull()
        {
            // Arrange
            var command = new AuthenticateCommand("testuser", "wrongpassword", "127.0.0.1");
            var user = TestDataFactory.CreateTestUser();
            
            _mockUserRepository.GetByUsernameAsync(command.Username, Arg.Any<CancellationToken>())
                .Returns(user);
            _mockPasswordService.VerifyPasswordHash(command.Password, user.PasswordHash, user.PasswordSalt)
                .Returns(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeNull();
            await _mockJwtService.DidNotReceive().GenerateJwtTokenAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_ValidCredentials_AddsRefreshTokenToUser()
        {
            // Arrange
            var command = new AuthenticateCommand("testuser", "password123", "127.0.0.1");
            var user = TestDataFactory.CreateTestUser();
            var expectedRefreshToken = new OpenAlprWebhookProcessor.Features.Users.RefreshToken
            {
                Token = "refresh-token",
                Expires = System.DateTime.UtcNow.AddDays(7),
                Created = System.DateTime.UtcNow,
                CreatedByIp = "127.0.0.1"
            };
            
            _mockUserRepository.GetByUsernameAsync(command.Username, Arg.Any<CancellationToken>())
                .Returns(user);
            _mockPasswordService.VerifyPasswordHash(command.Password, user.PasswordHash, user.PasswordSalt)
                .Returns(true);
            _mockJwtService.GenerateRefreshToken(command.IpAddress)
                .Returns(expectedRefreshToken);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            user.RefreshTokens.Should().Contain(expectedRefreshToken);
        }

        [Test]
        public async Task Handle_UserWithNullRefreshTokens_InitializesRefreshTokensList()
        {
            // Arrange
            var command = new AuthenticateCommand("testuser", "password123", "127.0.0.1");
            var user = TestDataFactory.CreateTestUser();
            user.RefreshTokens = null;
            
            var expectedRefreshToken = new OpenAlprWebhookProcessor.Features.Users.RefreshToken
            {
                Token = "refresh-token",
                Expires = System.DateTime.UtcNow.AddDays(7),
                Created = System.DateTime.UtcNow,
                CreatedByIp = "127.0.0.1"
            };
            
            _mockUserRepository.GetByUsernameAsync(command.Username, Arg.Any<CancellationToken>())
                .Returns(user);
            _mockPasswordService.VerifyPasswordHash(command.Password, user.PasswordHash, user.PasswordSalt)
                .Returns(true);
            _mockJwtService.GenerateRefreshToken(command.IpAddress)
                .Returns(expectedRefreshToken);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            user.RefreshTokens.Should().NotBeNull();
            user.RefreshTokens.Should().Contain(expectedRefreshToken);
        }
    }
} 