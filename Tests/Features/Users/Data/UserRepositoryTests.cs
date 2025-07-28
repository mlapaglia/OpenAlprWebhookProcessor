using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users.Data.Repositories;
using Tests.TestHelpers;

namespace Tests.Features.Users.Data
{
    [TestFixture]
    public class UserRepositoryTests
    {
        private UsersContext _context;
        private UserRepository _repository;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<UsersContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new UsersContext(options);
            _repository = new UserRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region GetByIdAsync Tests

        [Test]
        public async Task GetByIdAsync_ExistingUser_ReturnsUser()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(user.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(user);
        }

        [Test]
        public async Task GetByIdAsync_NonExistentUser_ReturnsNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task GetByIdAsync_WithCancellationToken_RespectsToken()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var cancellationToken = new CancellationToken();

            // Act
            var result = await _repository.GetByIdAsync(user.Id, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(user);
        }

        #endregion

        #region GetAllAsync Tests

        [Test]
        public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyList()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetAllAsync_WithUsers_ReturnsAllUsers()
        {
            // Arrange
            var users = new List<User>
            {
                TestDataFactory.CreateTestUser("user1", "First1", "Last1"),
                TestDataFactory.CreateTestUser("user2", "First2", "Last2"),
                TestDataFactory.CreateTestUser("user3", "First3", "Last3")
            };

            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(users);
        }

        #endregion

        #region FindAsync Tests

        [Test]
        public async Task FindAsync_MatchingPredicate_ReturnsMatchingUsers()
        {
            // Arrange
            var users = new List<User>
            {
                TestDataFactory.CreateTestUser("user1", "John", "Doe"),
                TestDataFactory.CreateTestUser("user2", "Jane", "Smith"),
                TestDataFactory.CreateTestUser("user3", "John", "Johnson")
            };

            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.FindAsync(u => u.FirstName == "John");

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(u => u.Username == "user1");
            result.Should().Contain(u => u.Username == "user3");
        }

        [Test]
        public async Task FindAsync_NoMatches_ReturnsEmptyList()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("user1", "John", "Doe");
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.FindAsync(u => u.FirstName == "NonExistent");

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region FirstOrDefaultAsync Tests

        [Test]
        public async Task FirstOrDefaultAsync_MatchingPredicate_ReturnsFirstMatch()
        {
            // Arrange
            var users = new List<User>
            {
                TestDataFactory.CreateTestUser("user1", "John", "Doe"),
                TestDataFactory.CreateTestUser("user2", "John", "Smith")
            };

            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.FirstOrDefaultAsync(u => u.FirstName == "John");

            // Assert
            result.Should().NotBeNull();
            result.Username.Should().Be("user1"); // Should be the first one added
        }

        [Test]
        public async Task FirstOrDefaultAsync_NoMatches_ReturnsNull()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("user1", "John", "Doe");
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.FirstOrDefaultAsync(u => u.FirstName == "NonExistent");

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region AnyAsync Tests

        [Test]
        public async Task AnyAsync_MatchingPredicate_ReturnsTrue()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("user1", "John", "Doe");
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.AnyAsync(u => u.FirstName == "John");

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task AnyAsync_NoMatches_ReturnsFalse()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("user1", "John", "Doe");
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.AnyAsync(u => u.FirstName == "NonExistent");

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region CountAsync Tests

        [Test]
        public async Task CountAsync_MatchingPredicate_ReturnsCorrectCount()
        {
            // Arrange
            var users = new List<User>
            {
                TestDataFactory.CreateTestUser("user1", "John", "Doe"),
                TestDataFactory.CreateTestUser("user2", "Jane", "Smith"),
                TestDataFactory.CreateTestUser("user3", "John", "Johnson")
            };

            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.CountAsync(u => u.FirstName == "John");

            // Assert
            result.Should().Be(2);
        }

        [Test]
        public async Task CountAsync_NoMatches_ReturnsZero()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser("user1", "John", "Doe");
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.CountAsync(u => u.FirstName == "NonExistent");

            // Assert
            result.Should().Be(0);
        }

        #endregion

        #region GetQueryable Tests

        [Test]
        public async Task GetQueryable_ReturnsQueryableUsers()
        {
            // Arrange
            var users = new List<User>
            {
                TestDataFactory.CreateTestUser("user1", "John", "Doe"),
                TestDataFactory.CreateTestUser("user2", "Jane", "Smith")
            };

            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();

            // Act
            var queryable = _repository.GetQueryable();
            var result = await queryable.Where(u => u.FirstName == "John").ToListAsync();

            // Assert
            result.Should().HaveCount(1);
            result.First().Username.Should().Be("user1");
        }

        #endregion

        #region AddAsync Tests

        [Test]
        public async Task AddAsync_ValidUser_AddsToDatabase()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();

            // Act
            await _repository.AddAsync(user);
            await _repository.SaveChangesAsync();

            // Assert
            var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
            savedUser.Should().NotBeNull();
            savedUser.Should().BeEquivalentTo(user);
        }

        [Test]
        public async Task AddRangeAsync_ValidUsers_AddsAllToDatabase()
        {
            // Arrange
            var users = new List<User>
            {
                TestDataFactory.CreateTestUser("user1", "First1", "Last1"),
                TestDataFactory.CreateTestUser("user2", "First2", "Last2")
            };

            // Act
            await _repository.AddRangeAsync(users);
            await _repository.SaveChangesAsync();

            // Assert
            var savedUsers = await _context.Users.ToListAsync();
            savedUsers.Should().HaveCount(2);
            savedUsers.Should().BeEquivalentTo(users);
        }

        #endregion

        #region Update Tests

        [Test]
        public async Task Update_ExistingUser_UpdatesInDatabase()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            user.FirstName = "Updated";
            user.LastName = "Name";

            // Act
            _repository.Update(user);
            await _repository.SaveChangesAsync();

            // Assert
            var updatedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            updatedUser.Should().NotBeNull();
            updatedUser.FirstName.Should().Be("Updated");
            updatedUser.LastName.Should().Be("Name");
        }

        [Test]
        public async Task UpdateRange_ExistingUsers_UpdatesAllInDatabase()
        {
            // Arrange
            var users = new List<User>
            {
                TestDataFactory.CreateTestUser("user1", "First1", "Last1"),
                TestDataFactory.CreateTestUser("user2", "First2", "Last2")
            };

            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();

            users[0].FirstName = "Updated1";
            users[1].FirstName = "Updated2";

            // Act
            _repository.UpdateRange(users);
            await _repository.SaveChangesAsync();

            // Assert
            var updatedUsers = await _context.Users.ToListAsync();
            updatedUsers.Should().HaveCount(2);
            updatedUsers.Should().Contain(u => u.FirstName == "Updated1");
            updatedUsers.Should().Contain(u => u.FirstName == "Updated2");
        }

        #endregion

        #region Delete Tests

        [Test]
        public async Task Delete_ExistingUser_RemovesFromDatabase()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            _repository.Delete(user);
            await _repository.SaveChangesAsync();

            // Assert
            var deletedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            deletedUser.Should().BeNull();
        }

        [Test]
        public async Task DeleteRange_ExistingUsers_RemovesAllFromDatabase()
        {
            // Arrange
            var users = new List<User>
            {
                TestDataFactory.CreateTestUser("user1", "First1", "Last1"),
                TestDataFactory.CreateTestUser("user2", "First2", "Last2")
            };

            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();
            
            // Detach the entities to avoid tracking conflicts
            foreach (var user in users)
            {
                _context.Entry(user).State = EntityState.Detached;
            }

            // Act
            _repository.DeleteRange(users);
            await _repository.SaveChangesAsync();

            // Assert
            var remainingUsers = await _context.Users.ToListAsync();
            remainingUsers.Should().BeEmpty();
        }

        #endregion

        #region Custom Methods Tests

        [Test]
        public async Task GetByUsernameAsync_ExistingUser_ReturnsUserWithRefreshTokens()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            user.RefreshTokens = TestDataFactory.CreateTestRefreshTokens();
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByUsernameAsync(user.Username);

            // Assert
            result.Should().NotBeNull();
            result.Username.Should().Be(user.Username);
            result.RefreshTokens.Should().NotBeNull();
            result.RefreshTokens.Should().HaveCount(2);
        }

        [Test]
        public async Task GetByUsernameAsync_NonExistentUser_ReturnsNull()
        {
            // Act
            var result = await _repository.GetByUsernameAsync("nonexistent");

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task GetByRefreshTokenAsync_ExistingToken_ReturnsUserWithRefreshTokens()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            var refreshTokens = TestDataFactory.CreateTestRefreshTokens();
            user.RefreshTokens = refreshTokens;
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByRefreshTokenAsync(refreshTokens.First().Token);

            // Assert
            result.Should().NotBeNull();
            result.Username.Should().Be(user.Username);
            result.RefreshTokens.Should().NotBeNull();
            result.RefreshTokens.Should().HaveCount(2);
        }

        [Test]
        public async Task GetByRefreshTokenAsync_NonExistentToken_ReturnsNull()
        {
            // Act
            var result = await _repository.GetByRefreshTokenAsync("nonexistent-token");

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task GetAllWithRefreshTokensAsync_ReturnsAllUsersWithTokens()
        {
            // Arrange
            var users = new List<User>
            {
                TestDataFactory.CreateTestUser("user1", "First1", "Last1"),
                TestDataFactory.CreateTestUser("user2", "First2", "Last2")
            };

            users[0].RefreshTokens = TestDataFactory.CreateTestRefreshTokens();
            users[1].RefreshTokens = new List<OpenAlprWebhookProcessor.Features.Users.RefreshToken>();

            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllWithRefreshTokensAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(u => u.Username == "user1");
            result.Should().Contain(u => u.Username == "user2");
            result.First(u => u.Username == "user1").RefreshTokens.Should().HaveCount(2);
            result.First(u => u.Username == "user2").RefreshTokens.Should().BeEmpty();
        }

        [Test]
        public async Task GetByIdWithRefreshTokensAsync_ExistingUser_ReturnsUserWithTokens()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            user.RefreshTokens = TestDataFactory.CreateTestRefreshTokens();
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdWithRefreshTokensAsync(user.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
            result.RefreshTokens.Should().NotBeNull();
            result.RefreshTokens.Should().HaveCount(2);
        }

        [Test]
        public async Task GetByIdWithRefreshTokensAsync_NonExistentUser_ReturnsNull()
        {
            // Act
            var result = await _repository.GetByIdWithRefreshTokensAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task UsernameExistsAsync_ExistingUsername_ReturnsTrue()
        {
            // Arrange
            var user = TestDataFactory.CreateTestUser();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.UsernameExistsAsync(user.Username);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task UsernameExistsAsync_NonExistentUsername_ReturnsFalse()
        {
            // Act
            var result = await _repository.UsernameExistsAsync("nonexistent");

            // Assert
            result.Should().BeFalse();
        }

        #endregion
    }
} 