using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users;
using OpenAlprWebhookProcessor.Features.Users.Commands.RevokeToken;
using Tests.TestHelpers;

namespace Tests.Features.Users.Commands
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class RevokeTokenCommandHandlerTests : TestBase
    {
        private RevokeTokenCommandHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new RevokeTokenCommandHandler(UsersUnitOfWork);
        }

        [Test]
        public async Task Handle_ValidActiveToken_RevokesTokenSuccessfully()
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

            var command = new RevokeTokenCommand("valid-refresh-token", "192.168.1.1");

            // Act
            var result = await _handler.Handle(command, GetCancellationToken());

            // Assert
            result.Should().BeTrue();

            // Verify the token was revoked
            var updatedUser = await UsersUnitOfWork.Users.GetByRefreshTokenAsync("valid-refresh-token", GetCancellationToken());
            updatedUser.Should().NotBeNull();
            
            var revokedToken = updatedUser.RefreshTokens.Single(x => x.Token == "valid-refresh-token");
            revokedToken.Revoked.Should().NotBeNull();
            revokedToken.RevokedByIp.Should().Be("192.168.1.1");
        }

        [Test]
        public async Task Handle_InvalidToken_ReturnsFalse()
        {
            // Arrange
            var command = new RevokeTokenCommand("invalid-token", "192.168.1.1");

            // Act
            var result = await _handler.Handle(command, GetCancellationToken());

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task Handle_AlreadyRevokedToken_ReturnsFalse()
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
                RevokedByIp = "192.168.1.100"
            };
            user.RefreshTokens = new List<RefreshToken> { revokedRefreshToken };
            
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new RevokeTokenCommand("revoked-refresh-token", "192.168.1.1");

            // Act
            var result = await _handler.Handle(command, GetCancellationToken());

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task Handle_ExpiredToken_ReturnsFalse()
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

            var command = new RevokeTokenCommand("expired-refresh-token", "192.168.1.1");

            // Act
            var result = await _handler.Handle(command, GetCancellationToken());

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task Handle_EmptyToken_ReturnsFalse()
        {
            // Arrange
            var command = new RevokeTokenCommand("", "192.168.1.1");

            // Act
            var result = await _handler.Handle(command, GetCancellationToken());

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task Handle_NullToken_ReturnsFalse()
        {
            // Arrange
            var command = new RevokeTokenCommand(null, "192.168.1.1");

            // Act
            var result = await _handler.Handle(command, GetCancellationToken());

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task Handle_ValidToken_SavesChangesToDatabase()
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

            var command = new RevokeTokenCommand("valid-refresh-token", "192.168.1.1");

            // Act
            await _handler.Handle(command, GetCancellationToken());

            // Assert - verify changes were persisted by checking with a new context
            using var freshContext = ContextCreator.CreateUsersContext();
            var updatedUser = await freshContext.Users.Include(u => u.RefreshTokens)
                .SingleOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == "valid-refresh-token"));
            
            updatedUser.Should().NotBeNull();
            var revokedToken = updatedUser.RefreshTokens.Single(x => x.Token == "valid-refresh-token");
            revokedToken.Revoked.Should().NotBeNull();
        }

        [Test]
        public async Task Handle_MultipleTokens_RevokesOnlySpecifiedToken()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            var token1 = new RefreshToken { Token = "token1", Expires = DateTime.UtcNow.AddDays(7), Created = DateTime.UtcNow, CreatedByIp = "127.0.0.1" };
            var token2 = new RefreshToken { Token = "token2", Expires = DateTime.UtcNow.AddDays(7), Created = DateTime.UtcNow, CreatedByIp = "127.0.0.1" };
            var token3 = new RefreshToken { Token = "token3", Expires = DateTime.UtcNow.AddDays(7), Created = DateTime.UtcNow, CreatedByIp = "127.0.0.1" };
            
            user.RefreshTokens = new List<RefreshToken> { token1, token2, token3 };
            
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new RevokeTokenCommand("token2", "192.168.1.1");

            // Act
            var result = await _handler.Handle(command, GetCancellationToken());

            // Assert
            result.Should().BeTrue();
            
            var updatedUser = await UsersUnitOfWork.Users.GetByRefreshTokenAsync("token2", GetCancellationToken());
            var revokedToken = updatedUser.RefreshTokens.Single(x => x.Token == "token2");
            revokedToken.Revoked.Should().NotBeNull();
            revokedToken.RevokedByIp.Should().Be("192.168.1.1");
            
            // Other tokens should remain unchanged
            var unchangedToken1 = updatedUser.RefreshTokens.Single(x => x.Token == "token1");
            unchangedToken1.Revoked.Should().BeNull();
            
            var unchangedToken3 = updatedUser.RefreshTokens.Single(x => x.Token == "token3");
            unchangedToken3.Revoked.Should().BeNull();
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

            var command = new RevokeTokenCommand("valid-refresh-token", "192.168.1.1");
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().BeTrue();
            
            var updatedUser = await UsersUnitOfWork.Users.GetByRefreshTokenAsync("valid-refresh-token", cancellationToken);
            var revokedToken = updatedUser.RefreshTokens.Single(x => x.Token == "valid-refresh-token");
            revokedToken.Revoked.Should().NotBeNull();
        }

        [Test]
        public async Task Handle_UserWithNoTokens_ReturnsFalse()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            user.RefreshTokens = new List<RefreshToken>(); // No tokens
            
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new RevokeTokenCommand("non-existent-token", "192.168.1.1");

            // Act
            var result = await _handler.Handle(command, GetCancellationToken());

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task Handle_ValidTokenWithDifferentIpAddress_RevokesWithNewIp()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            var activeRefreshToken = new RefreshToken
            {
                Token = "valid-refresh-token",
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = "127.0.0.1" // Original IP
            };
            user.RefreshTokens = new List<RefreshToken> { activeRefreshToken };
            
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var command = new RevokeTokenCommand("valid-refresh-token", "10.0.0.1"); // Different IP

            // Act
            var result = await _handler.Handle(command, GetCancellationToken());

            // Assert
            result.Should().BeTrue();
            
            var updatedUser = await UsersUnitOfWork.Users.GetByRefreshTokenAsync("valid-refresh-token", GetCancellationToken());
            var revokedToken = updatedUser.RefreshTokens.Single(x => x.Token == "valid-refresh-token");
            revokedToken.Revoked.Should().NotBeNull();
            revokedToken.RevokedByIp.Should().Be("10.0.0.1"); // Should use new IP for revocation
            revokedToken.CreatedByIp.Should().Be("127.0.0.1"); // Original IP should remain unchanged
        }
    }
} 