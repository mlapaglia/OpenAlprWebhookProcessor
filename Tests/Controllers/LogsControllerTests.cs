using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.SystemLogs;
using OpenAlprWebhookProcessor.Features.SystemLogs.Queries.GetLogs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tests.TestHelpers;

namespace Tests.Controllers
{
    [TestFixture]
    public class LogsControllerTests : TestBase
    {
        private LogsController _controller;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _controller = new LogsController(Mediator);
        }

        [Test]
        public async Task GetLogs_ReturnsOkWithLogs()
        {
            // Arrange
            var expectedLogs = new List<string>
            {
                "2023-01-01 10:00:00 - INFO: Application started",
                "2023-01-01 10:01:00 - INFO: Database connected",
                "2023-01-01 10:02:00 - WARN: High memory usage detected",
                "2023-01-01 10:03:00 - ERROR: Failed to process webhook"
            };
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetLogsQuery>(), cancellationToken)
                .Returns(expectedLogs);

            // Act
            var result = await _controller.GetLogs(cancellationToken);

            // Assert
            AssertOkResult(result);
            var logs = GetControllerActionResult<List<string>>(result);
            logs.Should().NotBeNull();
            logs.Should().HaveCount(4);
            logs.Should().Contain("2023-01-01 10:00:00 - INFO: Application started");
            logs.Should().Contain("2023-01-01 10:01:00 - INFO: Database connected");
            logs.Should().Contain("2023-01-01 10:02:00 - WARN: High memory usage detected");
            logs.Should().Contain("2023-01-01 10:03:00 - ERROR: Failed to process webhook");
        }

        [Test]
        public async Task GetLogs_EmptyLogs_ReturnsOkWithEmptyList()
        {
            // Arrange
            var expectedLogs = new List<string>();
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetLogsQuery>(), cancellationToken)
                .Returns(expectedLogs);

            // Act
            var result = await _controller.GetLogs(cancellationToken);

            // Assert
            AssertOkResult(result);
            var logs = GetControllerActionResult<List<string>>(result);
            logs.Should().NotBeNull();
            logs.Should().BeEmpty();
        }

        [Test]
        public async Task GetLogs_NullLogs_ReturnsOkWithNullResult()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetLogsQuery>(), cancellationToken)
                .Returns((List<string>)null);

            // Act
            var result = await _controller.GetLogs(cancellationToken);

            // Assert
            AssertOkResult(result);
            var logs = GetControllerActionResult<List<string>>(result);
            logs.Should().BeNull();
        }

        [Test]
        public async Task GetLogs_CallsCorrectQuery()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetLogsQuery>(), cancellationToken)
                .Returns(new List<string>());

            // Act
            await _controller.GetLogs(cancellationToken);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Any<GetLogsQuery>(), 
                cancellationToken);
        }

        [Test]
        public async Task GetLogs_SingleLog_ReturnsOkWithSingleItem()
        {
            // Arrange
            var expectedLogs = new List<string>
            {
                "2023-01-01 10:00:00 - INFO: Single log entry"
            };
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetLogsQuery>(), cancellationToken)
                .Returns(expectedLogs);

            // Act
            var result = await _controller.GetLogs(cancellationToken);

            // Assert
            AssertOkResult(result);
            var logs = GetControllerActionResult<List<string>>(result);
            logs.Should().NotBeNull();
            logs.Should().HaveCount(1);
            logs[0].Should().Be("2023-01-01 10:00:00 - INFO: Single log entry");
        }

        [Test]
        public async Task GetLogs_LargeLogSet_ReturnsOkWithAllLogs()
        {
            // Arrange
            var expectedLogs = new List<string>();
            for (int i = 0; i < 1000; i++)
            {
                expectedLogs.Add($"2023-01-01 10:{i:D2}:00 - INFO: Log entry {i}");
            }
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetLogsQuery>(), cancellationToken)
                .Returns(expectedLogs);

            // Act
            var result = await _controller.GetLogs(cancellationToken);

            // Assert
            AssertOkResult(result);
            var logs = GetControllerActionResult<List<string>>(result);
            logs.Should().NotBeNull();
            logs.Should().HaveCount(1000);
            logs[0].Should().Be("2023-01-01 10:00:00 - INFO: Log entry 0");
            logs[999].Should().Be("2023-01-01 10:999:00 - INFO: Log entry 999");
        }

        [Test]
        public async Task GetLogs_LogsWithSpecialCharacters_ReturnsOkWithCorrectContent()
        {
            // Arrange
            var expectedLogs = new List<string>
            {
                "2023-01-01 10:00:00 - INFO: Special chars: !@#$%^&*()",
                "2023-01-01 10:01:00 - ERROR: JSON: {\"error\": \"Invalid request\"}",
                "2023-01-01 10:02:00 - WARN: Unicode: こんにちは世界",
                "2023-01-01 10:03:00 - INFO: Newline\ncontaining\nlog"
            };
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetLogsQuery>(), cancellationToken)
                .Returns(expectedLogs);

            // Act
            var result = await _controller.GetLogs(cancellationToken);

            // Assert
            AssertOkResult(result);
            var logs = GetControllerActionResult<List<string>>(result);
            logs.Should().NotBeNull();
            logs.Should().HaveCount(4);
            logs.Should().Contain("2023-01-01 10:00:00 - INFO: Special chars: !@#$%^&*()");
            logs.Should().Contain("2023-01-01 10:01:00 - ERROR: JSON: {\"error\": \"Invalid request\"}");
            logs.Should().Contain("2023-01-01 10:02:00 - WARN: Unicode: こんにちは世界");
            logs.Should().Contain("2023-01-01 10:03:00 - INFO: Newline\ncontaining\nlog");
        }

        [Test]
        public async Task GetLogs_ReturnsCorrectResponseType()
        {
            // Arrange
            var expectedLogs = new List<string> { "Test log" };
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetLogsQuery>(), cancellationToken)
                .Returns(expectedLogs);

            // Act
            var result = await _controller.GetLogs(cancellationToken);

            // Assert
            result.Should().BeOfType<ActionResult<List<string>>>();
            AssertOkResult(result);
        }

        [Test]
        public async Task GetLogs_MultipleCallsInSequence_EachCallsQuery()
        {
            // Arrange
            var expectedLogs = new List<string> { "Log entry" };
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetLogsQuery>(), cancellationToken)
                .Returns(expectedLogs);

            // Act
            await _controller.GetLogs(cancellationToken);
            await _controller.GetLogs(cancellationToken);
            await _controller.GetLogs(cancellationToken);

            // Assert
            await Mediator.Received(3).Send(
                Arg.Any<GetLogsQuery>(), 
                cancellationToken);
        }

        [Test]
        public async Task GetLogs_LogsWithDifferentLogLevels_ReturnsAllLogs()
        {
            // Arrange
            var expectedLogs = new List<string>
            {
                "2023-01-01 10:00:00 - TRACE: Detailed trace information",
                "2023-01-01 10:01:00 - DEBUG: Debug information",
                "2023-01-01 10:02:00 - INFO: General information",
                "2023-01-01 10:03:00 - WARN: Warning message",
                "2023-01-01 10:04:00 - ERROR: Error occurred",
                "2023-01-01 10:05:00 - FATAL: Critical error"
            };
            var cancellationToken = GetCancellationToken();

            Mediator.Send(Arg.Any<GetLogsQuery>(), cancellationToken)
                .Returns(expectedLogs);

            // Act
            var result = await _controller.GetLogs(cancellationToken);

            // Assert
            AssertOkResult(result);
            var logs = GetControllerActionResult<List<string>>(result);
            logs.Should().NotBeNull();
            logs.Should().HaveCount(6);
            logs.Should().Contain(log => log.Contains("TRACE"));
            logs.Should().Contain(log => log.Contains("DEBUG"));
            logs.Should().Contain(log => log.Contains("INFO"));
            logs.Should().Contain(log => log.Contains("WARN"));
            logs.Should().Contain(log => log.Contains("ERROR"));
            logs.Should().Contain(log => log.Contains("FATAL"));
        }
    }
} 