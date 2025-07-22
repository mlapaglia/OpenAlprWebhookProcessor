using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.MachineLearning;
using OpenAlprWebhookProcessor.Features.MachineLearning.Commands.TriggerTraining;
using OpenAlprWebhookProcessor.Features.MachineLearning.Commands.UpsertConfiguration;
using OpenAlprWebhookProcessor.Features.MachineLearning.Models;
using OpenAlprWebhookProcessor.Features.MachineLearning.Queries.GetConfiguration;
using OpenAlprWebhookProcessor.Features.MachineLearning.Queries.GetModelInfo;
using OpenAlprWebhookProcessor.Features.MachineLearning.Queries.GetModelStatus;
using OpenAlprWebhookProcessor.Features.MachineLearning.Queries.GetTopPredictions;
using OpenAlprWebhookProcessor.Features.MachineLearning.Queries.GetTrainingStatus;
using OpenAlprWebhookProcessor.Features.MachineLearning.Queries.PredictBatch;
using OpenAlprWebhookProcessor.Features.MachineLearning.Queries.PredictNextSeen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Tests.TestHelpers;

namespace Tests.Controllers
{
    [TestFixture]
    public class MachineLearningControllerTests : TestBase
    {
        private MachineLearningController _controller;
        private IMediator _mediator;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _mediator = Substitute.For<IMediator>();
            _controller = new MachineLearningController(_mediator);

            // Setup controller context for User.Identity
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "TestUser")
            }));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        #region Constructor Tests

        [Test]
        public void Constructor_WithValidMediator_InitializesCorrectly()
        {
            _controller.Should().NotBeNull();
        }

        #endregion

        #region PredictNextSeen Tests

        [Test]
        public async Task PredictNextSeen_WithValidInput_ReturnsOkResult()
        {
            // Arrange
            var input = new LicensePlateInput { LicensePlate = "ABC123" };
            var expectedResult = new LicensePlatePredictionResult
            {
                LicensePlate = "ABC123",
                PredictedHours = 4.0f,
                ConfidenceScore = 0.85
            };

            _mediator.Send(Arg.Any<PredictNextSeenQuery>(), Arg.Any<CancellationToken>())
                .Returns(expectedResult);

            // Act
            var result = await _controller.PredictNextSeen(input, CancellationToken.None);

            // Assert
            result.Should().BeOfType<ActionResult<LicensePlatePredictionResult>>();
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(expectedResult);

            await _mediator.Received(1).Send(
                Arg.Is<PredictNextSeenQuery>(q => q.Input == input), 
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task PredictNextSeen_WithNullInput_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.PredictNextSeen(null, CancellationToken.None);

            // Assert
            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("License plate is required");
        }

        [Test]
        public async Task PredictNextSeen_WithEmptyLicensePlate_ReturnsBadRequest()
        {
            // Arrange
            var input = new LicensePlateInput { LicensePlate = "" };

            // Act
            var result = await _controller.PredictNextSeen(input, CancellationToken.None);

            // Assert
            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("License plate is required");
        }

        [Test]
        public async Task PredictNextSeen_WhenHandlerThrowsArgumentException_ReturnsBadRequest()
        {
            // Arrange
            var input = new LicensePlateInput { LicensePlate = "ABC123" };

            _mediator.Send(Arg.Any<PredictNextSeenQuery>(), Arg.Any<CancellationToken>())
                .Throws(new ArgumentException("Invalid input"));

            // Act
            var result = await _controller.PredictNextSeen(input, CancellationToken.None);

            // Assert
            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Invalid input");
        }

        [Test]
        public async Task PredictNextSeen_WhenHandlerThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var input = new LicensePlateInput { LicensePlate = "ABC123" };

            _mediator.Send(Arg.Any<PredictNextSeenQuery>(), Arg.Any<CancellationToken>())
                .Throws(new InvalidOperationException("Service error"));

            // Act
            var result = await _controller.PredictNextSeen(input, CancellationToken.None);

            // Assert
            var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(500);
            statusCodeResult.Value.Should().Be("Error generating prediction");
        }

        #endregion

        #region PredictBatch Tests

        [Test]
        public async Task PredictBatch_WithValidInputs_ReturnsOkResult()
        {
            // Arrange
            var inputs = new List<LicensePlateInput>
            {
                new LicensePlateInput { LicensePlate = "ABC123" },
                new LicensePlateInput { LicensePlate = "XYZ789" }
            };

            var expectedResults = new List<LicensePlatePredictionResult>
            {
                new LicensePlatePredictionResult { LicensePlate = "ABC123", ConfidenceScore = 0.8 },
                new LicensePlatePredictionResult { LicensePlate = "XYZ789", ConfidenceScore = 0.7 }
            };

            _mediator.Send(Arg.Any<PredictBatchQuery>(), Arg.Any<CancellationToken>())
                .Returns(expectedResults);

            // Act
            var result = await _controller.PredictBatch(inputs, CancellationToken.None);

            // Assert
            result.Should().BeOfType<ActionResult<List<LicensePlatePredictionResult>>>();
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(expectedResults);
        }

        [Test]
        public async Task PredictBatch_WithNullInputs_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.PredictBatch(null, CancellationToken.None);

            // Assert
            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("At least one license plate input is required");
        }

        [Test]
        public async Task PredictBatch_WithEmptyInputsList_ReturnsBadRequest()
        {
            // Arrange
            var inputs = new List<LicensePlateInput>();

            // Act
            var result = await _controller.PredictBatch(inputs, CancellationToken.None);

            // Assert
            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("At least one license plate input is required");
        }

        [Test]
        public async Task PredictBatch_WithTooManyInputs_ReturnsBadRequest()
        {
            // Arrange
            var inputs = Enumerable.Range(1, 101)
                .Select(i => new LicensePlateInput { LicensePlate = $"PLATE{i}" })
                .ToList();

            // Act
            var result = await _controller.PredictBatch(inputs, CancellationToken.None);

            // Assert
            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Maximum 100 predictions per batch");
        }

        #endregion

        #region GetTopPredictions Tests

        [Test]
        public async Task GetTopPredictions_WithValidParameters_ReturnsOkResult()
        {
            // Arrange
            var expectedResults = new List<LicensePlatePredictionResult>
            {
                new LicensePlatePredictionResult { LicensePlate = "TOP001", ConfidenceScore = 0.9 }
            };

            _mediator.Send(Arg.Any<GetTopPredictionsQuery>(), Arg.Any<CancellationToken>())
                .Returns(expectedResults);

            // Act
            var result = await _controller.GetTopPredictions(10, 168, CancellationToken.None);

            // Assert
            result.Should().BeOfType<ActionResult<List<LicensePlatePredictionResult>>>();
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(expectedResults);
        }

        [Test]
        public async Task GetTopPredictions_WithInvalidCount_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.GetTopPredictions(0, 168, CancellationToken.None);

            // Assert
            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Count must be between 1 and 50");
        }

        [Test]
        public async Task GetTopPredictions_WithInvalidWithinHours_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.GetTopPredictions(10, 0, CancellationToken.None);

            // Assert
            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("WithinHours must be between 1 and 8760");
        }

        #endregion

        #region GetModelStatus Tests

        [Test]
        public async Task GetModelStatus_WithValidRequest_ReturnsOkResult()
        {
            // Arrange
            var expectedStatus = new ModelStatusDto
            {
                ModelAvailable = true,
                Status = "Ready",
                LastChecked = DateTime.UtcNow
            };

            _mediator.Send(Arg.Any<GetModelStatusQuery>(), Arg.Any<CancellationToken>())
                .Returns(expectedStatus);

            // Act
            var result = await _controller.GetModelStatus(CancellationToken.None);

            // Assert
            result.Should().BeOfType<ActionResult<ModelStatusDto>>();
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(expectedStatus);
        }

        [Test]
        public async Task GetModelStatus_WhenHandlerThrows_ReturnsInternalServerError()
        {
            // Arrange
            _mediator.Send(Arg.Any<GetModelStatusQuery>(), Arg.Any<CancellationToken>())
                .Throws(new InvalidOperationException("Service error"));

            // Act
            var result = await _controller.GetModelStatus(CancellationToken.None);

            // Assert
            var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(500);
            statusCodeResult.Value.Should().Be("Error checking model status");
        }

        #endregion

        #region TriggerTraining Tests

        [Test]
        public async Task TriggerTraining_WhenSuccessful_ReturnsOkResult()
        {
            // Arrange
            var expectedResult = new TrainingResultDto
            {
                Success = true,
                Message = "Training completed successfully",
                Timestamp = DateTime.UtcNow
            };

            _mediator.Send(Arg.Any<TriggerTrainingCommand>(), Arg.Any<CancellationToken>())
                .Returns(expectedResult);

            // Act
            var result = await _controller.TriggerTraining(CancellationToken.None);

            // Assert
            result.Should().BeOfType<ActionResult<TrainingResultDto>>();
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(expectedResult);

            await _mediator.Received(1).Send(
                Arg.Is<TriggerTrainingCommand>(c => c.RequestedBy == "TestUser"), 
                Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task TriggerTraining_WhenFails_ReturnsBadRequest()
        {
            // Arrange
            var expectedResult = new TrainingResultDto
            {
                Success = false,
                Message = "Training failed",
                Timestamp = DateTime.UtcNow
            };

            _mediator.Send(Arg.Any<TriggerTrainingCommand>(), Arg.Any<CancellationToken>())
                .Returns(expectedResult);

            // Act
            var result = await _controller.TriggerTraining(CancellationToken.None);

            // Assert
            result.Should().BeOfType<ActionResult<TrainingResultDto>>();
            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public async Task TriggerTraining_WhenHandlerThrows_ReturnsInternalServerError()
        {
            // Arrange
            _mediator.Send(Arg.Any<TriggerTrainingCommand>(), Arg.Any<CancellationToken>())
                .Throws(new InvalidOperationException("Service error"));

            // Act
            var result = await _controller.TriggerTraining(CancellationToken.None);

            // Assert
            var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(500);
            
            var resultValue = statusCodeResult.Value.Should().BeOfType<TrainingResultDto>().Subject;
            resultValue.Success.Should().BeFalse();
            resultValue.Message.Should().Be("Error triggering model training");
        }

        #endregion

        #region GetModelInfo Tests

        [Test]
        public async Task GetModelInfo_WithValidRequest_ReturnsOkResult()
        {
            // Arrange
            var expectedInfo = new ModelInfoDto
            {
                ModelAvailable = true,
                ModelType = "FastTree Regression",
                Features = new[] { "HourOfDay", "CameraId" },
                Description = "Test description"
            };

            _mediator.Send(Arg.Any<GetModelInfoQuery>(), Arg.Any<CancellationToken>())
                .Returns(expectedInfo);

            // Act
            var result = await _controller.GetModelInfo(CancellationToken.None);

            // Assert
            result.Should().BeOfType<ActionResult<ModelInfoDto>>();
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(expectedInfo);
        }

        [Test]
        public async Task GetModelInfo_WhenHandlerThrows_ReturnsInternalServerError()
        {
            // Arrange
            _mediator.Send(Arg.Any<GetModelInfoQuery>(), Arg.Any<CancellationToken>())
                .Throws(new InvalidOperationException("Service error"));

            // Act
            var result = await _controller.GetModelInfo(CancellationToken.None);

            // Assert
            var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(500);
            statusCodeResult.Value.Should().Be("Error retrieving model information");
        }

        #endregion

        #region GetTrainingStatus Tests

        [Test]
        public async Task GetTrainingStatusAsync_WithValidRequest_ReturnsOkResult()
        {
            // Arrange
            var expectedStatus = new TrainingStatusDto
            {
                IsTraining = true,
                TrainingDataCount = 1000,
                Configuration = new TrainingConfigurationDto()
            };

            _mediator.Send(Arg.Any<GetTrainingStatusQuery>(), Arg.Any<CancellationToken>())
                .Returns(expectedStatus);

            // Act
            var result = await _controller.GetTrainingStatusAsync(CancellationToken.None);

            // Assert
            result.Should().BeOfType<ActionResult<TrainingStatusDto>>();
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(expectedStatus);
        }

        [Test]
        public async Task GetTrainingStatusAsync_WhenHandlerThrows_ReturnsInternalServerError()
        {
            // Arrange
            _mediator.Send(Arg.Any<GetTrainingStatusQuery>(), Arg.Any<CancellationToken>())
                .Throws(new InvalidOperationException("Service error"));

            // Act
            var result = await _controller.GetTrainingStatusAsync(CancellationToken.None);

            // Assert
            var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(500);
            statusCodeResult.Value.Should().Be("Error retrieving training status");
        }

        #endregion

        #region Configuration Tests

        [Test]
        public async Task GetConfiguration_WithValidRequest_ReturnsOkResult()
        {
            // Arrange
            var expectedConfig = new MachineLearningConfigDto
            {
                MinimumModelQuality = 0.05,
                MinimumTrainingData = 100
            };

            _mediator.Send(Arg.Any<GetConfigurationQuery>(), Arg.Any<CancellationToken>())
                .Returns(expectedConfig);

            // Act
            var result = await _controller.GetConfiguration();

            // Assert
            result.Should().BeOfType<ActionResult<MachineLearningConfigDto>>();
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(expectedConfig);
        }

        [Test]
        public async Task UpsertConfiguration_WithValidRequest_ReturnsOkResult()
        {
            // Arrange
            var configDto = new MachineLearningConfigDto
            {
                MinimumModelQuality = 0.1,
                MinimumTrainingData = 200
            };

            _mediator.Send(Arg.Any<UpsertConfigurationCommand>(), Arg.Any<CancellationToken>())
                .Returns(Unit.Value);

            // Act
            var result = await _controller.UpsertConfiguration(configDto);

            // Assert
            result.Should().BeOfType<ActionResult<Unit>>();
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(Unit.Value);

            await _mediator.Received(1).Send(
                Arg.Is<UpsertConfigurationCommand>(c => 
                    c.Configuration == configDto && 
                    c.UpdatedBy == "TestUser"), 
                Arg.Any<CancellationToken>());
        }

        #endregion
    }
} 