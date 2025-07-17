using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetAllUsers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tests.TestHelpers;

namespace Tests.Features.Users.Queries
{
    [TestFixture]
    public class GetAllUsersQueryHandlerTests : TestBase
    {
        private GetAllUsersQueryHandler _handler;
        private IUsersUnitOfWork _mockUsersUnitOfWork;
        private IUserRepository _mockUserRepository;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _mockUsersUnitOfWork = Substitute.For<IUsersUnitOfWork>();
            _mockUserRepository = Substitute.For<IUserRepository>();
            
            _mockUsersUnitOfWork.Users.Returns(_mockUserRepository);
            
            _handler = new GetAllUsersQueryHandler(_mockUsersUnitOfWork);
        }

        [TearDown]
        public override void TearDown()
        {
            _mockUsersUnitOfWork.Dispose();
        }

        [Test]
        public async Task Handle_ValidQuery_ReturnsAllUsers()
        {
            // Arrange
            var users = new List<User>
            {
                TestDataFactory.CreateTestUser("user1", "First1", "Last1"),
                TestDataFactory.CreateTestUser("user2", "First2", "Last2"),
                TestDataFactory.CreateTestUser("user3", "First3", "Last3")
            };
            
            var query = new GetAllUsersQuery();
            
            _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(users);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(users);
        }

        [Test]
        public async Task Handle_EmptyUserList_ReturnsEmptyList()
        {
            // Arrange
            var emptyUsers = new List<User>();
            var query = new GetAllUsersQuery();
            
            _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(emptyUsers);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task Handle_ValidQuery_CallsCorrectRepositoryMethod()
        {
            // Arrange
            var users = new List<User> { TestDataFactory.CreateTestUser() };
            var query = new GetAllUsersQuery();
            
            _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(users);

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            await _mockUserRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_ValidQuery_ReturnsListNotEnumerable()
        {
            // Arrange
            var query = new GetAllUsersQuery();
            var users = new List<User> { TestDataFactory.CreateTestUser() };
            
            _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(users);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeOfType<List<User>>();
            result.Should().BeAssignableTo<IEnumerable<User>>();
        }

        [Test]
        public async Task Handle_ValidQuery_PreservesUserOrder()
        {
            // Arrange
            var user1 = TestDataFactory.CreateTestUser("user1", "First1", "Last1");
            var user2 = TestDataFactory.CreateTestUser("user2", "First2", "Last2");
            var user3 = TestDataFactory.CreateTestUser("user3", "First3", "Last3");
            
            var users = new List<User> { user1, user2, user3 };
            var query = new GetAllUsersQuery();
            
            _mockUserRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(users);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().ContainInOrder(user1, user2, user3);
        }

        [Test]
        public async Task Handle_ValidQuery_PassesCancellationToken()
        {
            // Arrange
            var users = new List<User> { TestDataFactory.CreateTestUser() };
            var query = new GetAllUsersQuery();
            var cancellationToken = new CancellationToken();
            
            _mockUserRepository.GetAllAsync(cancellationToken)
                .Returns(users);

            // Act
            await _handler.Handle(query, cancellationToken);

            // Assert
            await _mockUserRepository.Received(1).GetAllAsync(cancellationToken);
        }
    }
} 