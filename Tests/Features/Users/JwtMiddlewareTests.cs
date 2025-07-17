using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users;
using OpenAlprWebhookProcessor.Features.Users.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Tests.TestHelpers;
using System.Security.Cryptography;

namespace Tests.Features.Users
{
    [TestFixture]
    public class JwtMiddlewareTests
    {
        private JwtMiddleware _middleware;
        private ILogger<JwtMiddleware> _mockLogger;
        private RequestDelegate _mockNext;
        private IUserService _mockUserService;
        private DefaultHttpContext _httpContext;

        [SetUp]
        public void SetUp()
        {
            _mockLogger = Substitute.For<ILogger<JwtMiddleware>>();
            _mockNext = Substitute.For<RequestDelegate>();
            _mockUserService = Substitute.For<IUserService>();
            
            _middleware = new JwtMiddleware(_mockNext, _mockLogger);
            _httpContext = new DefaultHttpContext();
        }

        #region Constructor Tests

        [Test]
        public void Constructor_ValidParameters_CreatesInstance()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => new JwtMiddleware(_mockNext, _mockLogger));
        }

        #endregion

        #region Invoke Tests

        [Test]
        public async Task Invoke_NoAuthorizationHeader_CallsNextMiddleware()
        {
            // Arrange
            _httpContext.Request.Headers.Clear();

            // Act
            await _middleware.Invoke(_httpContext, _mockUserService);

            // Assert
            await _mockNext.Received(1).Invoke(_httpContext);
        }

        [Test]
        public async Task Invoke_EmptyAuthorizationHeader_CallsNextMiddleware()
        {
            // Arrange
            _httpContext.Request.Headers.Authorization = "";

            // Act
            await _middleware.Invoke(_httpContext, _mockUserService);

            // Assert
            await _mockNext.Received(1).Invoke(_httpContext);
        }

        [Test]
        public async Task Invoke_NonBearerToken_CallsNextMiddleware()
        {
            // Arrange
            _httpContext.Request.Headers.Authorization = "Basic dXNlcjpwYXNz";

            // Act
            await _middleware.Invoke(_httpContext, _mockUserService);

            // Assert
            await _mockNext.Received(1).Invoke(_httpContext);
        }

        [Test]
        public async Task Invoke_ValidToken_AttachesUserToContext()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            var jwtSecret = new byte[128];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(jwtSecret);
            }
            
            // Create a valid JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(jwtSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("unique_name", user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            
            _httpContext.Request.Headers["Authorization"] = $"Bearer {tokenString}";
            
            _mockUserService.GetJwtSecretKeyAsync().Returns(jwtSecret);
            _mockUserService.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

            // Act
            await _middleware.Invoke(_httpContext, _mockUserService);

            // Assert
            _httpContext.Items["User"].Should().Be(user);
            await _mockNext.Received(1).Invoke(_httpContext);
        }

        [Test]
        public async Task Invoke_InvalidToken_DoesNotAttachUserToContext()
        {
            // Arrange
            _httpContext.Request.Headers["Authorization"] = "Bearer invalid-token";
            
            _mockUserService.GetJwtSecretKeyAsync().Returns(new byte[128]);

            // Act
            await _middleware.Invoke(_httpContext, _mockUserService);

            // Assert
            _httpContext.Items.Should().NotContainKey("User");
            await _mockNext.Received(1).Invoke(_httpContext);
        }

        [Test]
        public async Task Invoke_ExpiredToken_DoesNotAttachUserToContext()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            var jwtSecretKey = GenerateRandomKey();
            var expiredToken = GenerateExpiredJwtToken(user, jwtSecretKey);

            _httpContext.Request.Headers.Authorization = $"Bearer {expiredToken}";
            _mockUserService.GetJwtSecretKeyAsync()
                .Returns(jwtSecretKey);

            // Act
            await _middleware.Invoke(_httpContext, _mockUserService);

            // Assert
            _httpContext.Items.Should().NotContainKey("User");
            await _mockNext.Received(1).Invoke(_httpContext);
        }

        [Test]
        public async Task Invoke_TokenWithWrongSignature_DoesNotAttachUserToContext()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            var correctKey = GenerateRandomKey();
            var wrongKey = GenerateRandomKey();
            var token = GenerateJwtToken(user, wrongKey);

            _httpContext.Request.Headers.Authorization = $"Bearer {token}";
            _mockUserService.GetJwtSecretKeyAsync()
                .Returns(correctKey);

            // Act
            await _middleware.Invoke(_httpContext, _mockUserService);

            // Assert
            _httpContext.Items.Should().NotContainKey("User");
            await _mockNext.Received(1).Invoke(_httpContext);
        }

        [Test]
        public async Task Invoke_UserNotFound_DoesNotAttachUserToContext()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            var jwtSecretKey = GenerateRandomKey();
            var token = GenerateJwtToken(user, jwtSecretKey);

            _httpContext.Request.Headers.Authorization = $"Bearer {token}";
            _mockUserService.GetJwtSecretKeyAsync()
                .Returns(jwtSecretKey);
            _mockUserService.GetByIdAsync(user.Id, Arg.Any<System.Threading.CancellationToken>())
                .Returns((User)null);

            // Act
            await _middleware.Invoke(_httpContext, _mockUserService);

            // Assert
            _httpContext.Items["User"].Should().BeNull();
            await _mockNext.Received(1).Invoke(_httpContext);
        }

        [Test]
        public async Task Invoke_MultipleAuthorizationHeaders_UsesFirstOne()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            var jwtSecretKey = GenerateRandomKey();
            var validToken = GenerateJwtToken(user, jwtSecretKey);
            var invalidToken = "invalid.token";

            _httpContext.Request.Headers.Authorization = new[] { $"Bearer {validToken}", $"Bearer {invalidToken}" };
            _mockUserService.GetJwtSecretKeyAsync()
                .Returns(jwtSecretKey);
            _mockUserService.GetByIdAsync(user.Id, Arg.Any<System.Threading.CancellationToken>())
                .Returns(user);

            // Act
            await _middleware.Invoke(_httpContext, _mockUserService);

            // Assert
            _httpContext.Items["User"].Should().Be(user);
            await _mockNext.Received(1).Invoke(_httpContext);
        }

        [Test]
        public async Task Invoke_TokenValidationThrowsException_DoesNotAttachUserToContext()
        {
            // Arrange
            var malformedToken = "this.is.not.a.valid.jwt.token.format";
            var jwtSecretKey = GenerateRandomKey();

            _httpContext.Request.Headers.Authorization = $"Bearer {malformedToken}";
            _mockUserService.GetJwtSecretKeyAsync()
                .Returns(jwtSecretKey);

            // Act
            await _middleware.Invoke(_httpContext, _mockUserService);

            // Assert
            _httpContext.Items.Should().NotContainKey("User");
            await _mockNext.Received(1).Invoke(_httpContext);
        }

        [Test]
        public async Task Invoke_UserServiceThrowsException_DoesNotAttachUserToContext()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            var jwtSecretKey = GenerateRandomKey();
            var token = GenerateJwtToken(user, jwtSecretKey);

            _httpContext.Request.Headers.Authorization = $"Bearer {token}";
            _mockUserService.GetJwtSecretKeyAsync()
                .Returns(jwtSecretKey);
            _mockUserService.GetByIdAsync(user.Id, Arg.Any<System.Threading.CancellationToken>())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            await _middleware.Invoke(_httpContext, _mockUserService);

            // Assert
            _httpContext.Items.Should().NotContainKey("User");
            await _mockNext.Received(1).Invoke(_httpContext);
        }

        [Test]
        public async Task Invoke_ValidToken_CallsUserServiceCorrectly()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            var jwtSecretKey = GenerateRandomKey();
            var token = GenerateJwtToken(user, jwtSecretKey);

            _httpContext.Request.Headers.Authorization = $"Bearer {token}";
            _mockUserService.GetJwtSecretKeyAsync()
                .Returns(jwtSecretKey);
            _mockUserService.GetByIdAsync(user.Id, Arg.Any<System.Threading.CancellationToken>())
                .Returns(user);

            // Act
            await _middleware.Invoke(_httpContext, _mockUserService);

            // Assert
            await _mockUserService.Received(1).GetJwtSecretKeyAsync();
            await _mockUserService.Received(1).GetByIdAsync(user.Id, Arg.Any<System.Threading.CancellationToken>());
        }

        [Test]
        public async Task Invoke_BearerTokenWithExtraSpaces_HandlesCorrectly()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            var jwtSecretKey = GenerateRandomKey();
            var token = GenerateJwtToken(user, jwtSecretKey);

            _httpContext.Request.Headers.Authorization = $"Bearer {token}"; // Correct format
            _mockUserService.GetJwtSecretKeyAsync()
                .Returns(jwtSecretKey);
            _mockUserService.GetByIdAsync(user.Id, Arg.Any<System.Threading.CancellationToken>())
                .Returns(user);

            // Act
            await _middleware.Invoke(_httpContext, _mockUserService);

            // Assert
            _httpContext.Items["User"].Should().Be(user);
            await _mockNext.Received(1).Invoke(_httpContext);
        }

        #endregion

        #region Helper Methods

        private byte[] GenerateRandomKey()
        {
            var key = new byte[128];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }
            return key;
        }

        private string GenerateJwtToken(User user, byte[] jwtSecretKey)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(jwtSecretKey),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateExpiredJwtToken(User user, byte[] jwtSecretKey)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var now = DateTime.UtcNow;
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                NotBefore = now.AddMinutes(-10),
                Expires = now.AddMinutes(-1), // Expired 1 minute ago
                IssuedAt = now.AddMinutes(-10),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(jwtSecretKey),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        #endregion
    }
} 