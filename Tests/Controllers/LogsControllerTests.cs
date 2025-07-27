using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.SystemLogs;
using OpenAlprWebhookProcessor.Features.SystemLogs.Queries.GetLogs;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using Tests.TestHelpers;

namespace Tests.Controllers
{
    [TestFixture]
    public class LogsControllerTests : TestBase
    {
        private LogsController _controller;
        private GetLogsQueryHandler _handler;
        private MockFileSystem _mockFileSystem;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _controller = new LogsController(Mediator);
            _mockFileSystem = new MockFileSystem();
            _handler = new GetLogsQueryHandler(_mockFileSystem);
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        #region Controller Tests

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
            var result = await _controller.GetLogs(cancellationToken, ApiLogLevel.Verbose);

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
            var result = await _controller.GetLogs(cancellationToken, ApiLogLevel.Verbose);

            // Assert
            AssertOkResult(result);
            var logs = GetControllerActionResult<List<string>>(result);
            logs.Should().NotBeNull();
            logs.Should().BeEmpty();
        }

        [Test]
        public async Task GetLogs_PassesCorrectLogLevel()
        {
            // Arrange
            var cancellationToken = GetCancellationToken();
            var expectedLogLevel = ApiLogLevel.Warning;

            Mediator.Send(Arg.Any<GetLogsQuery>(), cancellationToken)
                .Returns(new List<string>());

            // Act
            await _controller.GetLogs(cancellationToken, expectedLogLevel);

            // Assert
            await Mediator.Received(1).Send(
                Arg.Is<GetLogsQuery>(q => q.MinimumSeverity == expectedLogLevel), 
                cancellationToken);
        }

        #endregion

        #region Handler Tests with Mocked FileSystem

        [Test]
        public async Task Handler_WithNoLogFiles_ReturnsEmptyList()
        {
            // Arrange
            _mockFileSystem.AddDirectory("./config");

            var query = new GetLogsQuery(ApiLogLevel.Verbose);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task Handler_WithSingleLineLogEntries_ParsesCorrectly()
        {
            // Arrange
            var logContent = @"2025-01-24 08:36:29.632 -04:00 [INF] Application started successfully
2025-01-24 08:36:30.123 -04:00 [WRN] High memory usage detected
2025-01-24 08:36:31.456 -04:00 [ERR] Failed to connect to database";

            SetupMockFileSystem(new[] { "./config/log-20250124.txt" }, logContent);

            var query = new GetLogsQuery(ApiLogLevel.Verbose);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Contain("Failed to connect to database");
            result[1].Should().Contain("High memory usage detected");
            result[2].Should().Contain("Application started successfully");
        }

        [Test]
        public async Task Handler_WithMultiLineLogEntries_GroupsCorrectly()
        {
            // Arrange
            var logContent = @"2025-01-24 08:36:29.632 -04:00 [INF] Route matched with {action = ""GetTopPredictions"", controller = ""MachineLearning""}. Executing controller action with signature System.Threading.Tasks.Task`1[Microsoft.AspNetCore.Mvc.ActionResult`1[System.Collections.Generic.List`1[OpenAlprWebhookProcessor.Features.MachineLearning.Models.LicensePlatePredictionResult]]] GetTopPredictions(Int32, Int32, System.Threading.CancellationToken) on controller OpenAlprWebhookProcessor.Features.MachineLearning.MachineLearningController (OpenAlprWebhookProcessor.Server).
2025-01-24 08:36:29.631 -04:00 [INF] Executing OkObjectResult, writing value of type 'OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetMostSeenPlates.GetMostSeenPlatesResponse'.
2025-01-24 08:36:30.100 -04:00 [ERR] An error occurred while processing request
   at OpenAlprWebhookProcessor.Something.Method() in /app/src/file.cs:line 42
   at OpenAlprWebhookProcessor.Another.Handler() in /app/src/handler.cs:line 15
   --- End of stack trace ---";

            SetupMockFileSystem(new[] { "./config/log-20250124.txt" }, logContent);

            var query = new GetLogsQuery(ApiLogLevel.Verbose);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            
            // Check that multi-line entries are grouped together
            var errorEntry = result.FirstOrDefault(r => r.Contains("An error occurred while processing request"));
            errorEntry.Should().NotBeNull();
            errorEntry.Should().Contain("at OpenAlprWebhookProcessor.Something.Method()");
            errorEntry.Should().Contain("at OpenAlprWebhookProcessor.Another.Handler()");
            errorEntry.Should().Contain("--- End of stack trace ---");
            
            // Verify single line entries are preserved
            result.Should().Contain(r => r.Contains("Route matched with") && r.Contains("GetTopPredictions"));
            result.Should().Contain(r => r.Contains("Executing OkObjectResult"));
        }

        [Test]
        public async Task Handler_WithLogLevelFiltering_FiltersCorrectly()
        {
            // Arrange
            var logContent = @"2025-01-24 08:36:29.632 -04:00 [VRB] Verbose message
2025-01-24 08:36:30.123 -04:00 [DBG] Debug message
2025-01-24 08:36:31.456 -04:00 [INF] Information message
2025-01-24 08:36:32.789 -04:00 [WRN] Warning message
2025-01-24 08:36:33.012 -04:00 [ERR] Error message
2025-01-24 08:36:34.345 -04:00 [FTL] Fatal message";

            SetupMockFileSystem(new[] { "./config/log-20250124.txt" }, logContent);

            var query = new GetLogsQuery(ApiLogLevel.Warning);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3); // Warning, Error, Fatal
            result.Should().Contain(r => r.Contains("[WRN]"));
            result.Should().Contain(r => r.Contains("[ERR]"));
            result.Should().Contain(r => r.Contains("[FTL]"));
            result.Should().NotContain(r => r.Contains("[VRB]"));
            result.Should().NotContain(r => r.Contains("[DBG]"));
            result.Should().NotContain(r => r.Contains("[INF]"));
        }

        [Test]
        public async Task Handler_WithLargeLogFile_LimitsTo500Entries()
        {
            // Arrange
            var logBuilder = new StringBuilder();
            for (int i = 0; i < 600; i++)
            {
                // Generate valid timestamps by using hours and minutes properly
                var hour = 8 + (i / 3600); // Each hour has 3600 seconds
                var minute = (i / 60) % 60; // Minutes from 0-59
                var second = i % 60; // Seconds from 0-59
                var millisecond = (i * 17) % 1000; // Vary milliseconds to make timestamps unique
                
                logBuilder.AppendLine($"2025-01-24 {hour:D2}:{minute:D2}:{second:D2}.{millisecond:D3} -04:00 [INF] Log entry {i}");
            }

            SetupMockFileSystem(new[] { "./config/log-20250124.txt" }, logBuilder.ToString());

            var query = new GetLogsQuery(ApiLogLevel.Verbose);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(500);
            // Should be in reverse order (first 500 entries, then reversed)
            result[0].Should().Contain("Log entry 499"); // Last of the first 500 entries
            result[499].Should().Contain("Log entry 0"); // First of the first 500 entries
        }

        [Test]
        public async Task Handler_WithComplexMultiLineStackTrace_PreservesFormatting()
        {
            // Arrange
            var logContent = @"2025-01-24 08:36:29.632 -04:00 [ERR] Unhandled exception occurred during webhook processing
System.InvalidOperationException: Unable to process license plate data
   at OpenAlprWebhookProcessor.WebhookProcessor.GroupWebhookHandler.ProcessGroupAsync(Group group, CancellationToken cancellationToken = default) in /app/src/WebhookProcessor/GroupWebhookHandler.cs:line 45
   at OpenAlprWebhookProcessor.WebhookProcessor.GroupWebhookHandler.HandleAsync(WebhookRequest request, CancellationToken cancellationToken = default) in /app/src/WebhookProcessor/GroupWebhookHandler.cs:line 28
   at OpenAlprWebhookProcessor.Features.Webhooks.WebhookController.ProcessWebhook(WebhookRequest request, CancellationToken cancellationToken = default) in /app/src/Features/Webhooks/WebhookController.cs:line 67
   --- End of inner exception stack trace ---
   at System.Threading.Tasks.Task.ThrowIfExceptional(Boolean includeTaskCanceledExceptions)
   at System.Threading.Tasks.Task.Wait(Int32 millisecondsTimeout, CancellationToken cancellationToken = default)
2025-01-24 08:36:30.123 -04:00 [INF] Webhook processing completed";

            SetupMockFileSystem(new[] { "./config/log-20250124.txt" }, logContent);

            var query = new GetLogsQuery(ApiLogLevel.Verbose);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            
            var stackTraceEntry = result.FirstOrDefault(r => r.Contains("Unhandled exception occurred"));
            stackTraceEntry.Should().NotBeNull();
            stackTraceEntry.Should().Contain("System.InvalidOperationException");
            stackTraceEntry.Should().Contain("at OpenAlprWebhookProcessor.WebhookProcessor.GroupWebhookHandler.ProcessGroupAsync");
            stackTraceEntry.Should().Contain("--- End of inner exception stack trace ---");
            stackTraceEntry.Should().Contain("at System.Threading.Tasks.Task.ThrowIfExceptional");
            
            var simpleEntry = result.FirstOrDefault(r => r.Contains("Webhook processing completed"));
            simpleEntry.Should().NotBeNull();
            simpleEntry.Should().Be("2025-01-24 08:36:30.123 -04:00 [INF] Webhook processing completed");
        }

        [Test]
        public async Task Handler_WithEmptyLogFile_ReturnsEmptyList()
        {
            // Arrange
            SetupMockFileSystem(new[] { "./config/log-20250124.txt" }, "");

            var query = new GetLogsQuery(ApiLogLevel.Verbose);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task Handler_WithVerboseLogLevel_IncludesAllLevels()
        {
            // Arrange
            var logContent = @"2025-01-24 08:36:29.632 -04:00 [VRB] Verbose message
2025-01-24 08:36:30.123 -04:00 [DBG] Debug message
2025-01-24 08:36:31.456 -04:00 [INF] Information message
2025-01-24 08:36:32.789 -04:00 [WRN] Warning message
2025-01-24 08:36:33.012 -04:00 [ERR] Error message
2025-01-24 08:36:34.345 -04:00 [FTL] Fatal message";

            SetupMockFileSystem(new[] { "./config/log-20250124.txt" }, logContent);

            var query = new GetLogsQuery(ApiLogLevel.Verbose); // Include all levels
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(6); // All log levels included
            result.Should().Contain(r => r.Contains("[VRB]"));
            result.Should().Contain(r => r.Contains("[DBG]"));
            result.Should().Contain(r => r.Contains("[INF]"));
            result.Should().Contain(r => r.Contains("[WRN]"));
            result.Should().Contain(r => r.Contains("[ERR]"));
            result.Should().Contain(r => r.Contains("[FTL]"));
        }

        [Test]
        public async Task Handler_WithInvalidLogLevelEntry_IncludesEntryWhenFilterIsVerbose()
        {
            // Arrange
            var logContent = @"2025-01-24 08:36:29.632 -04:00 [XYZ] Invalid log level entry
2025-01-24 08:36:30.123 -04:00 [INF] Valid information entry";

            SetupMockFileSystem(new[] { "./config/log-20250124.txt" }, logContent);

            var query = new GetLogsQuery(ApiLogLevel.Verbose);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(r => r.Contains("[XYZ] Invalid log level entry"));
            result.Should().Contain(r => r.Contains("[INF] Valid information entry"));
        }

        #endregion

        #region Helper Methods

        private void SetupMockFileSystem(string[] logFiles, string logContent)
        {
            _mockFileSystem.AddDirectory("./config");

            foreach (var logFile in logFiles)
            {
                _mockFileSystem.AddFile(logFile, new MockFileData(logContent));
            }
        }

        #endregion
    }
} 