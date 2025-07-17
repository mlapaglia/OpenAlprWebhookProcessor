using FluentAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Services;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Tests.Features.Users.Services
{
    [TestFixture]
    public class PasswordServiceTests
    {
        private PasswordService _passwordService;

        [SetUp]
        public void SetUp()
        {
            _passwordService = new PasswordService();
        }

        #region CreatePasswordHash Tests

        [Test]
        public void CreatePasswordHash_ValidPassword_CreatesHashAndSalt()
        {
            // Arrange
            var password = "testPassword123";

            // Act
            _passwordService.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            // Assert
            passwordHash.Should().NotBeNull();
            passwordSalt.Should().NotBeNull();
            passwordHash.Length.Should().Be(64); // HMACSHA512 produces 64-byte hash
            passwordSalt.Length.Should().Be(128); // HMACSHA512 key is 128 bytes
        }

        [Test]
        public void CreatePasswordHash_SamePassword_ProducesDifferentHashes()
        {
            // Arrange
            var password = "testPassword123";

            // Act
            _passwordService.CreatePasswordHash(password, out byte[] passwordHash1, out byte[] passwordSalt1);
            _passwordService.CreatePasswordHash(password, out byte[] passwordHash2, out byte[] passwordSalt2);

            // Assert
            passwordHash1.Should().NotBeEquivalentTo(passwordHash2);
            passwordSalt1.Should().NotBeEquivalentTo(passwordSalt2);
        }

        [Test]
        public void CreatePasswordHash_DifferentPasswords_ProducesDifferentHashes()
        {
            // Arrange
            var password1 = "testPassword123";
            var password2 = "differentPassword456";

            // Act
            _passwordService.CreatePasswordHash(password1, out byte[] passwordHash1, out byte[] passwordSalt1);
            _passwordService.CreatePasswordHash(password2, out byte[] passwordHash2, out byte[] passwordSalt2);

            // Assert
            passwordHash1.Should().NotBeEquivalentTo(passwordHash2);
            passwordSalt1.Should().NotBeEquivalentTo(passwordSalt2);
        }

        [Test]
        public void CreatePasswordHash_NullPassword_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => 
                _passwordService.CreatePasswordHash(null, out byte[] passwordHash, out byte[] passwordSalt));
            
            exception.ParamName.Should().Be("password");
        }

        [Test]
        public void CreatePasswordHash_EmptyPassword_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _passwordService.CreatePasswordHash("", out byte[] passwordHash, out byte[] passwordSalt));
            
            exception.ParamName.Should().Be("password");
            exception.Message.Should().Contain("Value cannot be empty or whitespace only string.");
        }

        [Test]
        public void CreatePasswordHash_WhitespacePassword_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _passwordService.CreatePasswordHash("   ", out byte[] passwordHash, out byte[] passwordSalt));
            
            exception.ParamName.Should().Be("password");
            exception.Message.Should().Contain("Value cannot be empty or whitespace only string.");
        }

        [Test]
        public void CreatePasswordHash_ComplexPassword_CreatesValidHash()
        {
            // Arrange
            var password = "ComplexP@ssw0rd!#$%^&*()_+=-{}[]|\\:;\"'<>,.?/~`1234567890";

            // Act
            _passwordService.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            // Assert
            passwordHash.Should().NotBeNull();
            passwordSalt.Should().NotBeNull();
            passwordHash.Length.Should().Be(64);
            passwordSalt.Length.Should().Be(128);
        }

        #endregion

        #region VerifyPasswordHash Tests

        [Test]
        public void VerifyPasswordHash_CorrectPassword_ReturnsTrue()
        {
            // Arrange
            var password = "testPassword123";
            _passwordService.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            // Act
            var result = _passwordService.VerifyPasswordHash(password, passwordHash, passwordSalt);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void VerifyPasswordHash_IncorrectPassword_ReturnsFalse()
        {
            // Arrange
            var password = "testPassword123";
            var wrongPassword = "wrongPassword456";
            _passwordService.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            // Act
            var result = _passwordService.VerifyPasswordHash(wrongPassword, passwordHash, passwordSalt);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void VerifyPasswordHash_NullPassword_ThrowsArgumentNullException()
        {
            // Arrange
            var password = "testPassword123";
            _passwordService.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => 
                _passwordService.VerifyPasswordHash(null, passwordHash, passwordSalt));
            
            exception.ParamName.Should().Be("password");
        }

        [Test]
        public void VerifyPasswordHash_EmptyPassword_ThrowsArgumentException()
        {
            // Arrange
            var password = "testPassword123";
            _passwordService.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _passwordService.VerifyPasswordHash("", passwordHash, passwordSalt));
            
            exception.ParamName.Should().Be("password");
            exception.Message.Should().Contain("Value cannot be empty or whitespace only string.");
        }

        [Test]
        public void VerifyPasswordHash_WhitespacePassword_ThrowsArgumentException()
        {
            // Arrange
            var password = "testPassword123";
            _passwordService.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _passwordService.VerifyPasswordHash("   ", passwordHash, passwordSalt));
            
            exception.ParamName.Should().Be("password");
            exception.Message.Should().Contain("Value cannot be empty or whitespace only string.");
        }

        [Test]
        public void VerifyPasswordHash_InvalidHashLength_ThrowsArgumentException()
        {
            // Arrange
            var password = "testPassword123";
            _passwordService.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);
            var invalidHash = new byte[32]; // Wrong length

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _passwordService.VerifyPasswordHash(password, invalidHash, passwordSalt));
            
            exception.ParamName.Should().Be("storedHash");
            exception.Message.Should().Contain("Invalid length of password hash (64 bytes expected).");
        }

        [Test]
        public void VerifyPasswordHash_InvalidSaltLength_ThrowsArgumentException()
        {
            // Arrange
            var password = "testPassword123";
            _passwordService.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);
            var invalidSalt = new byte[64]; // Wrong length

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _passwordService.VerifyPasswordHash(password, passwordHash, invalidSalt));
            
            exception.ParamName.Should().Be("storedSalt");
            exception.Message.Should().Contain("Invalid length of password salt (128 bytes expected).");
        }

        [Test]
        public void VerifyPasswordHash_CorruptedHash_ReturnsFalse()
        {
            // Arrange
            var password = "testPassword123";
            _passwordService.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);
            
            // Corrupt the hash by changing one byte
            passwordHash[0] = (byte)(passwordHash[0] ^ 0xFF);

            // Act
            var result = _passwordService.VerifyPasswordHash(password, passwordHash, passwordSalt);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void VerifyPasswordHash_CorruptedSalt_ReturnsFalse()
        {
            // Arrange
            var password = "testPassword123";
            _passwordService.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);
            
            // Corrupt the salt by changing one byte
            passwordSalt[0] = (byte)(passwordSalt[0] ^ 0xFF);

            // Act
            var result = _passwordService.VerifyPasswordHash(password, passwordHash, passwordSalt);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void VerifyPasswordHash_CaseSensitive_ReturnsFalse()
        {
            // Arrange
            var password = "testPassword123";
            var wrongCasePassword = "TESTPASSWORD123";
            _passwordService.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            // Act
            var result = _passwordService.VerifyPasswordHash(wrongCasePassword, passwordHash, passwordSalt);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void VerifyPasswordHash_ComplexPassword_VerifiesCorrectly()
        {
            // Arrange
            var password = "ComplexP@ssw0rd!#$%^&*()_+=-{}[]|\\:;\"'<>,.?/~`1234567890";
            _passwordService.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            // Act
            var result = _passwordService.VerifyPasswordHash(password, passwordHash, passwordSalt);

            // Assert
            result.Should().BeTrue();
        }

        #endregion

        #region Integration Tests

        [Test]
        public void CreateAndVerifyPasswordHash_MultiplePasswords_WorksCorrectly()
        {
            // Arrange
            var passwords = new[] { "password1", "password2", "password3", "password4" };
            var hashSaltPairs = new (byte[] hash, byte[] salt)[passwords.Length];

            // Act - Create hashes
            for (int i = 0; i < passwords.Length; i++)
            {
                _passwordService.CreatePasswordHash(passwords[i], out byte[] hash, out byte[] salt);
                hashSaltPairs[i] = (hash, salt);
            }

            // Assert - Verify correct passwords
            for (int i = 0; i < passwords.Length; i++)
            {
                var result = _passwordService.VerifyPasswordHash(passwords[i], hashSaltPairs[i].hash, hashSaltPairs[i].salt);
                result.Should().BeTrue($"Password {i} should verify correctly");
            }

            // Assert - Verify incorrect passwords
            for (int i = 0; i < passwords.Length; i++)
            {
                for (int j = 0; j < passwords.Length; j++)
                {
                    if (i != j)
                    {
                        var result = _passwordService.VerifyPasswordHash(passwords[i], hashSaltPairs[j].hash, hashSaltPairs[j].salt);
                        result.Should().BeFalse($"Password {i} should not verify with hash/salt from password {j}");
                    }
                }
            }
        }

        [Test]
        public void CreatePasswordHash_GeneratesSecureRandomSalt()
        {
            // Arrange
            var password = "testPassword123";
            var saltCollector = new byte[10][];

            // Act
            for (int i = 0; i < 10; i++)
            {
                _passwordService.CreatePasswordHash(password, out byte[] hash, out byte[] salt);
                saltCollector[i] = salt;
            }

            // Assert - All salts should be different
            for (int i = 0; i < saltCollector.Length; i++)
            {
                for (int j = i + 1; j < saltCollector.Length; j++)
                {
                    saltCollector[i].Should().NotBeEquivalentTo(saltCollector[j], 
                        $"Salt {i} should be different from salt {j}");
                }
            }
        }

        #endregion
    }
} 