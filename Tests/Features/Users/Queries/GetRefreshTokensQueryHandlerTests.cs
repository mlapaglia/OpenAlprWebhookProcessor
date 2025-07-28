using AwesomeAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetRefreshTokens;
using Tests.TestHelpers;

namespace Tests.Features.Users.Queries
{
    [TestFixture]
    public class GetRefreshTokensQueryHandlerTests : TestBase
    {
        private GetRefreshTokensQueryHandler _handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _handler = new GetRefreshTokensQueryHandler(UsersUnitOfWork);
        }

        [Test]
        public async Task Handle_ExistingUserWithTokens_ReturnsRefreshTokens()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            var refreshTokens = new List<RefreshToken>
            {
                new RefreshToken
                {
                    Token = "token1",
                    Expires = DateTime.UtcNow.AddDays(7),
                    Created = DateTime.UtcNow,
                    CreatedByIp = "127.0.0.1"
                },
                new RefreshToken
                {
                    Token = "token2",
                    Expires = DateTime.UtcNow.AddDays(3),
                    Created = DateTime.UtcNow.AddDays(-1),
                    CreatedByIp = "192.168.1.1"
                }
            };
            user.RefreshTokens = refreshTokens;
            
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var query = new GetRefreshTokensQuery(user.Id);

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(rt => rt.Token == "token1");
            result.Should().Contain(rt => rt.Token == "token2");
        }

        [Test]
        public async Task Handle_ExistingUserWithNoTokens_ReturnsEmptyList()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            user.RefreshTokens = new List<RefreshToken>(); // No tokens
            
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var query = new GetRefreshTokensQuery(user.Id);

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task Handle_NonExistentUser_ReturnsNull()
        {
            // Arrange
            var query = new GetRefreshTokensQuery(999);

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task Handle_UserWithMixedTokenStates_ReturnsAllTokens()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            var refreshTokens = new List<RefreshToken>
            {
                new RefreshToken // Active token
                {
                    Token = "active-token",
                    Expires = DateTime.UtcNow.AddDays(7),
                    Created = DateTime.UtcNow,
                    CreatedByIp = "127.0.0.1"
                },
                new RefreshToken // Expired token
                {
                    Token = "expired-token",
                    Expires = DateTime.UtcNow.AddDays(-1),
                    Created = DateTime.UtcNow.AddDays(-8),
                    CreatedByIp = "127.0.0.1"
                },
                new RefreshToken // Revoked token
                {
                    Token = "revoked-token",
                    Expires = DateTime.UtcNow.AddDays(7),
                    Created = DateTime.UtcNow,
                    CreatedByIp = "127.0.0.1",
                    Revoked = DateTime.UtcNow.AddHours(-1),
                    RevokedByIp = "192.168.1.1"
                }
            };
            user.RefreshTokens = refreshTokens;
            
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var query = new GetRefreshTokensQuery(user.Id);

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().Contain(rt => rt.Token == "active-token");
            result.Should().Contain(rt => rt.Token == "expired-token");
            result.Should().Contain(rt => rt.Token == "revoked-token");
        }

        [Test]
        public async Task Handle_ValidUserId_ReturnsCorrectTokenData()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            var refreshToken = new RefreshToken
            {
                Token = "detailed-token",
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = "127.0.0.1"
            };
            user.RefreshTokens = new List<RefreshToken> { refreshToken };
            
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var query = new GetRefreshTokensQuery(user.Id);

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Should().ContainSingle();
            
            var returnedToken = result.Single();
            returnedToken.Token.Should().Be("detailed-token");
            returnedToken.CreatedByIp.Should().Be("127.0.0.1");
            returnedToken.Expires.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromMinutes(1));
            returnedToken.Created.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        }

        [Test]
        public async Task Handle_WithCancellationToken_UsesTokenCorrectly()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            user.RefreshTokens = TestDataFactory.CreateTestRefreshTokens();
            
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var query = new GetRefreshTokensQuery(user.Id);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2); // TestDataFactory creates 2 tokens
        }

        [Test]
        public async Task Handle_ZeroUserId_ReturnsNull()
        {
            // Arrange
            var query = new GetRefreshTokensQuery(0);

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task Handle_NegativeUserId_ReturnsNull()
        {
            // Arrange
            var query = new GetRefreshTokensQuery(-1);

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task Handle_MultipleUsers_ReturnsOnlySpecifiedUserTokens()
        {
            // Arrange
            var user1 = TestDataFactory.CreateTestUser("user1", "First", "User");
            var user2 = TestDataFactory.CreateTestUser("user2", "Second", "User");
            
            user1.RefreshTokens = new List<RefreshToken>
            {
                new RefreshToken { Token = "user1-token1", Expires = DateTime.UtcNow.AddDays(7), Created = DateTime.UtcNow, CreatedByIp = "127.0.0.1" },
                new RefreshToken { Token = "user1-token2", Expires = DateTime.UtcNow.AddDays(7), Created = DateTime.UtcNow, CreatedByIp = "127.0.0.1" }
            };
            
            user2.RefreshTokens = new List<RefreshToken>
            {
                new RefreshToken { Token = "user2-token1", Expires = DateTime.UtcNow.AddDays(7), Created = DateTime.UtcNow, CreatedByIp = "192.168.1.1" }
            };
            
            await UsersUnitOfWork.Users.AddAsync(user1, GetCancellationToken());
            await UsersUnitOfWork.Users.AddAsync(user2, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var query = new GetRefreshTokensQuery(user1.Id);

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(rt => rt.Token == "user1-token1");
            result.Should().Contain(rt => rt.Token == "user1-token2");
            result.Should().NotContain(rt => rt.Token == "user2-token1");
        }

        [Test]
        public async Task Handle_LargeUserId_ReturnsNull()
        {
            // Arrange
            var query = new GetRefreshTokensQuery(int.MaxValue);

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task Handle_UserWithSingleToken_ReturnsSingleToken()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            var refreshToken = new RefreshToken
            {
                Token = "single-token",
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = "127.0.0.1"
            };
            user.RefreshTokens = new List<RefreshToken> { refreshToken };
            
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var query = new GetRefreshTokensQuery(user.Id);

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Should().ContainSingle();
            result.Single().Token.Should().Be("single-token");
        }

        [Test]
        public async Task Handle_UserDeletedAfterCreation_ReturnsNull()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            user.RefreshTokens = TestDataFactory.CreateTestRefreshTokens();
            
            await UsersUnitOfWork.Users.AddAsync(user, GetCancellationToken());
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var userId = user.Id;

            // Delete the user
            UsersUnitOfWork.Users.Delete(user);
            await UsersUnitOfWork.SaveChangesAsync(GetCancellationToken());

            var query = new GetRefreshTokensQuery(userId);

            // Act
            var result = await _handler.Handle(query, GetCancellationToken());

            // Assert
            result.Should().BeNull();
        }
    }
} 