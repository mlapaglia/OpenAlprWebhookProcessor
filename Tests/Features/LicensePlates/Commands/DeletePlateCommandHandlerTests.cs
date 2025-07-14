using NSubstitute;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.LicensePlates.Commands.DeletePlate;

namespace Tests.Features.LicensePlates.Commands
{
    [TestFixture]
    public class DeletePlateCommandHandlerTests
    {
        private IUnitOfWork _unitOfWork;
        private DeletePlateCommandHandler _handler;
        private IPlateGroupRepository _plateGroupRepository;

        [SetUp]
        public void Setup()
        {
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _plateGroupRepository = Substitute.For<IPlateGroupRepository>();
            _unitOfWork.PlateGroups.Returns(_plateGroupRepository);
            _handler = new DeletePlateCommandHandler(_unitOfWork);
        }

        [TearDown]
        public void Teardown()
        {
            _unitOfWork.Dispose();
        }

        [Test]
        public async Task Handle_ExistingPlate_DeletesPlateSuccessfully()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var command = new DeletePlateCommand(plateId);
            var existingPlate = new PlateGroup
            {
                Id = plateId,
                BestNumber = "ABC123"
            };

            _plateGroupRepository.GetByIdAsync(plateId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(existingPlate)!);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _plateGroupRepository.Received(1).Delete(existingPlate);
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
} 