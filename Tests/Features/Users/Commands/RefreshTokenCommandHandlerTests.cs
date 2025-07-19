using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users;
using OpenAlprWebhookProcessor.Features.Users.Commands.RefreshToken;
using OpenAlprWebhookProcessor.Features.Users.Services;
using Tests.TestHelpers;

namespace Tests.Features.Users.Commands
{
    [TestFixture]
    public class RefreshTokenCommandHandlerTests : TestBase
    {
        private RefreshTokenCommandHandler _handler;
        private IJwtService _mockJwtService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _mockJwtService = Substitute.For<IJwtService>();
            
            _handler = new RefreshTokenCommandHandler(
                UsersUnitOfWork,
                _mockJwtService);
        }

        [Test]
        public async Task Handle_ValidRefreshToken_ReturnsNewTokens()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            var activeRefreshToken = new RefreshToken
            {
                Token = "valid-refresh-token",
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = "127.0.0.1"
            };
            user.RefreshTokens = new List<RefreshToken> { activeRefreshToken };
            
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new RefreshTokenCommand("valid-refresh-token", "192.168.1.1");
            var newJwtToken = "new-jwt-token";
            var newRefreshToken = new RefreshToken
            {
                Token = "new-refresh-token",
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = "192.168.1.1"
            };

            _mockJwtService.GenerateRefreshToken("192.168.1.1")
                .Returns(newRefreshToken);
            _mockJwtService.GenerateJwtTokenAsync(user, Arg.Any<CancellationToken>())
                .Returns(newJwtToken);

            // Act
            var result = await _handler.Handle(command, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
            result.Username.Should().Be(user.Username);
            result.FirstName.Should().Be(user.FirstName);
            result.LastName.Should().Be(user.LastName);
            result.JwtToken.Should().Be(newJwtToken);
            result.RefreshToken.Should().Be(newRefreshToken.Token);

            // Verify the old token was revoked and new token was added
            var updatedUser = await UsersUnitOfWork.Users.GetByRefreshTokenAsync("new-refresh-token", GetCancellationToken());
            updatedUser.Should().NotBeNull();
            
            var oldToken = updatedUser.RefreshTokens.Single(x => x.Token == "valid-refresh-token");
            oldToken.Revoked.Should().NotBeNull();
            oldToken.RevokedByIp.Should().Be("192.168.1.1");
            oldToken.ReplacedByToken.Should().Be("new-refresh-token");
        }

        [Test]
        public async Task Handle_InvalidRefreshToken_ReturnsNull()
        {
            // Arrange
            var command = new RefreshTokenCommand("invalid-token", "192.168.1.1");

            // Act
            var result = await _handler.Handle(command, GetCancellationToken());

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task Handle_ExpiredRefreshToken_ReturnsNull()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            var expiredRefreshToken = new RefreshToken
            {
                Token = "expired-refresh-token",
                Expires = DateTime.UtcNow.AddDays(-1), // Expired
                Created = DateTime.UtcNow.AddDays(-8),
                CreatedByIp = "127.0.0.1"
            };
            user.RefreshTokens = new List<RefreshToken> { expiredRefreshToken };
            
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new RefreshTokenCommand("expired-refresh-token", "192.168.1.1");

            // Act
            var result = await _handler.Handle(command, GetCancellationToken());

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task Handle_RevokedRefreshToken_ReturnsNull()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            var revokedRefreshToken = new RefreshToken
            {
                Token = "revoked-refresh-token",
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = "127.0.0.1",
                Revoked = DateTime.UtcNow.AddMinutes(-30), // Already revoked
                RevokedByIp = "127.0.0.1"
            };
            user.RefreshTokens = new List<RefreshToken> { revokedRefreshToken };
            
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new RefreshTokenCommand("revoked-refresh-token", "192.168.1.1");

            // Act
            var result = await _handler.Handle(command, GetCancellationToken());

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task Handle_ValidToken_GeneratesNewJwtToken()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            var activeRefreshToken = new RefreshToken
            {
                Token = "valid-refresh-token",
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = "127.0.0.1"
            };
            user.RefreshTokens = new List<RefreshToken> { activeRefreshToken };
            
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new RefreshTokenCommand("valid-refresh-token", "192.168.1.1");
            var newJwtToken = "new-jwt-token";
            var newRefreshToken = new RefreshToken { Token = "new-refresh-token", Expires = DateTime.UtcNow.AddDays(7), Created = DateTime.UtcNow, CreatedByIp = "192.168.1.1" };

            _mockJwtService.GenerateRefreshToken("192.168.1.1")
                .Returns(newRefreshToken);
            _mockJwtService.GenerateJwtTokenAsync(user, Arg.Any<CancellationToken>())
                .Returns(newJwtToken);

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            await _mockJwtService.Received(1).GenerateJwtTokenAsync(user, Arg.Any<CancellationToken>());
            _mockJwtService.Received(1).GenerateRefreshToken("192.168.1.1");
        }

        [Test]
        public async Task Handle_ValidToken_RevokesOldToken()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            var activeRefreshToken = new RefreshToken
            {
                Token = "valid-refresh-token",
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = "127.0.0.1"
            };
            user.RefreshTokens = new List<RefreshToken> { activeRefreshToken };
            
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new RefreshTokenCommand("valid-refresh-token", "192.168.1.1");
            var newRefreshToken = new RefreshToken { Token = "new-refresh-token", Expires = DateTime.UtcNow.AddDays(7), Created = DateTime.UtcNow, CreatedByIp = "192.168.1.1" };

            _mockJwtService.GenerateRefreshToken("192.168.1.1")
                .Returns(newRefreshToken);
            _mockJwtService.GenerateJwtTokenAsync(user, Arg.Any<CancellationToken>())
                .Returns("jwt-token");

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var updatedUser = await UsersUnitOfWork.Users.GetByRefreshTokenAsync("new-refresh-token", GetCancellationToken());
            var oldToken = updatedUser.RefreshTokens.Single(x => x.Token == "valid-refresh-token");
            oldToken.Revoked.Should().NotBeNull();
            oldToken.RevokedByIp.Should().Be("192.168.1.1");
            oldToken.ReplacedByToken.Should().Be("new-refresh-token");
        }

        [Test]
        public async Task Handle_ValidToken_AddsNewRefreshToken()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            var activeRefreshToken = new RefreshToken
            {
                Token = "valid-refresh-token",
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = "127.0.0.1"
            };
            user.RefreshTokens = new List<RefreshToken> { activeRefreshToken };
            
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new RefreshTokenCommand("valid-refresh-token", "192.168.1.1");
            var newRefreshToken = new RefreshToken 
            { 
                Token = "new-refresh-token", 
                Expires = DateTime.UtcNow.AddDays(7), 
                Created = DateTime.UtcNow, 
                CreatedByIp = "192.168.1.1" 
            };

            _mockJwtService.GenerateRefreshToken("192.168.1.1")
                .Returns(newRefreshToken);
            _mockJwtService.GenerateJwtTokenAsync(user, Arg.Any<CancellationToken>())
                .Returns("jwt-token");

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert
            var updatedUser = await UsersUnitOfWork.Users.GetByRefreshTokenAsync("new-refresh-token", GetCancellationToken());
            updatedUser.RefreshTokens.Should().HaveCount(2);
            updatedUser.RefreshTokens.Should().Contain(x => x.Token == "new-refresh-token");
        }

        [Test]
        public async Task Handle_WithCancellationToken_UsesTokenCorrectly()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            var activeRefreshToken = new RefreshToken
            {
                Token = "valid-refresh-token",
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = "127.0.0.1"
            };
            user.RefreshTokens = new List<RefreshToken> { activeRefreshToken };
            
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new RefreshTokenCommand("valid-refresh-token", "192.168.1.1");
            var cancellationToken = GetCancellationToken();
            var newRefreshToken = new RefreshToken { Token = "new-refresh-token", Expires = DateTime.UtcNow.AddDays(7), Created = DateTime.UtcNow, CreatedByIp = "192.168.1.1" };

            _mockJwtService.GenerateRefreshToken("192.168.1.1")
                .Returns(newRefreshToken);
            _mockJwtService.GenerateJwtTokenAsync(user, cancellationToken)
                .Returns("jwt-token");

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            await _mockJwtService.Received(1).GenerateJwtTokenAsync(user, cancellationToken);
        }

        [Test]
        public async Task Handle_MultipleRefreshTokens_FindsCorrectToken()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            var token1 = new RefreshToken { Token = "token1", Expires = DateTime.UtcNow.AddDays(7), Created = DateTime.UtcNow, CreatedByIp = "127.0.0.1" };
            var token2 = new RefreshToken { Token = "token2", Expires = DateTime.UtcNow.AddDays(7), Created = DateTime.UtcNow, CreatedByIp = "127.0.0.1" };
            var token3 = new RefreshToken { Token = "token3", Expires = DateTime.UtcNow.AddDays(7), Created = DateTime.UtcNow, CreatedByIp = "127.0.0.1" };
            
            user.RefreshTokens = new List<RefreshToken> { token1, token2, token3 };
            
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new RefreshTokenCommand("token2", "192.168.1.1");
            var newRefreshToken = new RefreshToken { Token = "new-token", Expires = DateTime.UtcNow.AddDays(7), Created = DateTime.UtcNow, CreatedByIp = "192.168.1.1" };

            _mockJwtService.GenerateRefreshToken("192.168.1.1")
                .Returns(newRefreshToken);
            _mockJwtService.GenerateJwtTokenAsync(user, Arg.Any<CancellationToken>())
                .Returns("jwt-token");

            // Act
            var result = await _handler.Handle(command, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            
            var updatedUser = await UsersUnitOfWork.Users.GetByRefreshTokenAsync("new-token", GetCancellationToken());
            var revokedToken = updatedUser.RefreshTokens.Single(x => x.Token == "token2");
            revokedToken.Revoked.Should().NotBeNull();
            
            // Other tokens should remain unchanged
            var unchangedToken1 = updatedUser.RefreshTokens.Single(x => x.Token == "token1");
            unchangedToken1.Revoked.Should().BeNull();
            
            var unchangedToken3 = updatedUser.RefreshTokens.Single(x => x.Token == "token3");
            unchangedToken3.Revoked.Should().BeNull();
        }
    }
} 