using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.LicensePlates.Commands.EnrichPlate;
using Tests.TestHelpers;

namespace Tests.Features.LicensePlates.Commands.EnrichPlate
{
    [TestFixture]
    public class EnrichPlateCommandHandlerTests : TestBase
    {
        private EnrichPlateCommandHandler _handler;
        private ILicensePlateEnricherClient _licensePlateEnricherClient;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _licensePlateEnricherClient = Substitute.For<ILicensePlateEnricherClient>();
            _handler = new EnrichPlateCommandHandler(UnitOfWork, _licensePlateEnricherClient);
        }

        [Test]
        public async Task Handle_ValidUsPlateNotEnriched_EnrichesPlateSuccessfully()
        {
            // Arrange
            var plateGroup = TestDataFactory.CreateTestPlateGroup();
            plateGroup.BestNumber = "ABC123";
            plateGroup.VehicleRegion = "us-ca";
            plateGroup.IsEnriched = false;
            
            await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            await UnitOfWork.SaveChangesAsync();

            var enrichedData = new EnrichedLicensePlate
            {
                Make = "Toyota",
                Style = "Sedan"
            };

            var command = new EnrichPlateCommand(plateGroup.Id);
            var cancellationToken = GetCancellationToken();

            _licensePlateEnricherClient.GetLicenseInformationAsync(
                "ABC123", 
                "CA", 
                cancellationToken)
                .Returns(enrichedData);

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            var updatedPlate = await UnitOfWork.PlateGroups.GetByIdWithDetailsAsync(plateGroup.Id, cancellationToken);
            updatedPlate.Should().NotBeNull();
            updatedPlate.IsEnriched.Should().BeTrue();
            updatedPlate.VehicleType.Should().Be("Sedan");
            updatedPlate.VehicleMake.Should().Be("Toyota");
        }

        [Test]
        public void Handle_PlateNotFound_ThrowsArgumentException()
        {
            // Arrange
            var nonExistentPlateId = Guid.NewGuid();
            var command = new EnrichPlateCommand(nonExistentPlateId);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => 
                _handler.Handle(command, cancellationToken));

            exception.Message.Should().Be("Plate Id not found.");
        }

        [Test]
        public async Task Handle_PlateAlreadyEnriched_ThrowsArgumentException()
        {
            // Arrange
            var plateGroup = TestDataFactory.CreateTestPlateGroup();
            plateGroup.BestNumber = "ABC123";
            plateGroup.VehicleRegion = "us-ca";
            plateGroup.IsEnriched = true;
            
            await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            await UnitOfWork.SaveChangesAsync();

            var command = new EnrichPlateCommand(plateGroup.Id);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => 
                _handler.Handle(command, cancellationToken));

            exception.Message.Should().Be("Plate has already been enriched.");
        }

        [Test]
        public async Task Handle_NonUsRegion_ThrowsArgumentException()
        {
            // Arrange
            var plateGroup = TestDataFactory.CreateTestPlateGroup();
            plateGroup.BestNumber = "ABC123";
            plateGroup.VehicleRegion = "uk";
            plateGroup.IsEnriched = false;
            
            await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            await UnitOfWork.SaveChangesAsync();

            var command = new EnrichPlateCommand(plateGroup.Id);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => 
                _handler.Handle(command, cancellationToken));

            exception.Message.Should().Be("Plate must be United States region.");
        }

        [Test]
        public async Task Handle_EnricherReturnsNull_ThrowsInvalidOperationException()
        {
            // Arrange
            var plateGroup = TestDataFactory.CreateTestPlateGroup();
            plateGroup.BestNumber = "ABC123";
            plateGroup.VehicleRegion = "us-ca";
            plateGroup.IsEnriched = false;
            
            await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            await UnitOfWork.SaveChangesAsync();

            var command = new EnrichPlateCommand(plateGroup.Id);
            var cancellationToken = GetCancellationToken();

            _licensePlateEnricherClient.GetLicenseInformationAsync(
                "ABC123", 
                "CA", 
                cancellationToken)
                .Returns((EnrichedLicensePlate)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(() => 
                _handler.Handle(command, cancellationToken));

            exception.Message.Should().Be("Failed to enrich plate data.");
        }

        [Test]
        public async Task Handle_EnricherThrowsException_PropagatesException()
        {
            // Arrange
            var plateGroup = TestDataFactory.CreateTestPlateGroup();
            plateGroup.BestNumber = "ABC123";
            plateGroup.VehicleRegion = "us-ca";
            plateGroup.IsEnriched = false;
            
            await UnitOfWork.PlateGroups.AddAsync(plateGroup);
            await UnitOfWork.SaveChangesAsync();

            var command = new EnrichPlateCommand(plateGroup.Id);
            var cancellationToken = GetCancellationToken();

            _licensePlateEnricherClient.GetLicenseInformationAsync(
                "ABC123", 
                "CA", 
                cancellationToken)
                .ThrowsAsync(new Exception("Enricher service error"));

            // Act & Assert
            var exception = Assert.ThrowsAsync<Exception>(() => 
                _handler.Handle(command, cancellationToken));
            
            exception.Message.Should().Be("Enricher service error");
        }
    }
} 