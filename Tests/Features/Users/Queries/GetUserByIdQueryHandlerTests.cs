using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetUserById;
using System.Threading;
using System.Threading.Tasks;
using Tests.TestHelpers;

namespace Tests.Features.Users.Queries
{
    [TestFixture]
    public class GetUserByIdQueryHandlerTests : TestBase
    {
        private GetUserByIdQueryHandler _handler;
        private IUsersUnitOfWork _mockUsersUnitOfWork;
        private IUserRepository _mockUserRepository;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _mockUsersUnitOfWork = Substitute.For<IUsersUnitOfWork>();
            _mockUserRepository = Substitute.For<IUserRepository>();
            
            _mockUsersUnitOfWork.Users.Returns(_mockUserRepository);
            
            _handler = new GetUserByIdQueryHandler(_mockUsersUnitOfWork);
        }

        [TearDown]
        public override void TearDown()
        {
            _mockUsersUnitOfWork.Dispose();
        }

        [Test]
        public async Task Handle_ExistingUser_ReturnsUser()
        {
            // Arrange
            var expectedUser = TestDataFactory.CreateTestUser();
            var query = new GetUserByIdQuery(expectedUser.Id);
            
            _mockUserRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>())
                .Returns(expectedUser);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedUser);
        }

        [Test]
        public async Task Handle_NonExistentUser_ReturnsNull()
        {
            // Arrange
            var query = new GetUserByIdQuery(999);
            
            _mockUserRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>())
                .Returns((User)null);

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
            var query = new GetUserByIdQuery(userId);
            
            _mockUserRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>())
                .Returns(TestDataFactory.CreateTestUser());

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            await _mockUserRepository.Received(1).GetByIdAsync(userId, Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_ZeroId_AttemptsToFindUser()
        {
            // Arrange
            var query = new GetUserByIdQuery(0);
            
            _mockUserRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>())
                .Returns((User)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();
            await _mockUserRepository.Received(1).GetByIdAsync(0, Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_NegativeId_AttemptsToFindUser()
        {
            // Arrange
            var query = new GetUserByIdQuery(-1);
            
            _mockUserRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>())
                .Returns((User)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();
            await _mockUserRepository.Received(1).GetByIdAsync(-1, Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_ValidQuery_PassesCancellationToken()
        {
            // Arrange
            var userId = 123;
            var query = new GetUserByIdQuery(userId);
            var cancellationToken = new CancellationToken();
            
            _mockUserRepository.GetByIdAsync(query.Id, cancellationToken)
                .Returns(TestDataFactory.CreateTestUser());

            // Act
            await _handler.Handle(query, cancellationToken);

            // Assert
            await _mockUserRepository.Received(1).GetByIdAsync(userId, cancellationToken);
        }

        [Test]
        public async Task Handle_ExistingUser_ReturnsCompleteUserObject()
        {
            // Arrange
            var expectedUser = TestDataFactory.CreateTestUser("testuser", "Test", "User");
            var query = new GetUserByIdQuery(expectedUser.Id);
            
            _mockUserRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>())
                .Returns(expectedUser);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(expectedUser.Id);
            result.Username.Should().Be(expectedUser.Username);
            result.FirstName.Should().Be(expectedUser.FirstName);
            result.LastName.Should().Be(expectedUser.LastName);
            result.PasswordHash.Should().BeEquivalentTo(expectedUser.PasswordHash);
            result.PasswordSalt.Should().BeEquivalentTo(expectedUser.PasswordSalt);
            result.RefreshTokens.Should().BeEquivalentTo(expectedUser.RefreshTokens);
        }
    }
} 