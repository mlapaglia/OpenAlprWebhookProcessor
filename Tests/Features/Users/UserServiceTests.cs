using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users;
using OpenAlprWebhookProcessor.Features.Users.Data;
using System.IdentityModel.Tokens.Jwt;
using Tests.TestHelpers;

using Mediator;
namespace Tests.Features.Users
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class UserServiceTests : TestBase
    {
        private UserService _userService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _userService = new UserService(UsersContext);
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public async Task CreateAsync_WithValidUserAndPassword_CreatesUserSuccessfully()
        {
            // Arrange
            var user = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "johndoe"
            };
            var password = "Password123!";

            // Act
            var result = await _userService.CreateAsync(user, password);

            // Assert
            result.Should().NotBeNull();
            result.Username.Should().Be("johndoe");
            result.FirstName.Should().Be("John");
            result.LastName.Should().Be("Doe");
            result.PasswordHash.Should().NotBeNull();
            result.PasswordSalt.Should().NotBeNull();

            // Verify user was saved to database
            var savedUser = await UsersContext.Users.FirstOrDefaultAsync(u => u.Username == "johndoe");
            savedUser.Should().NotBeNull();
            savedUser.Id.Should().BeGreaterThan(0);
        }

        [Test]
        public void CreateAsync_WithEmptyPassword_ThrowsAppException()
        {
            // Arrange
            var user = new User { Username = "testuser" };

            // Act & Assert
            var exception = Assert.ThrowsAsync<AppException>(() =>
                _userService.CreateAsync(user, ""));
            exception.Message.Should().Be("Password is required");
        }

        [Test]
        public void CreateAsync_WithNullPassword_ThrowsAppException()
        {
            // Arrange
            var user = new User { Username = "testuser" };

            // Act & Assert
            var exception = Assert.ThrowsAsync<AppException>(() =>
                _userService.CreateAsync(user, null));
            exception.Message.Should().Be("Password is required");
        }

        [Test]
        public async Task CreateAsync_WithDuplicateUsername_ThrowsAppException()
        {
            // Arrange
            var existingUser = new User
            {
                Username = "duplicate",
                PasswordHash = new byte[] { 1, 2, 3 },
                PasswordSalt = new byte[] { 4, 5, 6 }
            };
            UsersContext.Users.Add(existingUser);
            await UsersContext.SaveChangesAsync();

            var newUser = new User { Username = "duplicate" };

            // Act & Assert
            var exception = Assert.ThrowsAsync<AppException>(() =>
                _userService.CreateAsync(newUser, "password"));
            exception.Message.Should().Be("Username \"duplicate\" is already taken");
        }

        [Test]
        public async Task UpdateAsync_WithValidUser_UpdatesUserSuccessfully()
        {
            // Arrange
            var user = await CreateTestUserAsync("testuser", "Original", "User");
            var updatedUser = new User
            {
                Id = user.Id,
                Username = "testuser", // Keep same username to avoid uniqueness check
                FirstName = "Updated",
                LastName = "Name"
            };

            // Act
            await _userService.UpdateAsync(updatedUser);

            // Assert
            var result = await UsersContext.Users.FindAsync(user.Id);
            result.FirstName.Should().Be("Updated");
            result.LastName.Should().Be("Name");
            result.Username.Should().Be("testuser");
        }

        [Test]
        public async Task UpdateAsync_WithPassword_UpdatesPasswordHash()
        {
            // Arrange
            var user = await CreateTestUserAsync("testuser");
            var originalHash = user.PasswordHash;
            var originalSalt = user.PasswordSalt;

            var updatedUser = new User { Id = user.Id, Username = "testuser" };

            // Act
            await _userService.UpdateAsync(updatedUser, "NewPassword123!");

            // Assert
            var result = await UsersContext.Users.FindAsync(user.Id);
            result.PasswordHash.Should().NotEqual(originalHash);
            result.PasswordSalt.Should().NotEqual(originalSalt);
        }

        [Test]
        public void UpdateAsync_WithNonExistentUser_ThrowsAppException()
        {
            // Arrange
            var user = new User { Id = 999, Username = "nonexistent" };

            // Act & Assert
            var exception = Assert.ThrowsAsync<AppException>(() =>
                _userService.UpdateAsync(user));
            exception.Message.Should().Be("User not found");
        }

        [Test]
        public async Task UpdateAsync_WithDuplicateUsername_ThrowsAppException()
        {
            // Arrange
            var user1 = await CreateTestUserAsync("user1");
            var user2 = await CreateTestUserAsync("user2");

            var updatedUser = new User
            {
                Id = user2.Id,
                Username = "user1" // Try to change to existing username
            };

            // Act & Assert
            var exception = Assert.ThrowsAsync<AppException>(() =>
                _userService.UpdateAsync(updatedUser));

            exception.Message.Should().Be("Username user2 is already taken");
        }

        [Test]
        public async Task DeleteAsync_WithExistingUser_DeletesUser()
        {
            // Arrange
            var user = await CreateTestUserAsync("testuser");

            // Act
            await _userService.DeleteAsync(user.Id);

            // Assert
            var result = await UsersContext.Users.FindAsync(user.Id);
            result.Should().BeNull();
        }

        [Test]
        public void DeleteAsync_WithNonExistentUser_DoesNotThrow()
        {
            // Arrange & Act & Assert
            Assert.DoesNotThrowAsync(() => _userService.DeleteAsync(999));
        }

        [Test]
        public async Task AuthenticateAsync_WithValidCredentials_ReturnsAuthenticateResponse()
        {
            // Arrange
            var user = await CreateTestUserAsync("testuser");
            var request = new AuthenticateRequest
            {
                Username = "testuser",
                Password = "TestPassword123!"
            };

            // Act
            var result = await _userService.AuthenticateAsync(request, "127.0.0.1", GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
            result.Username.Should().Be("testuser");
            result.JwtToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();

            // Verify JWT token structure
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.ReadJwtToken(result.JwtToken);
            jwt.Claims.Should().Contain(c => c.Type == "unique_name" && c.Value == user.Id.ToString());
        }

        [Test]
        public async Task AuthenticateAsync_WithInvalidUsername_ReturnsNull()
        {
            // Arrange
            await CreateTestUserAsync("testuser");
            var request = new AuthenticateRequest
            {
                Username = "wronguser",
                Password = "TestPassword123!"
            };

            // Act
            var result = await _userService.AuthenticateAsync(request, "127.0.0.1", GetCancellationToken());

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task AuthenticateAsync_WithInvalidPassword_ReturnsNull()
        {
            // Arrange
            await CreateTestUserAsync("testuser");
            var request = new AuthenticateRequest
            {
                Username = "testuser",
                Password = "WrongPassword"
            };

            // Act
            var result = await _userService.AuthenticateAsync(request, "127.0.0.1", GetCancellationToken());

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task AuthenticateAsync_WithValidCredentials_AddsRefreshTokenToUser()
        {
            // Arrange
            var user = await CreateTestUserAsync("testuser");
            var request = new AuthenticateRequest
            {
                Username = "testuser",
                Password = "TestPassword123!"
            };

            // Act
            await _userService.AuthenticateAsync(request, "127.0.0.1", GetCancellationToken());

            // Assert
            var updatedUser = await UsersContext.Users
                .Include(u => u.RefreshTokens)
                .FirstAsync(u => u.Id == user.Id);
            updatedUser.RefreshTokens.Should().HaveCount(1);
            updatedUser.RefreshTokens[0].CreatedByIp.Should().Be("127.0.0.1");
            updatedUser.RefreshTokens[0].IsActive.Should().BeTrue();
        }

        [Test]
        public async Task RefreshTokenAsync_WithValidToken_ReturnsNewAuthenticateResponse()
        {
            // Arrange
            var user = await CreateTestUserWithRefreshTokenAsync("testuser", "127.0.0.1");
            var refreshToken = user.RefreshTokens.First().Token;

            // Act
            var result = await _userService.RefreshTokenAsync(refreshToken, "127.0.0.1", GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
            result.JwtToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBe(refreshToken); // Should be a new token
        }

        [Test]
        public async Task RefreshTokenAsync_WithValidToken_RevokesOldTokenAndAddsNew()
        {
            // Arrange
            var user = await CreateTestUserWithRefreshTokenAsync("testuser", "127.0.0.1");
            var originalToken = user.RefreshTokens.First();

            // Act
            await _userService.RefreshTokenAsync(originalToken.Token, "192.168.1.1", GetCancellationToken());

            // Assert
            var updatedUser = await UsersContext.Users
                .Include(u => u.RefreshTokens)
                .FirstAsync(u => u.Id == user.Id);

            var revokedToken = updatedUser.RefreshTokens.First(rt => rt.Token == originalToken.Token);
            revokedToken.Revoked.Should().NotBeNull();
            revokedToken.RevokedByIp.Should().Be("192.168.1.1");
            revokedToken.IsActive.Should().BeFalse();

            updatedUser.RefreshTokens.Should().HaveCount(2);
            updatedUser.RefreshTokens.Should().Contain(rt => rt.IsActive);
        }

        [Test]
        public void RefreshTokenAsync_WithInvalidToken_ThrowsArgumentException()
        {
            // Arrange
            var invalidToken = "invalid-token";

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() =>
                _userService.RefreshTokenAsync(invalidToken, "127.0.0.1", GetCancellationToken()));
            exception.Message.Should().Be("unknown user");
        }

        [Test]
        public async Task RefreshTokenAsync_WithInactiveToken_ThrowsArgumentException()
        {
            // Arrange
            var user = await CreateTestUserWithRefreshTokenAsync("testuser", "127.0.0.1");
            var refreshToken = user.RefreshTokens.First();
            
            // Revoke the token
            refreshToken.Revoked = DateTime.UtcNow;
            UsersContext.Update(user);
            await UsersContext.SaveChangesAsync();

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() =>
                _userService.RefreshTokenAsync(refreshToken.Token, "127.0.0.1", GetCancellationToken()));
            exception.Message.Should().Be("user is already inactive");
        }

        [Test]
        public async Task RevokeTokenAsync_WithValidToken_RevokesTokenAndReturnsTrue()
        {
            // Arrange
            var user = await CreateTestUserWithRefreshTokenAsync("testuser", "127.0.0.1");
            var refreshToken = user.RefreshTokens.First();

            // Act
            var result = await _userService.RevokeTokenAsync(refreshToken.Token, "192.168.1.1", GetCancellationToken());

            // Assert
            result.Should().BeTrue();

            var updatedUser = await UsersContext.Users
                .Include(u => u.RefreshTokens)
                .FirstAsync(u => u.Id == user.Id);
            var revokedToken = updatedUser.RefreshTokens.First();
            revokedToken.Revoked.Should().NotBeNull();
            revokedToken.RevokedByIp.Should().Be("192.168.1.1");
            revokedToken.IsActive.Should().BeFalse();
        }

        [Test]
        public async Task RevokeTokenAsync_WithInvalidToken_ReturnsFalse()
        {
            // Arrange
            var invalidToken = "invalid-token";

            // Act
            var result = await _userService.RevokeTokenAsync(invalidToken, "127.0.0.1", GetCancellationToken());

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task RevokeTokenAsync_WithAlreadyRevokedToken_ReturnsFalse()
        {
            // Arrange
            var user = await CreateTestUserWithRefreshTokenAsync("testuser", "127.0.0.1");
            var refreshToken = user.RefreshTokens.First();
            
            // Revoke the token first
            refreshToken.Revoked = DateTime.UtcNow;
            UsersContext.Update(user);
            await UsersContext.SaveChangesAsync();

            // Act
            var result = await _userService.RevokeTokenAsync(refreshToken.Token, "127.0.0.1", GetCancellationToken());

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task GetAllAsync_WithNoUsers_ReturnsEmptyList()
        {
            // Act
            var result = await _userService.GetAllAsync(GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetAllAsync_WithMultipleUsers_ReturnsAllUsers()
        {
            // Arrange
            await CreateTestUserAsync("user1");
            await CreateTestUserAsync("user2");
            await CreateTestUserAsync("user3");

            // Act
            var result = await _userService.GetAllAsync(GetCancellationToken());

            // Assert
            result.Should().HaveCount(3);
            result.Should().Contain(u => u.Username == "user1");
            result.Should().Contain(u => u.Username == "user2");
            result.Should().Contain(u => u.Username == "user3");
        }

        [Test]
        public async Task GetByIdAsync_WithExistingUser_ReturnsUser()
        {
            // Arrange
            var user = await CreateTestUserAsync("testuser");

            // Act
            var result = await _userService.GetByIdAsync(user.Id, GetCancellationToken());

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
            result.Username.Should().Be("testuser");
        }

        [Test]
        public async Task GetByIdAsync_WithNonExistentUser_ReturnsNull()
        {
            // Act
            var result = await _userService.GetByIdAsync(999, GetCancellationToken());

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task GetJwtSecretKeyAsync_WithNoExistingKey_CreatesNewKey()
        {
            // Act
            var result = await _userService.GetJwtSecretKeyAsync();

            // Assert
            result.Should().NotBeNull();
            result.Length.Should().Be(128); // Base64 decoded should be 128 bytes

            // Verify key was saved to database
            var jwtKey = await UsersContext.JwtKeys.FirstOrDefaultAsync();
            jwtKey.Should().NotBeNull();
            jwtKey.Key.Should().NotBeNullOrEmpty();
        }

        [Test]
        public async Task GetJwtSecretKeyAsync_WithExistingValidKey_ReturnsExistingKey()
        {
            // Arrange
            var existingKey = Convert.ToBase64String(new byte[128]);
            var jwtKey = new JwtKey { Key = existingKey };
            UsersContext.JwtKeys.Add(jwtKey);
            await UsersContext.SaveChangesAsync();

            // Act
            var result = await _userService.GetJwtSecretKeyAsync();

            // Assert
            result.Should().NotBeNull();
            Convert.ToBase64String(result).Should().Be(existingKey);
        }

        [Test]
        public async Task GetJwtSecretKeyAsync_WithShortExistingKey_UpdatesKey()
        {
            // Arrange
            var shortKey = Convert.ToBase64String(new byte[64]); // Less than 128 bytes
            var jwtKey = new JwtKey { Key = shortKey };
            UsersContext.JwtKeys.Add(jwtKey);
            await UsersContext.SaveChangesAsync();

            // Act
            var result = await _userService.GetJwtSecretKeyAsync();

            // Assert
            result.Should().NotBeNull();
            result.Length.Should().Be(128);

            // Verify key was updated in database
            var updatedKey = await UsersContext.JwtKeys.FirstAsync();
            updatedKey.Key.Should().NotBe(shortKey);
            Convert.FromBase64String(updatedKey.Key).Length.Should().Be(128);
        }

        private async ValueTask<User> CreateTestUserAsync(string username, string firstName = "Test", string lastName = "User")
        {
            var user = new User
            {
                Username = username,
                FirstName = firstName,
                LastName = lastName
            };

            // Use the UserService to create the user with proper password hashing
            return await _userService.CreateAsync(user, "TestPassword123!");
        }

        private async ValueTask<User> CreateTestUserWithRefreshTokenAsync(string username, string ipAddress)
        {
            var user = await CreateTestUserAsync(username);
            
            // Add a refresh token
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"refresh-token-{username}")),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            user.RefreshTokens = new List<RefreshToken> { refreshToken };
            UsersContext.Update(user);
            await UsersContext.SaveChangesAsync();

            return user;
        }
    }
} 