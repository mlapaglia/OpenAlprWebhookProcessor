using AwesomeAssertions;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Users.Services;
using System.IdentityModel.Tokens.Jwt;

namespace Tests.Features.Users.Services
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class JwtServiceTests
    {
        private JwtService _jwtService;
        private IUsersUnitOfWork _usersUnitOfWork;
        private IJwtKeyRepository _jwtKeyRepository;
        private JwtKey _jwtKey;
        private User _user;

        [SetUp]
        public void SetUp()
        {
            _usersUnitOfWork = Substitute.For<IUsersUnitOfWork>();
            _jwtKeyRepository = Substitute.For<IJwtKeyRepository>();

            _jwtKey = new JwtKey
            {
                Id = Guid.NewGuid(),
                Key = Convert.ToBase64String(new byte[128])
            };

            _user = new User
            {
                Id = 1,
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                RefreshTokens = new List<RefreshToken>()
            };

            _usersUnitOfWork.JwtKeys.Returns(_jwtKeyRepository);
            _jwtKeyRepository.GetFirstAsync(Arg.Any<CancellationToken>()).Returns(_jwtKey);

            _jwtService = new JwtService(_usersUnitOfWork);
        }

        [TearDown]
        public void TearDown()
        {
            _usersUnitOfWork.Dispose();
        }

        [Test]
        public void Constructor_WithNullUnitOfWork_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => new JwtService(null));
        }

        [Test]
        public void Constructor_WithValidUnitOfWork_InitializesService()
        {
            // Arrange & Act
            var service = new JwtService(_usersUnitOfWork);

            // Assert
            service.Should().NotBeNull();
        }

        [Test]
        public async Task GenerateJwtTokenAsync_WithValidUser_GeneratesValidToken()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            // Act
            var token = await _jwtService.GenerateJwtTokenAsync(_user, cancellationToken);

            // Assert
            token.Should().NotBeNull();
            token.Should().NotBeEmpty();
            
            // Verify token structure
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);
            
            jsonToken.Should().NotBeNull();
            jsonToken.Claims.Should().Contain(c => c.Type == "unique_name" && c.Value == _user.Id.ToString());
        }

        [Test]
        public async Task GenerateJwtTokenAsync_TokenHasCorrectExpiration()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var beforeGeneration = DateTime.UtcNow.AddMinutes(15);

            // Act
            var token = await _jwtService.GenerateJwtTokenAsync(_user, cancellationToken);

            // Assert
            var afterGeneration = DateTime.UtcNow.AddMinutes(15);
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);
            
            jsonToken.ValidTo.Should().BeAfter(beforeGeneration.AddMinutes(-1));
            jsonToken.ValidTo.Should().BeBefore(afterGeneration.AddMinutes(1));
        }

        [Test]
        public async Task GenerateJwtTokenAsync_CallsGetJwtSecretKeyAsync()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            // Act
            await _jwtService.GenerateJwtTokenAsync(_user, cancellationToken);

            // Assert
            await _jwtKeyRepository.Received(1).GetFirstAsync(cancellationToken);
        }

        [Test]
        public void GenerateJwtTokenAsync_WithNullUser_ThrowsArgumentNullException()
        {
            // Arrange
            User nullUser = null;
            var cancellationToken = new CancellationToken();

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => 
                _jwtService.GenerateJwtTokenAsync(nullUser, cancellationToken));
        }

        [Test]
        public async Task GetJwtSecretKeyAsync_WithExistingKey_ReturnsExistingKey()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var expectedKey = new byte[128];
            new Random().NextBytes(expectedKey);
            _jwtKey.Key = Convert.ToBase64String(expectedKey);

            // Act
            var result = await _jwtService.GetJwtSecretKeyAsync(cancellationToken);

            // Assert
            result.Should().BeEquivalentTo(expectedKey);
            await _jwtKeyRepository.Received(1).GetFirstAsync(cancellationToken);
            await _jwtKeyRepository.DidNotReceive().AddAsync(Arg.Any<JwtKey>(), Arg.Any<CancellationToken>());
            await _usersUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task GetJwtSecretKeyAsync_WithNullKey_CreatesNewKey()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            _jwtKeyRepository.GetFirstAsync(cancellationToken).Returns((JwtKey)null);

            // Act
            var result = await _jwtService.GetJwtSecretKeyAsync(cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Length.Should().Be(128);
            await _jwtKeyRepository.Received(1).GetFirstAsync(cancellationToken);
            await _jwtKeyRepository.Received(1).AddAsync(Arg.Any<JwtKey>(), cancellationToken);
            await _usersUnitOfWork.Received(1).SaveChangesAsync(cancellationToken);
        }

        [Test]
        public async Task GetJwtSecretKeyAsync_WithShortKey_GeneratesNewKey()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var shortKey = new byte[64]; // Less than 128 bytes
            new Random().NextBytes(shortKey);
            _jwtKey.Key = Convert.ToBase64String(shortKey);

            // Act
            var result = await _jwtService.GetJwtSecretKeyAsync(cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Length.Should().Be(128);
            await _jwtKeyRepository.Received(1).GetFirstAsync(cancellationToken);
            await _jwtKeyRepository.DidNotReceive().AddAsync(Arg.Any<JwtKey>(), Arg.Any<CancellationToken>());
            await _usersUnitOfWork.Received(1).SaveChangesAsync(cancellationToken);
        }

        [Test]
        public async Task GetJwtSecretKeyAsync_WithKeyOfExactLength_DoesNotRecreateKey()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var exactLengthKey = new byte[128];
            new Random().NextBytes(exactLengthKey);
            _jwtKey.Key = Convert.ToBase64String(exactLengthKey);

            // Act
            var result = await _jwtService.GetJwtSecretKeyAsync(cancellationToken);

            // Assert
            result.Should().BeEquivalentTo(exactLengthKey);
            await _jwtKeyRepository.Received(1).GetFirstAsync(cancellationToken);
            await _jwtKeyRepository.DidNotReceive().AddAsync(Arg.Any<JwtKey>(), Arg.Any<CancellationToken>());
            await _usersUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public void GetJwtSecretKeyAsync_WithRepositoryException_ThrowsException()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var exception = new InvalidOperationException("Database error");
            _jwtKeyRepository.GetFirstAsync(cancellationToken).ThrowsAsync(exception);

            // Act & Assert
            var thrownException = Assert.ThrowsAsync<InvalidOperationException>(() => 
                _jwtService.GetJwtSecretKeyAsync(cancellationToken));
            
            thrownException.Message.Should().Be("Database error");
        }

        [Test]
        public void GenerateRefreshToken_WithValidIpAddress_GeneratesValidToken()
        {
            // Arrange
            var ipAddress = "192.168.1.100";

            // Act
            var refreshToken = _jwtService.GenerateRefreshToken(ipAddress);

            // Assert
            refreshToken.Should().NotBeNull();
            refreshToken.Token.Should().NotBeEmpty();
            refreshToken.Created.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            refreshToken.Expires.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(1));
            refreshToken.CreatedByIp.Should().Be(ipAddress);
            refreshToken.Revoked.Should().BeNull();
            refreshToken.RevokedByIp.Should().BeNull();
            refreshToken.ReplacedByToken.Should().BeNull();
        }

        [Test]
        public void GenerateRefreshToken_WithNullIpAddress_GeneratesValidToken()
        {
            // Arrange
            string ipAddress = null;

            // Act
            var refreshToken = _jwtService.GenerateRefreshToken(ipAddress);

            // Assert
            refreshToken.Should().NotBeNull();
            refreshToken.Token.Should().NotBeEmpty();
            refreshToken.CreatedByIp.Should().BeNull();
        }

        [Test]
        public void GenerateRefreshToken_WithEmptyIpAddress_GeneratesValidToken()
        {
            // Arrange
            var ipAddress = "";

            // Act
            var refreshToken = _jwtService.GenerateRefreshToken(ipAddress);

            // Assert
            refreshToken.Should().NotBeNull();
            refreshToken.Token.Should().NotBeEmpty();
            refreshToken.CreatedByIp.Should().Be("");
        }

        [Test]
        public void GenerateRefreshToken_MultipleTokens_GeneratesUniqueTokens()
        {
            // Arrange
            var ipAddress = "192.168.1.100";
            var tokens = new List<string>();

            // Act
            for (int i = 0; i < 10; i++)
            {
                var refreshToken = _jwtService.GenerateRefreshToken(ipAddress);
                tokens.Add(refreshToken.Token);
            }

            // Assert
            tokens.Should().OnlyHaveUniqueItems();
        }

        [Test]
        public void GenerateRefreshToken_TokenShouldNotBeExpired()
        {
            // Arrange
            var ipAddress = "192.168.1.100";

            // Act
            var refreshToken = _jwtService.GenerateRefreshToken(ipAddress);

            // Assert
            refreshToken.IsExpired.Should().BeFalse();
        }

        [Test]
        public void GenerateRefreshToken_TokenShouldBeBase64Encoded()
        {
            // Arrange
            var ipAddress = "192.168.1.100";

            // Act
            var refreshToken = _jwtService.GenerateRefreshToken(ipAddress);

            // Assert
            // Should not throw exception when decoding
            Assert.DoesNotThrow(() => Convert.FromBase64String(refreshToken.Token));
            
            // Decoded bytes should be 128 in length
            var decodedBytes = Convert.FromBase64String(refreshToken.Token);
            decodedBytes.Length.Should().Be(128);
        }

        [Test]
        public async Task GenerateJwtTokenAsync_WithDifferentUsers_GeneratesDifferentTokens()
        {
            // Arrange
            var user1 = new User { Id = 1, Username = "user1" };
            var user2 = new User { Id = 2, Username = "user2" };
            var cancellationToken = new CancellationToken();

            // Act
            var token1 = await _jwtService.GenerateJwtTokenAsync(user1, cancellationToken);
            var token2 = await _jwtService.GenerateJwtTokenAsync(user2, cancellationToken);

            // Assert
            token1.Should().NotBe(token2);
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken1 = tokenHandler.ReadJwtToken(token1);
            var jsonToken2 = tokenHandler.ReadJwtToken(token2);
            
            jsonToken1.Claims.Should().Contain(c => c.Type == "unique_name" && c.Value == "1");
            jsonToken2.Claims.Should().Contain(c => c.Type == "unique_name" && c.Value == "2");
        }

        [Test]
        public async Task GenerateJwtTokenAsync_UsesHmacSha256Algorithm()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            // Act
            var token = await _jwtService.GenerateJwtTokenAsync(_user, cancellationToken);

            // Assert
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);
            
            jsonToken.Header.Alg.Should().Be(SecurityAlgorithms.HmacSha256);
        }

        [Test]
        public async Task GetJwtSecretKeyAsync_WithConcurrentCalls_HandlesCorrectly()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var tasks = new List<Task<byte[]>>();

            // Act
            for (int i = 0; i < 5; i++)
            {
                tasks.Add(_jwtService.GetJwtSecretKeyAsync(cancellationToken));
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            foreach (var result in results)
            {
                result.Should().NotBeNull();
                result.Length.Should().Be(128);
            }
        }

        [Test]
        public void GetJwtSecretKeyAsync_WithSaveChangesException_ThrowsException()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            _jwtKeyRepository.GetFirstAsync(cancellationToken).Returns((JwtKey)null);
            var exception = new InvalidOperationException("Save failed");
            _usersUnitOfWork.SaveChangesAsync(cancellationToken).ThrowsAsync(exception);

            // Act & Assert
            var thrownException = Assert.ThrowsAsync<InvalidOperationException>(() => 
                _jwtService.GetJwtSecretKeyAsync(cancellationToken));
            
            thrownException.Message.Should().Be("Save failed");
        }
    }
} 