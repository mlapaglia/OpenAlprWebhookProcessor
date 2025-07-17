using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Commands.DeleteUser;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users.Data.Repositories;
using Tests.TestHelpers;

namespace Tests.Features.Users.Commands
{
    [TestFixture]
    public class DeleteUserCommandHandlerTests : TestBase
    {
        private DeleteUserCommandHandler _handler;
        private IUsersUnitOfWork _mockUsersUnitOfWork;
        private IUserRepository _mockUserRepository;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _mockUsersUnitOfWork = Substitute.For<IUsersUnitOfWork>();
            _mockUserRepository = Substitute.For<IUserRepository>();
            
            _mockUsersUnitOfWork.Users.Returns(_mockUserRepository);
            
            _handler = new DeleteUserCommandHandler(_mockUsersUnitOfWork);
        }

        [TearDown]
        public override void TearDown()
        {
            _mockUsersUnitOfWork.Dispose();
        }

        [Test]
        public async Task Handle_ExistingUser_DeletesUserSuccessfully()
        {
            // Arrange
            var existingUser = TestDataFactory.CreateTestUser();
            var command = new DeleteUserCommand(existingUser.Id);
            
            _mockUserRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
                .Returns(existingUser);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockUserRepository.Received(1).Delete(existingUser);
            await _mockUsersUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_NonExistentUser_DoesNotThrowError()
        {
            // Arrange
            var command = new DeleteUserCommand(999);
            
            _mockUserRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
                .Returns((User)null);

            // Act & Assert
            await _handler.Handle(command, CancellationToken.None);
            
            _mockUserRepository.DidNotReceive().Delete(Arg.Any<User>());
            await _mockUsersUnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_ValidId_CallsCorrectRepositoryMethods()
        {
            // Arrange
            var existingUser = TestDataFactory.CreateTestUser();
            var command = new DeleteUserCommand(existingUser.Id);
            
            _mockUserRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
                .Returns(existingUser);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            await _mockUserRepository.Received(1).GetByIdAsync(command.Id, Arg.Any<CancellationToken>());
            _mockUserRepository.Received(1).Delete(existingUser);
            await _mockUsersUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_ZeroId_AttemptsToFindUser()
        {
            // Arrange
            var command = new DeleteUserCommand(0);
            
            _mockUserRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
                .Returns((User)null);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            await _mockUserRepository.Received(1).GetByIdAsync(0, Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Handle_NegativeId_AttemptsToFindUser()
        {
            // Arrange
            var command = new DeleteUserCommand(-1);
            
            _mockUserRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
                .Returns((User)null);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            await _mockUserRepository.Received(1).GetByIdAsync(-1, Arg.Any<CancellationToken>());
        }
    }
} 