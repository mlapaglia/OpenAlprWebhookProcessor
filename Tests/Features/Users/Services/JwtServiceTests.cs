using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Users.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Tests.TestHelpers;
using System.Security.Cryptography;

namespace Tests.Features.Users.Services
{
    [TestFixture]
    public class JwtServiceTests
    {
        private JwtService _jwtService;
        private IUsersUnitOfWork _mockUsersUnitOfWork;
        private IJwtKeyRepository _mockJwtKeyRepository;

        [SetUp]
        public void SetUp()
        {
            _mockUsersUnitOfWork = Substitute.For<IUsersUnitOfWork>();
            _mockJwtKeyRepository = Substitute.For<IJwtKeyRepository>();
            
            _mockUsersUnitOfWork.JwtKeys.Returns(_mockJwtKeyRepository);
            
            _jwtService = new JwtService(_mockUsersUnitOfWork);
        }

        [TearDown]
        public void TearDown()
        {
            _mockUsersUnitOfWork.Dispose();
        }

        #region Constructor Tests

        [Test]
        public void Constructor_NullUsersUnitOfWork_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new JwtService(null));
            exception.ParamName.Should().Be("usersUnitOfWork");
        }

        #endregion

        #region GenerateJwtTokenAsync Tests

        [Test]
        public async Task GenerateJwtTokenAsync_ValidUser_ReturnsValidJwtToken()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            
            // Generate a proper JWT key with random bytes
            var keyBytes = new byte[128];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(keyBytes);
            }
            
            var jwtKey = new JwtKey
            {
                Id = Guid.NewGuid(),
                Key = Convert.ToBase64String(keyBytes)
            };
            
            _mockJwtKeyRepository.GetFirstJwtKeyAsync(Arg.Any<CancellationToken>())
                .Returns(jwtKey);

            // Act
            var result = await _jwtService.GenerateJwtTokenAsync(user, CancellationToken.None);

            // Assert
            result.Should().NotBeNullOrEmpty();
            
            // Verify it's a valid JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(result);
            
            token.Should().NotBeNull();
            token.Claims.Should().Contain(c => c.Type == "unique_name" && c.Value == user.Id.ToString());
        }

        [Test]
        public async Task GenerateJwtTokenAsync_ValidUser_TokenHasCorrectExpiration()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            var jwtKey = new JwtKey
            {
                Id = Guid.NewGuid(),
                Key = Convert.ToBase64String(new byte[128])
            };
            
            _mockJwtKeyRepository.GetFirstJwtKeyAsync(Arg.Any<CancellationToken>())
                .Returns(jwtKey);

            var beforeGeneration = DateTime.UtcNow;

            // Act
            var result = await _jwtService.GenerateJwtTokenAsync(user, CancellationToken.None);

            var afterGeneration = DateTime.UtcNow;

            // Assert
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(result);
            
            token.ValidTo.Should().BeAfter(beforeGeneration.AddMinutes(14));
            token.ValidTo.Should().BeBefore(afterGeneration.AddMinutes(16));
        }

        [Test]
        public async Task GenerateJwtTokenAsync_ValidUser_TokenHasCorrectSigningAlgorithm()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            var jwtKey = new JwtKey
            {
                Id = Guid.NewGuid(),
                Key = Convert.ToBase64String(new byte[128])
            };
            
            _mockJwtKeyRepository.GetFirstJwtKeyAsync(Arg.Any<CancellationToken>())
                .Returns(jwtKey);

            // Act
            var result = await _jwtService.GenerateJwtTokenAsync(user, CancellationToken.None);

            // Assert
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(result);
            
            token.Header.Alg.Should().Be(SecurityAlgorithms.HmacSha256);
        }

        [Test]
        public async Task GenerateJwtTokenAsync_CallsGetJwtSecretKeyAsync()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            var jwtKey = new JwtKey
            {
                Id = Guid.NewGuid(),
                Key = Convert.ToBase64String(new byte[128])
            };
            
            _mockJwtKeyRepository.GetFirstJwtKeyAsync(Arg.Any<CancellationToken>())
                .Returns(jwtKey);

            // Act
            await _jwtService.GenerateJwtTokenAsync(user, CancellationToken.None);

            // Assert
            await _mockJwtKeyRepository.Received(1).GetFirstJwtKeyAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task GenerateJwtTokenAsync_DifferentUsers_GenerateDifferentTokens()
        {
            // Arrange
            var user1 = TestDataFactory.CreateTestUser("user1", "First1", "Last1");
            var user2 = TestDataFactory.CreateTestUser("user2", "First2", "Last2");
            user1.Id = 1;
            user2.Id = 2;
            
            var jwtKey = new JwtKey
            {
                Id = Guid.NewGuid(),
                Key = Convert.ToBase64String(new byte[128])
            };
            
            _mockJwtKeyRepository.GetFirstJwtKeyAsync(Arg.Any<CancellationToken>())
                .Returns(jwtKey);

            // Act
            var token1 = await _jwtService.GenerateJwtTokenAsync(user1, CancellationToken.None);
            var token2 = await _jwtService.GenerateJwtTokenAsync(user2, CancellationToken.None);

            // Assert
            token1.Should().NotBe(token2);
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken1 = tokenHandler.ReadJwtToken(token1);
            var jwtToken2 = tokenHandler.ReadJwtToken(token2);
            
            var user1Claim = jwtToken1.Claims.First(c => c.Type == "unique_name");
            var user2Claim = jwtToken2.Claims.First(c => c.Type == "unique_name");
            
            user1Claim.Value.Should().Be(user1.Id.ToString());
            user2Claim.Value.Should().Be(user2.Id.ToString());
        }

        #endregion

        #region GetJwtSecretKeyAsync Tests

        [Test]
        public async Task GetJwtSecretKeyAsync_ExistingValidKey_ReturnsKey()
        {
            // Arrange
            var keyBytes = new byte[128];
            new Random().NextBytes(keyBytes);
            var jwtKey = new JwtKey
            {
                Id = Guid.NewGuid(),
                Key = Convert.ToBase64String(keyBytes)
            };
            
            _mockJwtKeyRepository.GetFirstJwtKeyAsync(Arg.Any<CancellationToken>())
                .Returns(jwtKey);

            // Act
            var result = await _jwtService.GetJwtSecretKeyAsync(CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(128);
            result.Should().BeEquivalentTo(keyBytes);
        }

        [Test]
        public async Task GetJwtSecretKeyAsync_NoExistingKey_CreatesNewKey()
        {
            // Arrange
            _mockJwtKeyRepository.GetFirstJwtKeyAsync(Arg.Any<CancellationToken>())
                .Returns((JwtKey)null);

            // Act
            var result = await _jwtService.GetJwtSecretKeyAsync(CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(128);
            
            await _mockJwtKeyRepository.Received(1).AddAsync(Arg.Any<JwtKey>(), Arg.Any<CancellationToken>());
            await _mockUsersUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task GetJwtSecretKeyAsync_ExistingKeyTooShort_UpdatesKey()
        {
            // Arrange
            var shortKeyBytes = new byte[64]; // Too short
            new Random().NextBytes(shortKeyBytes);
            var jwtKey = new JwtKey
            {
                Id = Guid.NewGuid(),
                Key = Convert.ToBase64String(shortKeyBytes)
            };
            
            _mockJwtKeyRepository.GetFirstJwtKeyAsync(Arg.Any<CancellationToken>())
                .Returns(jwtKey);

            // Act
            var result = await _jwtService.GetJwtSecretKeyAsync(CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(128);
            
            jwtKey.Key.Should().NotBe(Convert.ToBase64String(shortKeyBytes));
            await _mockUsersUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task GetJwtSecretKeyAsync_PassesCancellationToken()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var jwtKey = new JwtKey
            {
                Id = Guid.NewGuid(),
                Key = Convert.ToBase64String(new byte[128])
            };
            
            _mockJwtKeyRepository.GetFirstJwtKeyAsync(cancellationToken)
                .Returns(jwtKey);

            // Act
            await _jwtService.GetJwtSecretKeyAsync(cancellationToken);

            // Assert
            await _mockJwtKeyRepository.Received(1).GetFirstJwtKeyAsync(cancellationToken);
        }

        [Test]
        public async Task GetJwtSecretKeyAsync_NewKeyCreation_CreatesBase64Key()
        {
            // Arrange
            JwtKey capturedKey = null;
            
            _mockJwtKeyRepository.GetFirstJwtKeyAsync(Arg.Any<CancellationToken>())
                .Returns((JwtKey)null);
            
            await _mockJwtKeyRepository.AddAsync(Arg.Do<JwtKey>(key => capturedKey = key), Arg.Any<CancellationToken>());

            // Act
            await _jwtService.GetJwtSecretKeyAsync(CancellationToken.None);

            // Assert
            capturedKey.Should().NotBeNull();
            capturedKey.Key.Should().NotBeNullOrEmpty();
            
            // Verify it's valid base64
            var decodedBytes = Convert.FromBase64String(capturedKey.Key);
            decodedBytes.Should().HaveCount(128);
        }

        #endregion

        #region GenerateRefreshToken Tests

        [Test]
        public void GenerateRefreshToken_ValidIpAddress_ReturnsValidRefreshToken()
        {
            // Arrange
            var ipAddress = "127.0.0.1";
            var beforeGeneration = DateTime.UtcNow;

            // Act
            var result = _jwtService.GenerateRefreshToken(ipAddress);

            var afterGeneration = DateTime.UtcNow;

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();
            result.CreatedByIp.Should().Be(ipAddress);
            result.Created.Should().BeAfter(beforeGeneration.AddSeconds(-1));
            result.Created.Should().BeBefore(afterGeneration.AddSeconds(1));
            result.Expires.Should().BeAfter(beforeGeneration.AddDays(6));
            result.Expires.Should().BeBefore(afterGeneration.AddDays(8));
        }

        [Test]
        public void GenerateRefreshToken_DifferentIpAddresses_GeneratesDifferentTokens()
        {
            // Arrange
            var ipAddress1 = "127.0.0.1";
            var ipAddress2 = "192.168.1.1";

            // Act
            var token1 = _jwtService.GenerateRefreshToken(ipAddress1);
            var token2 = _jwtService.GenerateRefreshToken(ipAddress2);

            // Assert
            token1.Token.Should().NotBe(token2.Token);
            token1.CreatedByIp.Should().Be(ipAddress1);
            token2.CreatedByIp.Should().Be(ipAddress2);
        }

        [Test]
        public void GenerateRefreshToken_SameIpAddress_GeneratesDifferentTokens()
        {
            // Arrange
            var ipAddress = "127.0.0.1";

            // Act
            var token1 = _jwtService.GenerateRefreshToken(ipAddress);
            var token2 = _jwtService.GenerateRefreshToken(ipAddress);

            // Assert
            token1.Token.Should().NotBe(token2.Token);
            token1.CreatedByIp.Should().Be(token2.CreatedByIp);
        }

        [Test]
        public void GenerateRefreshToken_ValidIpAddress_TokenIsBase64()
        {
            // Arrange
            var ipAddress = "127.0.0.1";

            // Act
            var result = _jwtService.GenerateRefreshToken(ipAddress);

            // Assert
            result.Token.Should().NotBeNullOrEmpty();
            
            // Verify it's valid base64
            var decodedBytes = Convert.FromBase64String(result.Token);
            decodedBytes.Should().HaveCount(128);
        }

        [Test]
        public void GenerateRefreshToken_NullIpAddress_SetsNullIpAddress()
        {
            // Act
            var result = _jwtService.GenerateRefreshToken(null);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();
            result.CreatedByIp.Should().BeNull();
        }

        [Test]
        public void GenerateRefreshToken_EmptyIpAddress_SetsEmptyIpAddress()
        {
            // Act
            var result = _jwtService.GenerateRefreshToken("");

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();
            result.CreatedByIp.Should().BeEmpty();
        }

        [Test]
        public void GenerateRefreshToken_IPv6Address_HandlesCorrectly()
        {
            // Arrange
            var ipv6Address = "2001:0db8:85a3:0000:0000:8a2e:0370:7334";

            // Act
            var result = _jwtService.GenerateRefreshToken(ipv6Address);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();
            result.CreatedByIp.Should().Be(ipv6Address);
        }

        [Test]
        public void GenerateRefreshToken_MultipleGenerations_AllUnique()
        {
            // Arrange
            var ipAddress = "127.0.0.1";
            var tokens = new string[100];

            // Act
            for (int i = 0; i < 100; i++)
            {
                tokens[i] = _jwtService.GenerateRefreshToken(ipAddress).Token;
            }

            // Assert
            tokens.Should().OnlyHaveUniqueItems();
        }

        #endregion
    }
} 