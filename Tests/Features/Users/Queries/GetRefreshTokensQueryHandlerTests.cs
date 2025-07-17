using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetRefreshTokens;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tests.TestHelpers;

namespace Tests.Features.Users.Queries
{
    [TestFixture]
    public class GetRefreshTokensQueryHandlerTests : TestBase
    {
        private GetRefreshTokensQueryHandler _handler;
        private IUsersUnitOfWork _mockUsersUnitOfWork;
        private IUserRepository _mockUserRepository;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _mockUsersUnitOfWork = Substitute.For<IUsersUnitOfWork>();
            _mockUserRepository = Substitute.For<IUserRepository>();
            
            _mockUsersUnitOfWork.Users.Returns(_mockUserRepository);
            
            _handler = new GetRefreshTokensQueryHandler(_mockUsersUnitOfWork);
        }

        [TearDown]
        public override void TearDown()
        {
            _mockUsersUnitOfWork.Dispose();
        }

        [Test]
        public async Task Handle_ExistingUserWithTokens_ReturnsRefreshTokens()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            var refreshTokens = new List<OpenAlprWebhookProcessor.Features.Users.RefreshToken>
            {
                new OpenAlprWebhookProcessor.Features.Users.RefreshToken
                {
                    Token = "token1",
                    Expires = DateTime.UtcNow.AddDays(7),
                    Created = DateTime.UtcNow,
                    CreatedByIp = "127.0.0.1"
                },
                new OpenAlprWebhookProcessor.Features.Users.RefreshToken
                {
                    Token = "token2",
                    Expires = DateTime.UtcNow.AddDays(7),
                    Created = DateTime.UtcNow,
                    CreatedByIp = "127.0.0.1"
                }
            };
            user.RefreshTokens = refreshTokens;
            
            var query = new GetRefreshTokensQuery(user.Id);
            
            _mockUserRepository.GetByIdWithRefreshTokensAsync(query.UserId, Arg.Any<CancellationToken>())
                .Returns(user);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(refreshTokens);
        }

        [Test]
        public async Task Handle_ExistingUserWithNoTokens_ReturnsEmptyList()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            user.RefreshTokens = new List<OpenAlprWebhookProcessor.Features.Users.RefreshToken>();
            
            var query = new GetRefreshTokensQuery(user.Id);
            
            _mockUserRepository.GetByIdWithRefreshTokensAsync(query.UserId, Arg.Any<CancellationToken>())
                .Returns(user);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task Handle_NonExistentUser_ReturnsNull()
        {
            // Arrange
            var query = new GetRefreshTokensQuery(999);
            
            _mockUserRepository.GetByIdWithRefreshTokensAsync(query.UserId, Arg.Any<CancellationToken>())
                .Returns((User)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task Handle_UserWithNullRefreshTokens_ReturnsNull()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            user.RefreshTokens = null;
            
            var query = new GetRefreshTokensQuery(user.Id);
            
            _mockUserRepository.GetByIdWithRefreshTokensAsync(query.UserId, Arg.Any<CancellationToken>())
                .Returns(user);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task Handle_ValidQuery_CallsCorrectRepositoryMethod()
        {
            // Arrange
            var userId = 123;
            var query = new GetRefreshTokensQuery(userId);
            
            _mockUserRepository.GetByIdWithRefreshTokensAsync(query.UserId, Arg.Any<CancellationToken>())
                .Returns(TestDataFactory.CreateTestUser());

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            await _mockUserRepository.Received(1).GetByIdWithRefreshTokensAsync(userId, Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_ValidQuery_PassesCancellationToken()
        {
            // Arrange
            var userId = 123;
            var query = new GetRefreshTokensQuery(userId);
            var cancellationToken = new CancellationToken();
            
            _mockUserRepository.GetByIdWithRefreshTokensAsync(query.UserId, cancellationToken)
                .Returns(TestDataFactory.CreateTestUser());

            // Act
            await _handler.Handle(query, cancellationToken);

            // Assert
            await _mockUserRepository.Received(1).GetByIdWithRefreshTokensAsync(userId, cancellationToken);
        }

        [Test]
        public async Task Handle_ZeroUserId_AttemptsToFindUser()
        {
            // Arrange
            var query = new GetRefreshTokensQuery(0);
            
            _mockUserRepository.GetByIdWithRefreshTokensAsync(query.UserId, Arg.Any<CancellationToken>())
                .Returns((User)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();
            await _mockUserRepository.Received(1).GetByIdWithRefreshTokensAsync(0, Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_NegativeUserId_AttemptsToFindUser()
        {
            // Arrange
            var query = new GetRefreshTokensQuery(-1);
            
            _mockUserRepository.GetByIdWithRefreshTokensAsync(query.UserId, Arg.Any<CancellationToken>())
                .Returns((User)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();
            await _mockUserRepository.Received(1).GetByIdWithRefreshTokensAsync(-1, Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_ExistingUserWithMixedTokens_ReturnsAllTokens()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            var refreshTokens = new List<OpenAlprWebhookProcessor.Features.Users.RefreshToken>
            {
                new OpenAlprWebhookProcessor.Features.Users.RefreshToken
                {
                    Token = "active-token",
                    Expires = DateTime.UtcNow.AddDays(7),
                    Created = DateTime.UtcNow,
                    CreatedByIp = "127.0.0.1"
                },
                new OpenAlprWebhookProcessor.Features.Users.RefreshToken
                {
                    Token = "expired-token",
                    Expires = DateTime.UtcNow.AddDays(-1),
                    Created = DateTime.UtcNow.AddDays(-8),
                    CreatedByIp = "127.0.0.1"
                },
                new OpenAlprWebhookProcessor.Features.Users.RefreshToken
                {
                    Token = "revoked-token",
                    Expires = DateTime.UtcNow.AddDays(7),
                    Created = DateTime.UtcNow,
                    CreatedByIp = "127.0.0.1",
                    Revoked = DateTime.UtcNow.AddHours(-1),
                    RevokedByIp = "127.0.0.1"
                }
            };
            user.RefreshTokens = refreshTokens;
            
            var query = new GetRefreshTokensQuery(user.Id);
            
            _mockUserRepository.GetByIdWithRefreshTokensAsync(query.UserId, Arg.Any<CancellationToken>())
                .Returns(user);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(refreshTokens);
        }
    }
} 