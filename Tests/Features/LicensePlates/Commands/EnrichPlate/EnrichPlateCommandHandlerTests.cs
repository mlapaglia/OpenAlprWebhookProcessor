using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Data.Repositories;
using OpenAlprWebhookProcessor.Features.LicensePlates.Commands.EnrichPlate;
using Tests.TestHelpers;

namespace Tests.Features.LicensePlates.Commands.EnrichPlate
{
    [TestFixture]
    public class EnrichPlateCommandHandlerTests : TestBase
    {
        private EnrichPlateCommandHandler _handler;
        private IUnitOfWork _unitOfWork;
        private ILicensePlateEnricherClient _licensePlateEnricherClient;
        private IPlateGroupRepository _plateGroupRepository;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _licensePlateEnricherClient = Substitute.For<ILicensePlateEnricherClient>();
            _plateGroupRepository = Substitute.For<IPlateGroupRepository>();
            
            _unitOfWork.PlateGroups.Returns(_plateGroupRepository);
            _handler = new EnrichPlateCommandHandler(_unitOfWork, _licensePlateEnricherClient);
        }

        [TearDown]
        public override void TearDown()
        {
            _unitOfWork.Dispose();
        }

        [Test]
        public async Task Handle_ValidUsPlateNotEnriched_EnrichesPlateSuccessfully()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var command = new EnrichPlateCommand(plateId);
            var plateGroup = TestDataFactory.CreateTestPlateGroup();
            plateGroup.Id = plateId;
            plateGroup.BestNumber = "ABC123";
            plateGroup.VehicleRegion = "us-ca";
            plateGroup.IsEnriched = false;

            var enrichedData = new EnrichedLicensePlate
            {
                Make = "Toyota",
                Style = "Sedan"
            };

            var cancellationToken = GetCancellationToken();

            _plateGroupRepository.GetByIdWithDetailsAsync(plateId, cancellationToken)
                .Returns(plateGroup);

            _licensePlateEnricherClient.GetLicenseInformationAsync(
                "ABC123", 
                "CA", 
                cancellationToken)
                .Returns(enrichedData);

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            plateGroup.IsEnriched.Should().BeTrue();
            plateGroup.VehicleType.Should().Be("Sedan");
            plateGroup.VehicleMake.Should().Be("Toyota");

            _plateGroupRepository.Received(1).Update(plateGroup);
            await _unitOfWork.Received(1).SaveChangesAsync(cancellationToken);
        }

        [Test]
        public async Task Handle_PlateNotFound_ThrowsArgumentException()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var command = new EnrichPlateCommand(plateId);
            var cancellationToken = GetCancellationToken();

            _plateGroupRepository.GetByIdWithDetailsAsync(plateId, cancellationToken)
                .Returns((PlateGroup)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () => 
                await _handler.Handle(command, cancellationToken));

            exception.Message.Should().Be("Plate Id not found.");
            
            _plateGroupRepository.DidNotReceive().Update(Arg.Any<PlateGroup>());
            await _unitOfWork.DidNotReceive().SaveChangesAsync(cancellationToken);
        }

        [Test]
        public async Task Handle_NonUsPlate_ThrowsArgumentException()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var command = new EnrichPlateCommand(plateId);
            var plateGroup = TestDataFactory.CreateTestPlateGroup();
            plateGroup.Id = plateId;
            plateGroup.VehicleRegion = "ca-on"; // Canadian plate
            plateGroup.IsEnriched = false;

            var cancellationToken = GetCancellationToken();

            _plateGroupRepository.GetByIdWithDetailsAsync(plateId, cancellationToken)
                .Returns(plateGroup);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () => 
                await _handler.Handle(command, cancellationToken));

            exception.Message.Should().Be("Plate must be United States region.");
            
            _plateGroupRepository.DidNotReceive().Update(Arg.Any<PlateGroup>());
            await _unitOfWork.DidNotReceive().SaveChangesAsync(cancellationToken);
        }

        [Test]
        public async Task Handle_PlateAlreadyEnriched_ThrowsArgumentException()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var command = new EnrichPlateCommand(plateId);
            var plateGroup = TestDataFactory.CreateTestPlateGroup();
            plateGroup.Id = plateId;
            plateGroup.VehicleRegion = "us-ca";
            plateGroup.IsEnriched = true; // Already enriched

            var cancellationToken = GetCancellationToken();

            _plateGroupRepository.GetByIdWithDetailsAsync(plateId, cancellationToken)
                .Returns(plateGroup);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () => 
                await _handler.Handle(command, cancellationToken));

            exception.Message.Should().Be("Plate has already been enriched.");
            
            _plateGroupRepository.DidNotReceive().Update(Arg.Any<PlateGroup>());
            await _unitOfWork.DidNotReceive().SaveChangesAsync(cancellationToken);
        }

        [Test]
        public async Task Handle_EnricherReturnsNull_ThrowsInvalidOperationException()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var command = new EnrichPlateCommand(plateId);
            var plateGroup = TestDataFactory.CreateTestPlateGroup();
            plateGroup.Id = plateId;
            plateGroup.BestNumber = "ABC123";
            plateGroup.VehicleRegion = "us-ca";
            plateGroup.IsEnriched = false;

            var cancellationToken = GetCancellationToken();

            _plateGroupRepository.GetByIdWithDetailsAsync(plateId, cancellationToken)
                .Returns(plateGroup);

            _licensePlateEnricherClient.GetLicenseInformationAsync(
                "ABC123", 
                "CA", 
                cancellationToken)
                .Returns((EnrichedLicensePlate)null);

            // Act & Assert
                        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _handler.Handle(command, cancellationToken));

            exception.Message.Should().Be("Failed to enrich plate data.");
            
            _plateGroupRepository.DidNotReceive().Update(Arg.Any<PlateGroup>());
            await _unitOfWork.DidNotReceive().SaveChangesAsync(cancellationToken);
        }

        [Test]
        public async Task Handle_EnricherThrowsException_PropagatesException()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var command = new EnrichPlateCommand(plateId);
            var plateGroup = TestDataFactory.CreateTestPlateGroup();
            plateGroup.Id = plateId;
            plateGroup.BestNumber = "ABC123";
            plateGroup.VehicleRegion = "us-ca";
            plateGroup.IsEnriched = false;

            var cancellationToken = GetCancellationToken();
            var enricherException = new Exception("Enricher service unavailable");

            _plateGroupRepository.GetByIdWithDetailsAsync(plateId, cancellationToken)
                .Returns(plateGroup);

            _licensePlateEnricherClient.GetLicenseInformationAsync(
                "ABC123", 
                "CA", 
                cancellationToken)
                .ThrowsAsync(enricherException);

            // Act & Assert
                        var exception = Assert.ThrowsAsync<Exception>(async () =>
                await _handler.Handle(command, cancellationToken));

            exception.Should().Be(enricherException);
            
            _plateGroupRepository.DidNotReceive().Update(Arg.Any<PlateGroup>());
            await _unitOfWork.DidNotReceive().SaveChangesAsync(cancellationToken);
        }

        [Test]
        public async Task Handle_VariousUsRegions_ExtractsStateCorrectly()
        {
            // Arrange
            var testCases = new[]
            {
                new { Region = "us-ca", ExpectedState = "CA" },
                new { Region = "us-tx", ExpectedState = "TX" },
                new { Region = "us-ny", ExpectedState = "NY" }
            };

            foreach (var testCase in testCases)
            {
                var plateId = Guid.NewGuid();
                var command = new EnrichPlateCommand(plateId);
                var plateGroup = TestDataFactory.CreateTestPlateGroup();
                plateGroup.Id = plateId;
                plateGroup.BestNumber = "ABC123";
                plateGroup.VehicleRegion = testCase.Region;
                plateGroup.IsEnriched = false;

                var enrichedData = new EnrichedLicensePlate
                {
                    Make = "Toyota",
                    Style = "Sedan"
                };

                var cancellationToken = GetCancellationToken();

                _plateGroupRepository.GetByIdWithDetailsAsync(plateId, cancellationToken)
                    .Returns(plateGroup);

                _licensePlateEnricherClient.GetLicenseInformationAsync(
                    "ABC123", 
                    testCase.ExpectedState, 
                    cancellationToken)
                    .Returns(enrichedData);

                // Act
                await _handler.Handle(command, cancellationToken);

                // Assert
                await _licensePlateEnricherClient.Received(1).GetLicenseInformationAsync(
                    "ABC123", 
                    testCase.ExpectedState, 
                    cancellationToken);
            }
        }
    }
} 