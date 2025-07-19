using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users.Data.Repositories;
using OpenAlprWebhookProcessor.Features.Users.Queries.CanRegister;
using System;
using System.Linq.Expressions;
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
            
            _mockUserRepository.AnyAsync(x => true, Arg.Any<CancellationToken>())
                .Returns(false);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task Handle_ExistingUsers_ReturnsFalse()
        {
            // Arrange
            var query = new CanRegisterQuery();
            
            _mockUserRepository.AnyAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
                .Returns(true);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task Handle_SingleUser_ReturnsFalse()
        {
            // Arrange
            var query = new CanRegisterQuery();

            _mockUserRepository.AnyAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
                .Returns(true);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }
    }
} 