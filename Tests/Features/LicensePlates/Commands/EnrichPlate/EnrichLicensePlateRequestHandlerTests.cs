using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Features.LicensePlates.Commands.EnrichPlate;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tests.TestHelpers;

namespace Tests.Features.LicensePlates.Commands.EnrichPlate
{
    [TestFixture]
    public class EnrichLicensePlateRequestHandlerTests : TestBase
    {
        private EnrichLicensePlateRequestHandler _handler;
        private ILicensePlateEnricherClient _licensePlateEnricherClient;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            _licensePlateEnricherClient = Substitute.For<ILicensePlateEnricherClient>();
            _handler = new EnrichLicensePlateRequestHandler(
                _licensePlateEnricherClient,
                UnitOfWork);
        }

        [TearDown]
        public override void TearDown()
        {
            // Clear change tracker to prevent tracking conflicts between tests
            Context.ChangeTracker.Clear();
            base.TearDown();
        }

        [Test]
        public async Task HandleAsync_WithValidPlateId_EnrichesPlateSuccessfully()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var plateGroup = CreatePlateGroup(plateId, "us-ca", false);
            await SeedPlateGroupAsync(plateGroup);

            var enrichedPlate = CreateEnrichedPlate();
            _licensePlateEnricherClient.GetLicenseInformationAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
                .Returns(enrichedPlate);

            // Act
            await _handler.HandleAsync(plateId, CancellationToken.None);

            // Assert
            var updatedPlate = await GetPlateGroupByIdAsync(plateId);
            updatedPlate.VehicleType.Should().Be(enrichedPlate.Style);
            updatedPlate.VehicleMake.Should().Be(enrichedPlate.Make);
            updatedPlate.VehicleMakeModel.Should().Be($"{enrichedPlate.Make} {enrichedPlate.Model}");
            updatedPlate.VehicleYear.Should().Be(enrichedPlate.Year);
            updatedPlate.IsEnriched.Should().BeTrue();

            await _licensePlateEnricherClient.Received(1).GetLicenseInformationAsync(
                plateGroup.BestNumber,
                "CA",
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task HandleAsync_WithNonExistentPlateId_ThrowsArgumentException()
        {
            // Arrange
            var nonExistentPlateId = Guid.NewGuid();

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(
                () => _handler.HandleAsync(nonExistentPlateId, CancellationToken.None));

            exception.Message.Should().Be("Plate Id not found.");
        }

        [Test]
        public async Task HandleAsync_WithNonUsRegion_ThrowsArgumentException()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var plateGroup = CreatePlateGroup(plateId, "eu-uk", false);
            await SeedPlateGroupAsync(plateGroup);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(
                () => _handler.HandleAsync(plateId, CancellationToken.None));

            exception.Message.Should().Be("Plate must be United States region.");
        }

        [Test]
        public async Task HandleAsync_WithAlreadyEnrichedPlate_ThrowsArgumentException()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var plateGroup = CreatePlateGroup(plateId, "us-tx", true);
            await SeedPlateGroupAsync(plateGroup);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(
                () => _handler.HandleAsync(plateId, CancellationToken.None));

            exception.Message.Should().Be("Plate has already been enriched.");
        }

        [Test]
        public async Task HandleAsync_WithDifferentUsStates_ConvertsCorrectly()
        {
            // Arrange
            var testCases = new[]
            {
                ("us-ca", "CA"),
                ("us-tx", "TX"),
                ("us-ny", "NY"),
                ("us-fl", "FL")
            };

            foreach (var (region, expectedState) in testCases)
            {
                // Clear change tracker to prevent conflicts between iterations
                Context.ChangeTracker.Clear();
                
                var plateId = Guid.NewGuid();
                var plateGroup = CreatePlateGroup(plateId, region, false);
                await SeedPlateGroupAsync(plateGroup);

                var enrichedPlate = CreateEnrichedPlate();
                _licensePlateEnricherClient.GetLicenseInformationAsync(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<CancellationToken>())
                    .Returns(enrichedPlate);

                // Act
                await _handler.HandleAsync(plateId, CancellationToken.None);

                // Assert
                await _licensePlateEnricherClient.Received().GetLicenseInformationAsync(
                    plateGroup.BestNumber,
                    expectedState,
                    Arg.Any<CancellationToken>());

                _licensePlateEnricherClient.ClearReceivedCalls();
            }
        }

        [Test]
        public async Task HandleAsync_WhenEnricherThrowsException_PropagatesException()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var plateGroup = CreatePlateGroup(plateId, "us-ca", false);
            await SeedPlateGroupAsync(plateGroup);

            var expectedException = new InvalidOperationException("Enricher service failed");
            _licensePlateEnricherClient.GetLicenseInformationAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
                .Throws(expectedException);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                () => _handler.HandleAsync(plateId, CancellationToken.None));

            exception.Should().Be(expectedException);
        }

        [Test]
        public async Task HandleAsync_WithCancellationToken_PassesTokenToEnricher()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var plateGroup = CreatePlateGroup(plateId, "us-ca", false);
            await SeedPlateGroupAsync(plateGroup);

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var enrichedPlate = CreateEnrichedPlate();
            _licensePlateEnricherClient.GetLicenseInformationAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                cancellationToken)
                .Returns(enrichedPlate);

            // Act
            await _handler.HandleAsync(plateId, cancellationToken);

            // Assert
            await _licensePlateEnricherClient.Received(1).GetLicenseInformationAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                cancellationToken);
        }

        [Test]
        public async Task HandleAsync_WithNullEnrichedPlateValues_HandlesGracefully()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var plateGroup = CreatePlateGroup(plateId, "us-ca", false);
            await SeedPlateGroupAsync(plateGroup);

            var enrichedPlate = new EnrichedLicensePlate
            {
                Style = null,
                Make = null,
                Model = null,
                Year = null
            };

            _licensePlateEnricherClient.GetLicenseInformationAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
                .Returns(enrichedPlate);

            // Act
            await _handler.HandleAsync(plateId, CancellationToken.None);

            // Assert
            var updatedPlate = await GetPlateGroupByIdAsync(plateId);
            updatedPlate.VehicleType.Should().BeNull();
            updatedPlate.VehicleMake.Should().BeNull();
            updatedPlate.VehicleMakeModel.Should().Be(" "); // Make + " " + Model when both are null
            updatedPlate.VehicleYear.Should().BeNull();
            updatedPlate.IsEnriched.Should().BeTrue();
        }

        [Test]
        public async Task HandleAsync_WithEmptyStringEnrichedValues_HandlesCorrectly()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var plateGroup = CreatePlateGroup(plateId, "us-ca", false);
            await SeedPlateGroupAsync(plateGroup);

            var enrichedPlate = new EnrichedLicensePlate
            {
                Style = "",
                Make = "",
                Model = "",
                Year = ""
            };

            _licensePlateEnricherClient.GetLicenseInformationAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
                .Returns(enrichedPlate);

            // Act
            await _handler.HandleAsync(plateId, CancellationToken.None);

            // Assert
            var updatedPlate = await GetPlateGroupByIdAsync(plateId);
            updatedPlate.VehicleType.Should().Be("");
            updatedPlate.VehicleMake.Should().Be("");
            updatedPlate.VehicleMakeModel.Should().Be(" "); // Empty string + " " + Empty string
            updatedPlate.VehicleYear.Should().Be("");
            updatedPlate.IsEnriched.Should().BeTrue();
        }

        [Test]
        public async Task HandleAsync_WithPartialEnrichedData_UpdatesAvailableFields()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var plateGroup = CreatePlateGroup(plateId, "us-ca", false);
            await SeedPlateGroupAsync(plateGroup);

            var enrichedPlate = new EnrichedLicensePlate
            {
                Style = "Sedan",
                Make = "Toyota",
                Model = null, // Missing model
                Year = "2020"
            };

            _licensePlateEnricherClient.GetLicenseInformationAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
                .Returns(enrichedPlate);

            // Act
            await _handler.HandleAsync(plateId, CancellationToken.None);

            // Assert
            var updatedPlate = await GetPlateGroupByIdAsync(plateId);
            updatedPlate.VehicleType.Should().Be("Sedan");
            updatedPlate.VehicleMake.Should().Be("Toyota");
            updatedPlate.VehicleMakeModel.Should().Be("Toyota "); // Make + " " + null
            updatedPlate.VehicleYear.Should().Be("2020");
            updatedPlate.IsEnriched.Should().BeTrue();
        }

        [Test]
        public async Task HandleAsync_WithLowercaseRegion_ConvertsToUppercase()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var plateGroup = CreatePlateGroup(plateId, "us-ca", false);
            await SeedPlateGroupAsync(plateGroup);

            var enrichedPlate = CreateEnrichedPlate();
            _licensePlateEnricherClient.GetLicenseInformationAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
                .Returns(enrichedPlate);

            // Act
            await _handler.HandleAsync(plateId, CancellationToken.None);

            // Assert
            await _licensePlateEnricherClient.Received(1).GetLicenseInformationAsync(
                Arg.Any<string>(),
                "CA", // Should be uppercase
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task HandleAsync_WithExistingVehicleData_OverwritesWithEnrichedData()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var plateGroup = CreatePlateGroup(plateId, "us-ca", false);
            plateGroup.VehicleType = "Old Type";
            plateGroup.VehicleMake = "Old Make";
            plateGroup.VehicleMakeModel = "Old Make Model";
            plateGroup.VehicleYear = "2000";
            await SeedPlateGroupAsync(plateGroup);

            var enrichedPlate = CreateEnrichedPlate();
            _licensePlateEnricherClient.GetLicenseInformationAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
                .Returns(enrichedPlate);

            // Act
            await _handler.HandleAsync(plateId, CancellationToken.None);

            // Assert
            var updatedPlate = await GetPlateGroupByIdAsync(plateId);
            updatedPlate.VehicleType.Should().Be(enrichedPlate.Style);
            updatedPlate.VehicleMake.Should().Be(enrichedPlate.Make);
            updatedPlate.VehicleMakeModel.Should().Be($"{enrichedPlate.Make} {enrichedPlate.Model}");
            updatedPlate.VehicleYear.Should().Be(enrichedPlate.Year);
        }

        [Test]
        public async Task HandleAsync_CallsEnricherWithCorrectPlateNumber()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var expectedPlateNumber = "ABC123";
            var plateGroup = CreatePlateGroup(plateId, "us-ca", false);
            plateGroup.BestNumber = expectedPlateNumber;
            await SeedPlateGroupAsync(plateGroup);

            var enrichedPlate = CreateEnrichedPlate();
            _licensePlateEnricherClient.GetLicenseInformationAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
                .Returns(enrichedPlate);

            // Act
            await _handler.HandleAsync(plateId, CancellationToken.None);

            // Assert
            await _licensePlateEnricherClient.Received(1).GetLicenseInformationAsync(
                expectedPlateNumber,
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
        }

        private PlateGroup CreatePlateGroup(Guid id, string vehicleRegion, bool isEnriched)
        {
            return new PlateGroup
            {
                Id = id,
                BestNumber = "TEST123",
                VehicleRegion = vehicleRegion,
                IsEnriched = isEnriched,
                ReceivedOnEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                OpenAlprUuid = Guid.NewGuid().ToString(),
                OpenAlprCameraId = 1,
                Confidence = 95.5
            };
        }

        private EnrichedLicensePlate CreateEnrichedPlate()
        {
            return new EnrichedLicensePlate
            {
                Style = "Sedan",
                Make = "Toyota",
                Model = "Camry",
                Year = "2020",
                Vin = "1HGBH41JXMN109186",
                Engine = "2.4L"
            };
        }

        private async Task SeedPlateGroupAsync(PlateGroup plateGroup)
        {
            Context.PlateGroups.Add(plateGroup);
            await Context.SaveChangesAsync();
            
            // Detach the entity to prevent tracking conflicts when the handler fetches it
            Context.Entry(plateGroup).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        }

        private async Task<PlateGroup> GetPlateGroupByIdAsync(Guid id)
        {
            return await Context.PlateGroups
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
} 