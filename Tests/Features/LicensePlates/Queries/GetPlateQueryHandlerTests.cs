using FluentAssertions;
using NSubstitute;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetPlate;

namespace Tests.Features.LicensePlates.Queries
{
    [TestFixture]
    public class GetPlateQueryHandlerTests
    {
        private IUnitOfWork _unitOfWork;
        private GetPlateQueryHandler _handler;
        private IPlateGroupRepository _plateGroupRepository;
        private IRepository<Alert> _alertRepository;
        private IRepository<Ignore> _ignoreRepository;

        [SetUp]
        public void Setup()
        {
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _plateGroupRepository = Substitute.For<IPlateGroupRepository>();
            _alertRepository = Substitute.For<IRepository<Alert>>();
            _ignoreRepository = Substitute.For<IRepository<Ignore>>();

            _unitOfWork.PlateGroups.Returns(_plateGroupRepository);
            _unitOfWork.Alerts.Returns(_alertRepository);
            _unitOfWork.Ignores.Returns(_ignoreRepository);

            _handler = new GetPlateQueryHandler(_unitOfWork);
        }

        [TearDown]
        public void TearDown()
        {
            _unitOfWork.Dispose();
        }

        [Test]
        public async Task Handle_ExistingPlate_ReturnsPlate()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var query = new GetPlateQuery(plateId);
            var plateGroup = new PlateGroup
            {
                Id = plateId,
                BestNumber = "ABC123",
                VehicleColor = "Red",
                VehicleMakeModel = "Toyota Camry",
                ReceivedOnEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                PossibleNumbers = new List<PlateGroupPossibleNumbers>()
            };

            _plateGroupRepository.GetByIdWithDetailsAsync(plateId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(plateGroup)!);

            _alertRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult((IEnumerable<Alert>)new List<Alert>()));

            _ignoreRepository.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult((IEnumerable<Ignore>)new List<Ignore>()));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(plateId);
            result.PlateNumber.Should().Be("ABC123");
        }

        [Test]
        public async Task Handle_NonExistingPlate_ReturnsNull()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var query = new GetPlateQuery(plateId);

            _plateGroupRepository.GetByIdWithDetailsAsync(plateId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<PlateGroup?>(null));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }
    }
} 