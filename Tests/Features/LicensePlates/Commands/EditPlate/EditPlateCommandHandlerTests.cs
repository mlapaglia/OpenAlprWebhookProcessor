using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.LicensePlates.Commands.EditPlate;
using Tests.TestHelpers;

namespace Tests.Features.LicensePlates.Commands.EditPlate
{
    [TestFixture]
    public class EditPlateCommandHandlerTests : TestBase
    {
        private EditPlateCommandHandler _handler;
        private IUnitOfWork _unitOfWork;
        private IPlateGroupRepository _plateGroupRepository;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _plateGroupRepository = Substitute.For<IPlateGroupRepository>();
            
            _unitOfWork.PlateGroups.Returns(_plateGroupRepository);
            _handler = new EditPlateCommandHandler(_unitOfWork);
        }

        [TearDown]
        public override void TearDown()
        {
            _unitOfWork.Dispose();
        }

        [Test]
        public async Task Handle_ValidPlateId_UpdatesPlateSuccessfully()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var command = new EditPlateCommand
            {
                Id = plateId,
                PlateNumber = "XYZ789",
                Notes = "Updated notes"
            };
            
            var plateGroup = TestDataFactory.CreateTestPlateGroup();
            plateGroup.Id = plateId;
            plateGroup.BestNumber = "ABC123";

            var cancellationToken = GetCancellationToken();

            _plateGroupRepository.GetByIdAsync(plateId, cancellationToken)
                .Returns(plateGroup);

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            plateGroup.BestNumber.Should().Be("XYZ789");
            _plateGroupRepository.Received(1).Update(plateGroup);
            await _unitOfWork.Received(1).SaveChangesAsync(cancellationToken);
        }

        [Test]
        public async Task Handle_PlateNotFound_ThrowsArgumentException()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var command = new EditPlateCommand
            {
                Id = plateId,
                PlateNumber = "XYZ789",
                Notes = "Updated notes"
            };

            var cancellationToken = GetCancellationToken();

            _plateGroupRepository.GetByIdAsync(plateId, cancellationToken)
                .Returns((PlateGroup)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => 
                _handler.Handle(command, cancellationToken));

            exception.Message.Should().Be($"Plate with ID {plateId} not found");
            
            _plateGroupRepository.DidNotReceive().Update(Arg.Any<PlateGroup>());
            await _unitOfWork.DidNotReceive().SaveChangesAsync(cancellationToken);
        }

        [Test]
        public async Task Handle_EmptyPlateNumber_UpdatesPlateWithEmptyString()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var command = new EditPlateCommand
            {
                Id = plateId,
                PlateNumber = "",
                Notes = "Updated notes"
            };
            
            var plateGroup = TestDataFactory.CreateTestPlateGroup();
            plateGroup.Id = plateId;
            plateGroup.BestNumber = "ABC123";

            var cancellationToken = GetCancellationToken();

            _plateGroupRepository.GetByIdAsync(plateId, cancellationToken)
                .Returns(plateGroup);

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            plateGroup.BestNumber.Should().Be("");
            _plateGroupRepository.Received(1).Update(plateGroup);
            await _unitOfWork.Received(1).SaveChangesAsync(cancellationToken);
        }

        [Test]
        public async Task Handle_NullNotes_UpdatesPlateWithNullNotes()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var command = new EditPlateCommand
            {
                Id = plateId,
                PlateNumber = "XYZ789",
                Notes = null
            };
            
            var plateGroup = TestDataFactory.CreateTestPlateGroup();
            plateGroup.Id = plateId;
            plateGroup.BestNumber = "ABC123";

            var cancellationToken = GetCancellationToken();

            _plateGroupRepository.GetByIdAsync(plateId, cancellationToken)
                .Returns(plateGroup);

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            plateGroup.BestNumber.Should().Be("XYZ789");
            _plateGroupRepository.Received(1).Update(plateGroup);
            await _unitOfWork.Received(1).SaveChangesAsync(cancellationToken);
        }

        [Test]
        public async Task Handle_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var command = new EditPlateCommand
            {
                Id = plateId,
                PlateNumber = "XYZ789",
                Notes = "Updated notes"
            };
            
            var plateGroup = TestDataFactory.CreateTestPlateGroup();
            plateGroup.Id = plateId;

            var cancellationToken = GetCancellationToken();
            var repositoryException = new InvalidOperationException("Database connection failed");

            _plateGroupRepository.GetByIdAsync(plateId, cancellationToken)
                .Returns(plateGroup);

            _plateGroupRepository.When(x => x.Update(plateGroup))
                .Do(x => throw repositoryException);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(() => 
                _handler.Handle(command, cancellationToken));

            exception.Should().Be(repositoryException);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(cancellationToken);
        }

        [Test]
        public void Handle_SaveChangesThrowsException_PropagatesException()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var command = new EditPlateCommand
            {
                Id = plateId,
                PlateNumber = "XYZ789",
                Notes = "Updated notes"
            };
            
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
            _plateGroupRepository.Received(1).Update(plateGroup);
        }

        [Test]
        public async Task Handle_EmptyGuid_ThrowsArgumentException()
        {
            // Arrange
            var command = new EditPlateCommand
            {
                Id = Guid.Empty,
                PlateNumber = "XYZ789",
                Notes = "Updated notes"
            };

            var cancellationToken = GetCancellationToken();

            _plateGroupRepository.GetByIdAsync(Guid.Empty, cancellationToken)
                .Returns((PlateGroup)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => 
                _handler.Handle(command, cancellationToken));

            exception.Message.Should().Be($"Plate with ID {Guid.Empty} not found");
            
            _plateGroupRepository.DidNotReceive().Update(Arg.Any<PlateGroup>());
            await _unitOfWork.DidNotReceive().SaveChangesAsync(cancellationToken);
        }
    }
} 