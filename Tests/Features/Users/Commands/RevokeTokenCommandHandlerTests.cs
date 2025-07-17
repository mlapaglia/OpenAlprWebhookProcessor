using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users;
using OpenAlprWebhookProcessor.Features.Users.Commands.RevokeToken;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users.Data.Repositories;
using Tests.TestHelpers;

namespace Tests.Features.Users.Commands
{
    [TestFixture]
    public class RevokeTokenCommandHandlerTests : TestBase
    {
        private RevokeTokenCommandHandler _handler;
        private IUsersUnitOfWork _mockUsersUnitOfWork;
        private IUserRepository _mockUserRepository;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _mockUsersUnitOfWork = Substitute.For<IUsersUnitOfWork>();
            _mockUserRepository = Substitute.For<IUserRepository>();
            
            _mockUsersUnitOfWork.Users.Returns(_mockUserRepository);
            
            _handler = new RevokeTokenCommandHandler(_mockUsersUnitOfWork);
        }

        [TearDown]
        public override void TearDown()
        {
            _mockUsersUnitOfWork.Dispose();
        }

        [Test]
        public async Task Handle_ValidActiveToken_RevokesTokenSuccessfully()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            var activeRefreshToken = new OpenAlprWebhookProcessor.Features.Users.RefreshToken
            {
                Token = "valid-refresh-token",
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = "127.0.0.1"
            };
            user.RefreshTokens.Add(activeRefreshToken);
            
            var command = new RevokeTokenCommand("valid-refresh-token", "127.0.0.1");
            
            _mockUserRepository.GetByRefreshTokenAsync(command.Token, Arg.Any<CancellationToken>())
                .Returns(user);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            activeRefreshToken.Revoked.Should().NotBeNull();
            activeRefreshToken.RevokedByIp.Should().Be(command.IpAddress);
            
            _mockUserRepository.Received(1).Update(user);
            await _mockUsersUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_InvalidToken_ReturnsFalse()
        {
            // Arrange
            var command = new RevokeTokenCommand("invalid-token", "127.0.0.1");
            
            _mockUserRepository.GetByRefreshTokenAsync(command.Token, Arg.Any<CancellationToken>())
                .Returns((User)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            _mockUserRepository.DidNotReceive().Update(Arg.Any<User>());
            await _mockUsersUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_AlreadyRevokedToken_ReturnsFalse()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
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
            
            var command = new RevokeTokenCommand("revoked-refresh-token", "127.0.0.1");
            
            _mockUserRepository.GetByRefreshTokenAsync(command.Token, Arg.Any<CancellationToken>())
                .Returns(user);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            _mockUserRepository.DidNotReceive().Update(Arg.Any<User>());
            await _mockUsersUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_ExpiredToken_ReturnsFalse()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            var expiredRefreshToken = new RefreshToken
            {
                Token = "expired-refresh-token",
                Expires = DateTime.UtcNow.AddDays(-1),
                Created = DateTime.UtcNow.AddDays(-8),
                CreatedByIp = "127.0.0.1"
            };
            user.RefreshTokens.Add(expiredRefreshToken);
            
            var command = new RevokeTokenCommand("expired-refresh-token", "127.0.0.1");
            
            _mockUserRepository.GetByRefreshTokenAsync(command.Token, Arg.Any<CancellationToken>())
                .Returns(user);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            expiredRefreshToken.Revoked.Should().BeNull();
            expiredRefreshToken.RevokedByIp.Should().BeNull();
            
            _mockUserRepository.DidNotReceive().Update(user);
            await _mockUsersUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_NullToken_ReturnsFalse()
        {
            // Arrange
            var command = new RevokeTokenCommand(null, "127.0.0.1");
            
            _mockUserRepository.GetByRefreshTokenAsync(command.Token, Arg.Any<CancellationToken>())
                .Returns((User)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            _mockUserRepository.DidNotReceive().Update(Arg.Any<User>());
            await _mockUsersUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_EmptyToken_ReturnsFalse()
        {
            // Arrange
            var command = new RevokeTokenCommand("", "127.0.0.1");
            
            _mockUserRepository.GetByRefreshTokenAsync(command.Token, Arg.Any<CancellationToken>())
                .Returns((User)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            _mockUserRepository.DidNotReceive().Update(Arg.Any<User>());
            await _mockUsersUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_ValidToken_CallsCorrectRepositoryMethods()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            var activeRefreshToken = new OpenAlprWebhookProcessor.Features.Users.RefreshToken
            {
                Token = "valid-refresh-token",
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = "127.0.0.1"
            };
            user.RefreshTokens.Add(activeRefreshToken);
            
            var command = new RevokeTokenCommand("valid-refresh-token", "127.0.0.1");
            
            _mockUserRepository.GetByRefreshTokenAsync(command.Token, Arg.Any<CancellationToken>())
                .Returns(user);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            await _mockUserRepository.Received(1).GetByRefreshTokenAsync(command.Token, Arg.Any<CancellationToken>());
            _mockUserRepository.Received(1).Update(user);
            await _mockUsersUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_ValidToken_SetsCorrectRevocationData()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            var activeRefreshToken = new OpenAlprWebhookProcessor.Features.Users.RefreshToken
            {
                Token = "valid-refresh-token",
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = "127.0.0.1"
            };
            user.RefreshTokens.Add(activeRefreshToken);
            
            var command = new RevokeTokenCommand("valid-refresh-token", "192.168.1.100");
            var beforeRevocation = DateTime.UtcNow;
            
            _mockUserRepository.GetByRefreshTokenAsync(command.Token, Arg.Any<CancellationToken>())
                .Returns(user);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            activeRefreshToken.Revoked.Should().NotBeNull();
            activeRefreshToken.Revoked.Should().BeAfter(beforeRevocation);
            activeRefreshToken.RevokedByIp.Should().Be("192.168.1.100");
        }
    }
} 