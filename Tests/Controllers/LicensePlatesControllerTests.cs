using AwesomeAssertions;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features;
using OpenAlprWebhookProcessor.Features.LicensePlates;
using OpenAlprWebhookProcessor.Features.LicensePlates.Commands.DeletePlate;
using OpenAlprWebhookProcessor.Features.LicensePlates.Commands.EditPlate;
using OpenAlprWebhookProcessor.Features.LicensePlates.Commands.EnrichPlate;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetLicensePlateCounts;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetMostSeenPlates;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetPlate;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetPlateFilters;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetStatistics;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.SearchLicensePlates;
using Tests.TestHelpers;

namespace Tests.Controllers
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class LicensePlatesControllerTests : TestBase
    {
        private LicensePlatesController _controller;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _controller = new LicensePlatesController(Mediator);
        }

        [Test]
        public async Task SearchPlates_ValidRequest_ReturnsCorrectResult()
        {
            // Arrange
            var searchRequest = TestDataFactory.CreateTestSearchLicensePlateRequest("ABC123");
            var expectedResponse = TestDataFactory.CreateTestSearchLicensePlateResponse();
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<SearchLicensePlatesQuery>(), cancellationToken)
                .Returns(expectedResponse);

            // Act
            var result = await _controller.SearchPlates(searchRequest, cancellationToken);

            // Assert
            AssertOkResult(result);
            var response = GetControllerActionResult<SearchLicensePlateResponse>(result);
            response.Should().NotBeNull();
            response.Plates.Should().HaveCount(2);
            response.TotalCount.Should().Be(2);
        }

        [Test]
        public async Task SearchPlates_CallsCorrectCommand()
        {
            // Arrange
            var searchRequest = TestDataFactory.CreateTestSearchLicensePlateRequest("TEST123");
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.SearchPlates(searchRequest, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<SearchLicensePlatesQuery>(q => 
                    q.PlateNumber == "TEST123" && 
                    q.StrictMatch == searchRequest.StrictMatch &&
                    q.RegexSearchEnabled == searchRequest.RegexSearchEnabled &&
                    q.PageNumber == searchRequest.PageNumber &&
                    q.PageSize == searchRequest.PageSize), 
                cancellationToken);
        }

        [Test]
        public async Task UpsertPlate_ValidPlate_ReturnsOk()
        {
            // Arrange
            var licensePlate = TestDataFactory.CreateTestLicensePlate("EDIT123");
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _controller.UpsertPlate(licensePlate, cancellationToken);

            // Assert
            AssertOkResult(result);
            await Mediator.Received(1).Send(Arg.Any<EditPlateCommand>(), cancellationToken);
        }

        [Test]
        public async Task UpsertPlate_CallsCorrectCommand()
        {
            // Arrange
            var licensePlate = TestDataFactory.CreateTestLicensePlate("EDIT123");
            licensePlate.Notes = "Updated notes";
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.UpsertPlate(licensePlate, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<EditPlateCommand>(cmd => 
                    cmd.Id == licensePlate.Id && 
                    cmd.PlateNumber == "EDIT123" && 
                    cmd.Notes == "Updated notes"), 
                cancellationToken);
        }

        [Test]
        public async Task GetPlate_ValidId_ReturnsCorrectResult()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var expectedPlate = TestDataFactory.CreateTestLicensePlate("GET123");
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetPlateQuery>(), cancellationToken)
                .Returns(expectedPlate);

            // Act
            var result = await _controller.GetPlate(plateId, cancellationToken);

            // Assert
            AssertOkResult(result);
            var plate = GetControllerActionResult<LicensePlate>(result);
            plate.Should().NotBeNull();
            plate.PlateNumber.Should().Be("GET123");
        }

        [Test]
        public async Task GetPlate_PlateNotFound_ReturnsNotFound()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetPlateQuery>(), cancellationToken)
                .Returns((LicensePlate)null);

            // Act
            var result = await _controller.GetPlate(plateId, cancellationToken);

            // Assert
            AssertNotFoundResult(result.Result);
        }

        [Test]
        public async Task GetPlate_CallsCorrectQuery()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.GetPlate(plateId, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<GetPlateQuery>(q => q.Id == plateId), 
                cancellationToken);
        }

        [Test]
        public async Task GetLicensePlateCounts_ValidDates_ReturnsCorrectResult()
        {
            // Arrange
            var startDate = DateTimeOffset.UtcNow.AddDays(-30);
            var endDate = DateTimeOffset.UtcNow;
            var expectedResponse = TestDataFactory.CreateTestGetLicensePlateCountsResponse();
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetLicensePlateCountsQuery>(), cancellationToken)
                .Returns(expectedResponse);

            // Act
            var result = await _controller.GetLicensePlateCounts(startDate, endDate, cancellationToken);

            // Assert
            AssertOkResult(result);
            var response = GetControllerActionResult<GetLicensePlateCountsResponse>(result);
            response.Should().NotBeNull();
            response.Counts.Should().HaveCount(2);
        }

        [Test]
        public async Task GetLicensePlateCounts_CallsCorrectQuery()
        {
            // Arrange
            var startDate = DateTimeOffset.UtcNow.AddDays(-30);
            var endDate = DateTimeOffset.UtcNow;
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.GetLicensePlateCounts(startDate, endDate, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<GetLicensePlateCountsQuery>(q => 
                    q.StartDate == startDate && 
                    q.EndDate == endDate), 
                cancellationToken);
        }

        [Test]
        public async Task GetMostSeenPlates_ValidParameters_ReturnsCorrectResult()
        {
            // Arrange
            var startDate = DateTimeOffset.UtcNow.AddDays(-30);
            var endDate = DateTimeOffset.UtcNow;
            var limit = 20;
            var expectedResponse = TestDataFactory.CreateTestGetMostSeenPlatesResponse();
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetMostSeenPlatesQuery>(), cancellationToken)
                .Returns(expectedResponse);

            // Act
            var result = await _controller.GetMostSeenPlates(startDate, endDate, cancellationToken, limit);

            // Assert
            AssertOkResult(result);
            var response = GetControllerActionResult<GetMostSeenPlatesResponse>(result);
            response.Should().NotBeNull();
            response.Counts.Should().HaveCount(2);
            response.Counts[0].PlateNumber.Should().Be("ABC123");
            response.Counts[0].Count.Should().Be(25);
        }

        [Test]
        public async Task GetMostSeenPlates_CallsCorrectQuery()
        {
            // Arrange
            var startDate = DateTimeOffset.UtcNow.AddDays(-30);
            var endDate = DateTimeOffset.UtcNow;
            var limit = 15;
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.GetMostSeenPlates(startDate, endDate, cancellationToken, limit);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<GetMostSeenPlatesQuery>(q => 
                    q.StartDate == startDate && 
                    q.EndDate == endDate && 
                    q.Limit == limit), 
                cancellationToken);
        }

        [Test]
        public async Task GetMostSeenPlates_DefaultLimit_UsesCorrectDefault()
        {
            // Arrange
            var startDate = DateTimeOffset.UtcNow.AddDays(-30);
            var endDate = DateTimeOffset.UtcNow;
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.GetMostSeenPlates(startDate, endDate, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<GetMostSeenPlatesQuery>(q => q.Limit == 10), 
                cancellationToken);
        }

        [Test]
        public async Task GetStatistics_ValidPlateNumber_ReturnsCorrectResult()
        {
            // Arrange
            var plateNumber = "STATS123";
            var expectedStatistics = TestDataFactory.CreateTestPlateStatistics();
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetStatisticsQuery>(), cancellationToken)
                .Returns(expectedStatistics);

            // Act
            var result = await _controller.GetStatistics(plateNumber, cancellationToken);

            // Assert
            AssertOkResult(result);
            // Note: Controller returns ActionResult instead of ActionResult<T> for this endpoint
        }

        [Test]
        public async Task GetStatistics_CallsCorrectQuery()
        {
            // Arrange
            var plateNumber = "STATS123";
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.GetStatistics(plateNumber, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<GetStatisticsQuery>(q => q.PlateNumber == plateNumber), 
                cancellationToken);
        }

        [Test]
        public async Task GetPlateFilters_ReturnsCorrectResult()
        {
            // Arrange
            var expectedFilters = TestDataFactory.CreateTestGetLicensePlateFiltersResponse();
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetPlateFiltersQuery>(), cancellationToken)
                .Returns(expectedFilters);

            // Act
            var result = await _controller.GetPlateFilters(cancellationToken);

            // Assert
            AssertOkResult(result);
            // Note: Controller returns ActionResult instead of ActionResult<T> for this endpoint
        }

        [Test]
        public async Task GetPlateFilters_CallsCorrectQuery()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.GetPlateFilters(cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Any<GetPlateFiltersQuery>(), 
                cancellationToken);
        }

        [Test]
        public async Task DeletePlate_ValidId_ReturnsOk()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _controller.DeletePlate(plateId, cancellationToken);

            // Assert
            AssertOkResult(result);
            await Mediator.Received(1).Send(Arg.Any<DeletePlateCommand>(), cancellationToken);
        }

        [Test]
        public async Task DeletePlate_CallsCorrectCommand()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.DeletePlate(plateId, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<DeletePlateCommand>(cmd => cmd.Id == plateId), 
                cancellationToken);
        }

        [Test]
        public async Task EnrichPlate_ValidId_ReturnsOk()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _controller.EnrichPlate(plateId, cancellationToken);

            // Assert
            AssertOkResult(result);
            await Mediator.Received(1).Send(Arg.Any<EnrichPlateCommand>(), cancellationToken);
        }

        [Test]
        public async Task EnrichPlate_CallsCorrectCommand()
        {
            // Arrange
            var plateId = Guid.NewGuid();
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.EnrichPlate(plateId, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<EnrichPlateCommand>(cmd => cmd.PlateId == plateId), 
                cancellationToken);
        }
    }
} 