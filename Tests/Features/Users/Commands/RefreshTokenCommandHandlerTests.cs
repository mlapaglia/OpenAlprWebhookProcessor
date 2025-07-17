using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users;
using OpenAlprWebhookProcessor.Features.Users.Commands.RefreshToken;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Users.Services;
using Tests.TestHelpers;

namespace Tests.Features.Users.Commands
{
    [TestFixture]
    public class RefreshTokenCommandHandlerTests : TestBase
    {
        private RefreshTokenCommandHandler _handler;
        private IUsersUnitOfWork _mockUsersUnitOfWork;
        private IUserRepository _mockUserRepository;
        private IJwtService _mockJwtService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _mockUsersUnitOfWork = Substitute.For<IUsersUnitOfWork>();
            _mockUserRepository = Substitute.For<IUserRepository>();
            _mockJwtService = Substitute.For<IJwtService>();
            
            _mockUsersUnitOfWork.Users.Returns(_mockUserRepository);
            
            _handler = new RefreshTokenCommandHandler(
                _mockUsersUnitOfWork,
                _mockJwtService);
        }

        [TearDown]
        public override void TearDown()
        {
            _mockUsersUnitOfWork.Dispose();
        }

        [Test]
        public async Task Handle_ValidRefreshToken_ReturnsNewTokens()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            user.RefreshTokens = new List<RefreshToken>(); // Initialize the collection
            var activeRefreshToken = new OpenAlprWebhookProcessor.Features.Users.RefreshToken
            {
                Token = "valid-refresh-token",
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = "127.0.0.1"
            };
            user.RefreshTokens.Add(activeRefreshToken);
            
            var command = new RefreshTokenCommand("valid-refresh-token", "127.0.0.1");
            var newJwtToken = "new-jwt-token";
            var newRefreshToken = new OpenAlprWebhookProcessor.Features.Users.RefreshToken
            {
                Token = "new-refresh-token",
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = "127.0.0.1"
            };
            
            _mockUserRepository.GetByRefreshTokenAsync(command.Token, Arg.Any<CancellationToken>())
                .Returns(user);
            _mockJwtService.GenerateJwtTokenAsync(user, Arg.Any<CancellationToken>())
                .Returns(newJwtToken);
            _mockJwtService.GenerateRefreshToken(command.IpAddress)
                .Returns(newRefreshToken);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
            result.Username.Should().Be(user.Username);
            result.FirstName.Should().Be(user.FirstName);
            result.LastName.Should().Be(user.LastName);
            result.JwtToken.Should().Be(newJwtToken);
            result.RefreshToken.Should().Be(newRefreshToken.Token);
            
            activeRefreshToken.Revoked.Should().NotBeNull();
            activeRefreshToken.RevokedByIp.Should().Be(command.IpAddress);
            activeRefreshToken.ReplacedByToken.Should().Be(newRefreshToken.Token);
            
            user.RefreshTokens.Should().Contain(newRefreshToken);
            _mockUserRepository.Received(1).Update(user);
            await _mockUsersUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_InvalidRefreshToken_ReturnsNull()
        {
            // Arrange
            var command = new RefreshTokenCommand("invalid-token", "127.0.0.1");
            
            _mockUserRepository.GetByRefreshTokenAsync(command.Token, Arg.Any<CancellationToken>())
                .Returns((User)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task Handle_ExpiredRefreshToken_ReturnsNull()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            user.RefreshTokens = new List<RefreshToken>(); // Initialize the collection
            var expiredRefreshToken = new OpenAlprWebhookProcessor.Features.Users.RefreshToken
            {
                Token = "expired-refresh-token",
                Expires = DateTime.UtcNow.AddDays(-1),
                Created = DateTime.UtcNow.AddDays(-8),
                CreatedByIp = "127.0.0.1"
            };
            user.RefreshTokens.Add(expiredRefreshToken);
            
            var command = new RefreshTokenCommand("expired-refresh-token", "127.0.0.1");
            
            _mockUserRepository.GetByRefreshTokenAsync(command.Token, Arg.Any<CancellationToken>())
                .Returns(user);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task Handle_RevokedRefreshToken_ReturnsNull()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            user.RefreshTokens = new List<RefreshToken>(); // Initialize the collection
            var revokedRefreshToken = new OpenAlprWebhookProcessor.Features.Users.RefreshToken
            {
                Token = "revoked-refresh-token",
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = "127.0.0.1",
                Revoked = DateTime.UtcNow.AddHours(-1),
                RevokedByIp = "127.0.0.1"
            };
            user.RefreshTokens.Add(revokedRefreshToken);
            
            var command = new RefreshTokenCommand("revoked-refresh-token", "127.0.0.1");
            
            _mockUserRepository.GetByRefreshTokenAsync(command.Token, Arg.Any<CancellationToken>())
                .Returns(user);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task Handle_EmptyToken_ReturnsNull()
        {
            // Arrange
            var command = new RefreshTokenCommand("", "127.0.0.1");
            
            _mockUserRepository.GetByRefreshTokenAsync(command.Token, Arg.Any<CancellationToken>())
                .Returns((User)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task Handle_NullToken_ReturnsNull()
        {
            // Arrange
            var command = new RefreshTokenCommand(null, "127.0.0.1");
            
            _mockUserRepository.GetByRefreshTokenAsync(command.Token, Arg.Any<CancellationToken>())
                .Returns((User)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task Handle_ValidToken_GeneratesNewTokensCorrectly()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            user.RefreshTokens = new List<RefreshToken>(); // Initialize the collection
            
            var activeRefreshToken = new RefreshToken
            {
                Token = "valid-refresh-token",
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = "127.0.0.1"
            };
            user.RefreshTokens.Add(activeRefreshToken);
            
            var newRefreshToken = new RefreshToken
            {
                Token = "new-refresh-token",
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = "127.0.0.1"
            };
            
            var command = new RefreshTokenCommand("valid-refresh-token", "127.0.0.1");
            
            _mockUserRepository.GetByRefreshTokenAsync(command.Token, Arg.Any<CancellationToken>())
                .Returns(user);
            
            _mockJwtService.GenerateRefreshToken(command.IpAddress)
                .Returns(newRefreshToken);
            
            _mockJwtService.GenerateJwtTokenAsync(user, Arg.Any<CancellationToken>())
                .Returns("new-jwt-token");

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.JwtToken.Should().Be("new-jwt-token");
            result.RefreshToken.Should().Be("new-refresh-token");
            
            // Verify the old token was revoked
            activeRefreshToken.Revoked.Should().NotBeNull();
            activeRefreshToken.RevokedByIp.Should().Be(command.IpAddress);
            activeRefreshToken.ReplacedByToken.Should().Be(newRefreshToken.Token);
            
            // Verify the new token was added
            user.RefreshTokens.Should().Contain(newRefreshToken);
            
            await _mockJwtService.Received(1).GenerateJwtTokenAsync(user, Arg.Any<CancellationToken>());
            _mockJwtService.Received(1).GenerateRefreshToken(command.IpAddress);
        }
    }
} 