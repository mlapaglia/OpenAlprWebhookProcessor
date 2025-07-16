using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.LicensePlates.Commands.DeletePlate;
using Tests.TestHelpers;

namespace Tests.Features.LicensePlates.Commands.DeletePlate
{
    [TestFixture]
    public class DeletePlateCommandHandlerTests : TestBase
    {
        private DeletePlateCommandHandler _handler;
        private IUnitOfWork _unitOfWork;
        private IPlateGroupRepository _plateGroupRepository;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _plateGroupRepository = Substitute.For<IPlateGroupRepository>();
            
            _unitOfWork.PlateGroups.Returns(_plateGroupRepository);
            _handler = new DeletePlateCommandHandler(_unitOfWork);
        }

        [TearDown]
        public override void TearDown()
        {
            _unitOfWork.Dispose();
        }

        [Test]
        public async Task Handle_ValidPlateId_DeletesPlateSuccessfully()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var command = new DeletePlateCommand(plateId);
            var plateGroup = TestDataFactory.CreateTestPlateGroup();
            plateGroup.Id = plateId;

            var cancellationToken = GetCancellationToken();

            _plateGroupRepository.GetByIdAsync(plateId, cancellationToken)
                .Returns(plateGroup);

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            _plateGroupRepository.Received(1).Delete(plateGroup);
            await _unitOfWork.Received(1).SaveChangesAsync(cancellationToken);
        }

        [Test]
        public async Task Handle_PlateNotFound_ThrowsArgumentException()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var command = new DeletePlateCommand(plateId);
            var cancellationToken = GetCancellationToken();

            _plateGroupRepository.GetByIdAsync(plateId, cancellationToken)
                .Returns((PlateGroup)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => 
                _handler.Handle(command, cancellationToken));

            exception.Message.Should().Be($"Plate with ID {plateId} not found");
            
            _plateGroupRepository.DidNotReceive().Delete(Arg.Any<PlateGroup>());
            await _unitOfWork.DidNotReceive().SaveChangesAsync(cancellationToken);
        }

        [Test]
        public async Task Handle_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var command = new DeletePlateCommand(plateId);
            var plateGroup = TestDataFactory.CreateTestPlateGroup();
            plateGroup.Id = plateId;

            var cancellationToken = GetCancellationToken();
            var repositoryException = new InvalidOperationException("Database connection failed");

            _plateGroupRepository.GetByIdAsync(plateId, cancellationToken)
                .Returns(plateGroup);

            _plateGroupRepository.When(x => x.Delete(plateGroup))
                .Do(x => throw repositoryException);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(() => 
                _handler.Handle(command, cancellationToken));

            exception.Should().Be(repositoryException);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(cancellationToken);
        }

        [Test]
        public async Task Handle_SaveChangesThrowsException_PropagatesException()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var command = new DeletePlateCommand(plateId);
            var plateGroup = TestDataFactory.CreateTestPlateGroup();
            plateGroup.Id = plateId;

            var cancellationToken = GetCancellationToken();
            var saveException = new InvalidOperationException("Save operation failed");

            _plateGroupRepository.GetByIdAsync(plateId, cancellationToken)
                .Returns(plateGroup);

            _unitOfWork.SaveChangesAsync(cancellationToken)
                .ThrowsAsync(saveException);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(() => 
                _handler.Handle(command, cancellationToken));

            exception.Should().Be(saveException);
            _plateGroupRepository.Received(1).Delete(plateGroup);
        }

        [Test]
        public async Task Handle_EmptyGuid_ThrowsArgumentException()
        {
            // Arrange
            var command = new DeletePlateCommand(Guid.Empty);
            var cancellationToken = GetCancellationToken();

            _plateGroupRepository.GetByIdAsync(Guid.Empty, cancellationToken)
                .Returns((PlateGroup)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => 
                _handler.Handle(command, cancellationToken));

            exception.Message.Should().Be($"Plate with ID {Guid.Empty} not found");
            
            _plateGroupRepository.DidNotReceive().Delete(Arg.Any<PlateGroup>());
            await _unitOfWork.DidNotReceive().SaveChangesAsync(cancellationToken);
        }
    }
} 