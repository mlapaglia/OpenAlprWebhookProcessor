using AwesomeAssertions;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Ignores;
using OpenAlprWebhookProcessor.Features.Ignores.Commands.AddIgnore;
using OpenAlprWebhookProcessor.Features.Ignores.Commands.DeleteIgnore;
using OpenAlprWebhookProcessor.Features.Ignores.Commands.UpdateIgnore;
using OpenAlprWebhookProcessor.Features.Ignores.Queries.GetIgnores;
using Tests.TestHelpers;

namespace Tests.Controllers
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class IgnoresControllerTests : TestBase
    {
        private IgnoresController _controller;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _controller = new IgnoresController(Mediator);
        }

        #region AddIgnore Tests

        [Test]
        public async Task AddIgnore_ValidIgnore_ReturnsOk()
        {
            // Arrange
            var ignore = TestDataFactory.CreateTestIgnoreDto();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _controller.AddIgnore(ignore, cancellationToken);

            // Assert
            AssertOkResult(result);
            await Mediator.Received(1).Send(Arg.Any<AddIgnoreCommand>(), cancellationToken);
        }

        [Test]
        public async Task AddIgnore_CallsCorrectCommand()
        {
            // Arrange
            var ignore = TestDataFactory.CreateTestIgnoreDto("IGNORE123");
            ignore.Description = "Test ignore description";
            ignore.StrictMatch = true;
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.AddIgnore(ignore, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<AddIgnoreCommand>(cmd => 
                    cmd.Ignore.PlateNumber == "IGNORE123" && 
                    cmd.Ignore.Description == "Test ignore description" &&
                    cmd.Ignore.StrictMatch == true), 
                cancellationToken);
        }

        [Test]
        public async Task AddIgnore_WithNullDescription_CallsCorrectCommand()
        {
            // Arrange
            var ignore = TestDataFactory.CreateTestIgnoreDto("NULL123");
            ignore.Description = null;
            ignore.StrictMatch = false;
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.AddIgnore(ignore, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<AddIgnoreCommand>(cmd => 
                    cmd.Ignore.PlateNumber == "NULL123" && 
                    cmd.Ignore.Description == null &&
                    cmd.Ignore.StrictMatch == false), 
                cancellationToken);
        }

        [Test]
        public async Task AddIgnore_WithEmptyPlateNumber_CallsCorrectCommand()
        {
            // Arrange
            var ignore = TestDataFactory.CreateTestIgnoreDto("");
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.AddIgnore(ignore, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<AddIgnoreCommand>(cmd => cmd.Ignore.PlateNumber == ""), 
                cancellationToken);
        }

        #endregion

        #region GetIgnores Tests

        [Test]
        public async Task GetIgnores_ReturnsCorrectResult()
        {
            // Arrange
            var expectedIgnores = new List<IgnoreDto>
            {
                TestDataFactory.CreateTestIgnoreDto("IGNORE1"),
                TestDataFactory.CreateTestIgnoreDto("IGNORE2")
            };
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetIgnoresQuery>(), cancellationToken)
                .Returns(expectedIgnores);

            // Act
            var result = await _controller.GetIgnores(cancellationToken);

            // Assert
            AssertOkResult(result);
            var ignores = GetControllerActionResult<List<IgnoreDto>>(result);
            ignores.Should().HaveCount(2);
            ignores[0].PlateNumber.Should().Be("IGNORE1");
            ignores[1].PlateNumber.Should().Be("IGNORE2");
        }

        [Test]
        public async Task GetIgnores_CallsCorrectQuery()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();
            var expectedIgnores = new List<IgnoreDto>();
            Mediator.Send(Arg.Any<GetIgnoresQuery>(), cancellationToken)
                .Returns(expectedIgnores);

            // Act
            await _controller.GetIgnores(cancellationToken);

            // Assert
            await Mediator.Received(1).Send(Arg.Any<GetIgnoresQuery>(), cancellationToken);
        }

        [Test]
        public async Task GetIgnores_ReturnsEmptyList_WhenNoIgnores()
        {
            // Arrange
            var expectedIgnores = new List<IgnoreDto>();
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetIgnoresQuery>(), cancellationToken)
                .Returns(expectedIgnores);

            // Act
            var result = await _controller.GetIgnores(cancellationToken);

            // Assert
            AssertOkResult(result);
            var ignores = GetControllerActionResult<List<IgnoreDto>>(result);
            ignores.Should().BeEmpty();
        }

        [Test]
        public async Task GetIgnores_ReturnsIgnoresWithAllProperties()
        {
            // Arrange
            var expectedIgnore = TestDataFactory.CreateTestIgnoreDto("FULL123");
            expectedIgnore.Description = "Full test description";
            expectedIgnore.StrictMatch = true;
            
            var expectedIgnores = new List<IgnoreDto> { expectedIgnore };
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetIgnoresQuery>(), cancellationToken)
                .Returns(expectedIgnores);

            // Act
            var result = await _controller.GetIgnores(cancellationToken);

            // Assert
            AssertOkResult(result);
            var ignores = GetControllerActionResult<List<IgnoreDto>>(result);
            ignores.Should().HaveCount(1);
            
            var ignore = ignores[0];
            ignore.Id.Should().Be(expectedIgnore.Id);
            ignore.PlateNumber.Should().Be("FULL123");
            ignore.Description.Should().Be("Full test description");
            ignore.StrictMatch.Should().BeTrue();
        }

        #endregion

        #region UpdateIgnore Tests

        [Test]
        public async Task UpdateIgnore_ValidIgnore_ReturnsOk()
        {
            // Arrange
            var ignoreId = Guid.NewGuid();
            var ignore = TestDataFactory.CreateTestIgnoreDto("UPDATE123");
            ignore.Id = ignoreId;
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _controller.UpdateIgnore(ignoreId, ignore, cancellationToken);

            // Assert
            AssertOkResult(result);
            await Mediator.Received(1).Send(Arg.Any<UpdateIgnoreCommand>(), cancellationToken);
        }

        [Test]
        public async Task UpdateIgnore_CallsCorrectCommand()
        {
            // Arrange
            var ignoreId = Guid.NewGuid();
            var ignore = TestDataFactory.CreateTestIgnoreDto("UPDATE123");
            ignore.Id = ignoreId;
            ignore.Description = "Updated description";
            ignore.StrictMatch = true;
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.UpdateIgnore(ignoreId, ignore, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<UpdateIgnoreCommand>(cmd => 
                    cmd.Ignore.Id == ignoreId && 
                    cmd.Ignore.PlateNumber == "UPDATE123" && 
                    cmd.Ignore.Description == "Updated description" &&
                    cmd.Ignore.StrictMatch == true), 
                cancellationToken);
        }

        [Test]
        public async Task UpdateIgnore_MismatchedId_ReturnsBadRequest()
        {
            // Arrange
            var urlId = Guid.NewGuid();
            var bodyId = Guid.NewGuid();
            var ignore = TestDataFactory.CreateTestIgnoreDto("UPDATE123");
            ignore.Id = bodyId;
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _controller.UpdateIgnore(urlId, ignore, cancellationToken);

            // Assert
            AssertBadRequestResult(result);
            await Mediator.DidNotReceive().Send(Arg.Any<UpdateIgnoreCommand>(), cancellationToken);
        }

        [Test]
        public async Task UpdateIgnore_MismatchedId_ReturnsCorrectErrorMessage()
        {
            // Arrange
            var urlId = Guid.NewGuid();
            var bodyId = Guid.NewGuid();
            var ignore = TestDataFactory.CreateTestIgnoreDto("UPDATE123");
            ignore.Id = bodyId;
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _controller.UpdateIgnore(urlId, ignore, cancellationToken);

            // Assert
            result.Should().BeOfType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>();
            var badRequestResult = (Microsoft.AspNetCore.Mvc.BadRequestObjectResult)result;
            badRequestResult.Value.Should().Be("Ignore ID in URL does not match ignore ID in body");
        }

        [Test]
        public async Task UpdateIgnore_WithNullDescription_CallsCorrectCommand()
        {
            // Arrange
            var ignoreId = Guid.NewGuid();
            var ignore = TestDataFactory.CreateTestIgnoreDto("NULL123");
            ignore.Id = ignoreId;
            ignore.Description = null;
            ignore.StrictMatch = false;
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.UpdateIgnore(ignoreId, ignore, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<UpdateIgnoreCommand>(cmd => 
                    cmd.Ignore.Id == ignoreId && 
                    cmd.Ignore.PlateNumber == "NULL123" && 
                    cmd.Ignore.Description == null &&
                    cmd.Ignore.StrictMatch == false), 
                cancellationToken);
        }

        #endregion

        #region DeleteIgnore Tests

        [Test]
        public async Task DeleteIgnore_ValidId_ReturnsOk()
        {
            // Arrange
            var ignoreId = Guid.NewGuid();
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _controller.DeleteIgnore(ignoreId, cancellationToken);

            // Assert
            AssertOkResult(result);
            await Mediator.Received(1).Send(Arg.Any<DeleteIgnoreCommand>(), cancellationToken);
        }

        [Test]
        public async Task DeleteIgnore_CallsCorrectCommand()
        {
            // Arrange
            var ignoreId = Guid.NewGuid();
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.DeleteIgnore(ignoreId, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<DeleteIgnoreCommand>(cmd => cmd.Id == ignoreId), 
                cancellationToken);
        }

        [Test]
        public async Task DeleteIgnore_WithEmptyGuid_CallsCorrectCommand()
        {
            // Arrange
            var ignoreId = Guid.Empty;
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.DeleteIgnore(ignoreId, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<DeleteIgnoreCommand>(cmd => cmd.Id == Guid.Empty), 
                cancellationToken);
        }

        #endregion

        #region Edge Case Tests

        [Test]
        public async Task AddIgnore_WithSpecialCharacters_CallsCorrectCommand()
        {
            // Arrange
            var ignore = TestDataFactory.CreateTestIgnoreDto("ABC-123*");
            ignore.Description = "Special chars: @#$%^&*()";
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.AddIgnore(ignore, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<AddIgnoreCommand>(cmd => 
                    cmd.Ignore.PlateNumber == "ABC-123*" && 
                    cmd.Ignore.Description == "Special chars: @#$%^&*()"), 
                cancellationToken);
        }

        [Test]
        public async Task UpdateIgnore_WithSpecialCharacters_CallsCorrectCommand()
        {
            // Arrange
            var ignoreId = Guid.NewGuid();
            var ignore = TestDataFactory.CreateTestIgnoreDto("XYZ-456#");
            ignore.Id = ignoreId;
            ignore.Description = "Updated special chars: !@#$%";
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.UpdateIgnore(ignoreId, ignore, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<UpdateIgnoreCommand>(cmd => 
                    cmd.Ignore.Id == ignoreId && 
                    cmd.Ignore.PlateNumber == "XYZ-456#" && 
                    cmd.Ignore.Description == "Updated special chars: !@#$%"), 
                cancellationToken);
        }

        [Test]
        public async Task AddIgnore_WithLongPlateNumber_CallsCorrectCommand()
        {
            // Arrange
            var longPlateNumber = new string('A', 50); // Very long plate number
            var ignore = TestDataFactory.CreateTestIgnoreDto(longPlateNumber);
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.AddIgnore(ignore, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<AddIgnoreCommand>(cmd => cmd.Ignore.PlateNumber == longPlateNumber), 
                cancellationToken);
        }

        [Test]
        public async Task AddIgnore_WithLongDescription_CallsCorrectCommand()
        {
            // Arrange
            var longDescription = new string('D', 500); // Very long description
            var ignore = TestDataFactory.CreateTestIgnoreDto("LONG123");
            ignore.Description = longDescription;
            var cancellationToken = GetCancellationToken();

            // Act
            await _controller.AddIgnore(ignore, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<AddIgnoreCommand>(cmd => 
                    cmd.Ignore.PlateNumber == "LONG123" && 
                    cmd.Ignore.Description == longDescription), 
                cancellationToken);
        }

        #endregion

        #region Multiple Operations Tests

        [Test]
        public async Task MultipleOperations_EachCallsCorrectCommand()
        {
            // Arrange
            var ignoreId = Guid.NewGuid();
            var addIgnore = TestDataFactory.CreateTestIgnoreDto("ADD123");
            var updateIgnore = TestDataFactory.CreateTestIgnoreDto("UPDATE123");
            updateIgnore.Id = ignoreId;
            var cancellationToken = GetCancellationToken();

            // Act - Add
            await _controller.AddIgnore(addIgnore, cancellationToken);

            // Act - Update
            await _controller.UpdateIgnore(ignoreId, updateIgnore, cancellationToken);

            // Act - Delete
            await _controller.DeleteIgnore(ignoreId, cancellationToken);

            // Assert
            await Mediator.Received(1).Send(Arg.Any<AddIgnoreCommand>(), cancellationToken);
            await Mediator.Received(1).Send(Arg.Any<UpdateIgnoreCommand>(), cancellationToken);
            await Mediator.Received(1).Send(Arg.Any<DeleteIgnoreCommand>(), cancellationToken);
        }

        #endregion
    }
}
