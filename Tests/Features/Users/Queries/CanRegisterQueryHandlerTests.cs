using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Users.Queries.CanRegister;
using Tests.TestHelpers;

namespace Tests.Features.Users.Queries
{
    [TestFixture]
    public class CanRegisterQueryHandlerTests : TestBase
    {
        private CanRegisterQueryHandler _handler;
        private IUsersUnitOfWork _mockUsersUnitOfWork;
        private IUserRepository _mockUserRepository;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _mockUsersUnitOfWork = Substitute.For<IUsersUnitOfWork>();
            _mockUserRepository = Substitute.For<IUserRepository>();
            
            _mockUsersUnitOfWork.Users.Returns(_mockUserRepository);
            
            _handler = new CanRegisterQueryHandler(_mockUsersUnitOfWork);
        }

        [TearDown]
        public override void TearDown()
        {
            _mockUsersUnitOfWork.Dispose();
        }

        [Test]
        public async Task Handle_NoExistingUsers_ReturnsTrue()
        {
            // Arrange
            var emptyUsers = new List<User>();
            var query = new CanRegisterQuery();
            
            _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(emptyUsers);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task Handle_ExistingUsers_ReturnsFalse()
        {
            // Arrange
            var users = new List<User>
            {
                TestDataFactory.CreateTestUser("user1", "First1", "Last1"),
                TestDataFactory.CreateTestUser("user2", "First2", "Last2")
            };
            var query = new CanRegisterQuery();
            
            _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(users);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task Handle_SingleUser_ReturnsFalse()
        {
            // Arrange
            var users = new List<User>
            {
                TestDataFactory.CreateTestUser("user1", "First1", "Last1")
            };
            var query = new CanRegisterQuery();
            
            _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(users);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task Handle_ValidQuery_CallsCorrectRepositoryMethod()
        {
            // Arrange
            var users = new List<User> { TestDataFactory.CreateTestUser() };
            var query = new CanRegisterQuery();
            
            _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(users);

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            await _mockUserRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_ValidQuery_PassesCancellationToken()
        {
            // Arrange
            var users = new List<User>();
            var query = new CanRegisterQuery();
            var cancellationToken = new CancellationToken();
            
            _mockUserRepository.GetAllAsync(cancellationToken)
                .Returns(users);

            // Act
            await _handler.Handle(query, cancellationToken);

            // Assert
            await _mockUserRepository.Received(1).GetAllAsync(cancellationToken);
        }

        [Test]
        public async Task Handle_MultipleUsers_ReturnsFalse()
        {
            // Arrange
            var users = new List<User>
            {
                TestDataFactory.CreateTestUser("user1", "First1", "Last1"),
                TestDataFactory.CreateTestUser("user2", "First2", "Last2"),
                TestDataFactory.CreateTestUser("user3", "First3", "Last3"),
                TestDataFactory.CreateTestUser("user4", "First4", "Last4")
            };
            var query = new CanRegisterQuery();
            
            _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(users);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task Handle_UsesEnumerableCount_WorksCorrectly()
        {
            // Arrange
            var users = new List<User>
            {
                TestDataFactory.CreateTestUser("user1", "First1", "Last1"),
                TestDataFactory.CreateTestUser("user2", "First2", "Last2")
            };
            var query = new CanRegisterQuery();
            
            // Return as IEnumerable to test that .Count() works
            _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(users.AsEnumerable());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }
    }
} 