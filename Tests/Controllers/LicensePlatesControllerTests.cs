using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using OpenAlprWebhookProcessor.Controllers;
using OpenAlprWebhookProcessor.Features.LicensePlates.Commands.UpsertPlate;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.SearchLicensePlates;
using OpenAlprWebhookProcessor.LicensePlates;
using OpenAlprWebhookProcessor.LicensePlates.SearchLicensePlates;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.Controllers
{
    [TestFixture]
    public class LicensePlatesControllerTests
    {
        private IMediator _mediator;
        private LicensePlatesController _controller;

        [SetUp]
        public void Setup()
        {
            _mediator = Substitute.For<IMediator>();
            _controller = new LicensePlatesController(_mediator);
        }

        [Test]
        public async Task SearchPlates_ValidRequest_ReturnsOkWithResults()
        {
            // Arrange
            var request = new SearchLicensePlateRequest
            {
                PlateNumber = "ABC123",
                PageNumber = 0,
                PageSize = 10
            };

            var expectedResponse = new SearchLicensePlateResponse
            {
                Plates = new List<LicensePlate>
                {
                    new LicensePlate
                    {
                        Id = Guid.NewGuid(),
                        PlateNumber = "ABC123",
                        VehicleColor = "Red",
                        VehicleMake = "Toyota",
                        VehicleModel = "Camry"
                    }
                },
                TotalCount = 1
            };

            _mediator.Send(Arg.Any<SearchLicensePlatesQuery>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _controller.SearchPlates(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<ActionResult<SearchLicensePlateResponse>>();
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.Value.Should().BeEquivalentTo(expectedResponse);

            await _mediator.Received(1).Send(
                Arg.Is<SearchLicensePlatesQuery>(q => 
                    q.PlateNumber == request.PlateNumber && 
                    q.PageNumber == request.PageNumber && 
                    q.PageSize == request.PageSize),
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task SearchPlates_MapsAllQueryParametersCorrectly()
        {
            // Arrange
            var request = new SearchLicensePlateRequest
            {
                PlateNumber = "ABC123",
                StrictMatch = true,
                RegexSearchEnabled = false,
                StartSearchOn = DateTimeOffset.UtcNow.AddDays(-1),
                EndSearchOn = DateTimeOffset.UtcNow,
                FilterIgnoredPlates = true,
                VehicleColor = "Red",
                VehicleMake = "Toyota",
                VehicleModel = "Camry",
                VehicleType = "Sedan",
                VehicleRegion = "us-ca",
                FilterPlatesSeenLessThan = 5,
                PageNumber = 1,
                PageSize = 20
            };

            var expectedResponse = new SearchLicensePlateResponse
            {
                Plates = new List<LicensePlate>(),
                TotalCount = 0
            };

            _mediator.Send(Arg.Any<SearchLicensePlatesQuery>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(expectedResponse));

            // Act
            await _controller.SearchPlates(request, CancellationToken.None);

            // Assert
            await _mediator.Received(1).Send(
                Arg.Is<SearchLicensePlatesQuery>(q => 
                    q.PlateNumber == request.PlateNumber &&
                    q.StrictMatch == request.StrictMatch &&
                    q.RegexSearchEnabled == request.RegexSearchEnabled &&
                    q.StartSearchOn == request.StartSearchOn &&
                    q.EndSearchOn == request.EndSearchOn &&
                    q.FilterIgnoredPlates == request.FilterIgnoredPlates &&
                    q.VehicleColor == request.VehicleColor &&
                    q.VehicleMake == request.VehicleMake &&
                    q.VehicleModel == request.VehicleModel &&
                    q.VehicleType == request.VehicleType &&
                    q.VehicleRegion == request.VehicleRegion &&
                    q.FilterPlatesSeenLessThan == request.FilterPlatesSeenLessThan &&
                    q.PageNumber == request.PageNumber &&
                    q.PageSize == request.PageSize),
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task UpsertPlate_ValidRequest_ReturnsOk()
        {
            // Arrange
            var licensePlate = new LicensePlate
            {
                Id = Guid.NewGuid(),
                PlateNumber = "ABC123",
                Description = "Test plate",
                IsAlert = true,
                IsIgnore = false,
                VehicleColor = "Red",
                VehicleMake = "Toyota",
                VehicleModel = "Camry",
                VehicleType = "Sedan",
                VehicleRegion = "us-ca",
                Latitude = 37.7749,
                Longitude = -122.4194,
                ReceivedOnEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                IsStrictMatch = true,
                IsPatternMatch = false,
                IsEnriched = false,
                Notes = "Test notes"
            };

            // Act
            var result = await _controller.UpsertPlate(licensePlate, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkResult>();

            await _mediator.Received(1).Send(
                Arg.Is<UpsertPlateCommand>(cmd => 
                    cmd.Id == licensePlate.Id &&
                    cmd.PlateNumber == licensePlate.PlateNumber &&
                    cmd.Description == licensePlate.Description &&
                    cmd.IsAlert == licensePlate.IsAlert &&
                    cmd.IsIgnore == licensePlate.IsIgnore &&
                    cmd.VehicleColor == licensePlate.VehicleColor &&
                    cmd.VehicleMake == licensePlate.VehicleMake &&
                    cmd.VehicleModel == licensePlate.VehicleModel &&
                    cmd.VehicleType == licensePlate.VehicleType &&
                    cmd.VehicleRegion == licensePlate.VehicleRegion &&
                    cmd.Latitude == licensePlate.Latitude &&
                    cmd.Longitude == licensePlate.Longitude &&
                    cmd.ReceivedOnEpoch == licensePlate.ReceivedOnEpoch &&
                    cmd.IsStrictMatch == licensePlate.IsStrictMatch &&
                    cmd.IsPatternMatch == licensePlate.IsPatternMatch &&
                    cmd.IsEnriched == licensePlate.IsEnriched &&
                    cmd.Notes == licensePlate.Notes),
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task UpsertPlate_MediatorThrowsException_ExceptionPropagates()
        {
            // Arrange
            var licensePlate = new LicensePlate
            {
                Id = Guid.NewGuid(),
                PlateNumber = "ABC123"
            };

            _mediator.Send(Arg.Any<UpsertPlateCommand>(), Arg.Any<CancellationToken>())
                .ThrowsAsync(new InvalidOperationException("Test exception"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _controller.UpsertPlate(licensePlate, CancellationToken.None));

            exception.Message.Should().Be("Test exception");
        }
    }
} 