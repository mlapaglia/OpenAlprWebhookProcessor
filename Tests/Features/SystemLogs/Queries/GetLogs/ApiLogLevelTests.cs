using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.SystemLogs.Queries.GetLogs;
using Serilog.Events;

namespace Tests.Features.SystemLogs.Queries.GetLogs
{
    [TestFixture]
    public class ApiLogLevelTests
    {
        [Test]
        public void ApiLogLevel_ShouldHaveCorrectValues()
        {
            // Arrange & Act & Assert
            ((int)ApiLogLevel.Verbose).Should().Be(0);
            ((int)ApiLogLevel.Debug).Should().Be(1);
            ((int)ApiLogLevel.Information).Should().Be(2);
            ((int)ApiLogLevel.Warning).Should().Be(3);
            ((int)ApiLogLevel.Error).Should().Be(4);
            ((int)ApiLogLevel.Critical).Should().Be(5);
        }

        [TestFixture]
        public class ApiLogLevelExtensionsTests
        {
            [Test]
            [TestCase(ApiLogLevel.Verbose, LogLevel.Trace)]
            [TestCase(ApiLogLevel.Debug, LogLevel.Debug)]
            [TestCase(ApiLogLevel.Information, LogLevel.Information)]
            [TestCase(ApiLogLevel.Warning, LogLevel.Warning)]
            [TestCase(ApiLogLevel.Error, LogLevel.Error)]
            [TestCase(ApiLogLevel.Critical, LogLevel.Critical)]
            public void ToSystemLogLevel_ShouldReturnCorrectLogLevel(ApiLogLevel apiLevel, LogLevel expectedLogLevel)
            {
                // Act
                var result = apiLevel.ToSystemLogLevel();

                // Assert
                result.Should().Be(expectedLogLevel);
            }

            [Test]
            public void ToSystemLogLevel_WithInvalidValue_ShouldReturnInformation()
            {
                // Arrange
                var invalidApiLevel = (ApiLogLevel)99;

                // Act
                var result = invalidApiLevel.ToSystemLogLevel();

                // Assert
                result.Should().Be(LogLevel.Information);
            }

            [Test]
            [TestCase(LogEventLevel.Verbose, ApiLogLevel.Verbose)]
            [TestCase(LogEventLevel.Debug, ApiLogLevel.Debug)]
            [TestCase(LogEventLevel.Information, ApiLogLevel.Information)]
            [TestCase(LogEventLevel.Warning, ApiLogLevel.Warning)]
            [TestCase(LogEventLevel.Error, ApiLogLevel.Error)]
            [TestCase(LogEventLevel.Fatal, ApiLogLevel.Critical)]
            public void ToApiLogLevel_ShouldReturnCorrectApiLogLevel(LogEventLevel serilogLevel, ApiLogLevel expectedApiLevel)
            {
                // Act
                var result = serilogLevel.ToApiLogLevel();

                // Assert
                result.Should().Be(expectedApiLevel);
            }

            [Test]
            public void ToApiLogLevel_WithInvalidValue_ShouldReturnInformation()
            {
                // Arrange
                var invalidSerilogLevel = (LogEventLevel)99;

                // Act
                var result = invalidSerilogLevel.ToApiLogLevel();

                // Assert
                result.Should().Be(ApiLogLevel.Information);
            }

            [Test]
            public void ToSystemLogLevel_AllEnumValues_ShouldBeCovered()
            {
                // Arrange
                var allApiLogLevels = System.Enum.GetValues<ApiLogLevel>();

                // Act & Assert
                foreach (var apiLevel in allApiLogLevels)
                {
                    var result = apiLevel.ToSystemLogLevel();
                    // Enum conversions should always return a valid LogLevel
                    System.Enum.IsDefined(typeof(LogLevel), result).Should().BeTrue();
                }
            }

            [Test]
            public void ToApiLogLevel_AllEnumValues_ShouldBeCovered()
            {
                // Arrange
                var allSerilogLevels = System.Enum.GetValues<LogEventLevel>();

                // Act & Assert
                foreach (var serilogLevel in allSerilogLevels)
                {
                    var result = serilogLevel.ToApiLogLevel();
                    // Conversions should always return a valid ApiLogLevel
                    System.Enum.IsDefined(typeof(ApiLogLevel), result).Should().BeTrue();
                }
            }

            [Test]
            public void LogLevelConversions_ShouldBeSymmetric_WhereApplicable()
            {
                // Test that conversions work correctly for overlapping levels
                // Note: Not all conversions are symmetric due to different level granularities

                // Test Verbose/Trace
                var verboseToSystem = ApiLogLevel.Verbose.ToSystemLogLevel();
                verboseToSystem.Should().Be(LogLevel.Trace);

                // Test Debug
                var debugToSystem = ApiLogLevel.Debug.ToSystemLogLevel();
                debugToSystem.Should().Be(LogLevel.Debug);

                // Test Information
                var informationToSystem = ApiLogLevel.Information.ToSystemLogLevel();
                informationToSystem.Should().Be(LogLevel.Information);

                // Test Warning
                var warningToSystem = ApiLogLevel.Warning.ToSystemLogLevel();
                warningToSystem.Should().Be(LogLevel.Warning);

                // Test Error
                var errorToSystem = ApiLogLevel.Error.ToSystemLogLevel();
                errorToSystem.Should().Be(LogLevel.Error);

                // Test Critical
                var criticalToSystem = ApiLogLevel.Critical.ToSystemLogLevel();
                criticalToSystem.Should().Be(LogLevel.Critical);
            }

            [Test]
            public void SerilogToApiConversions_ShouldMapCorrectly()
            {
                // Test specific Serilog to API mappings
                LogEventLevel.Verbose.ToApiLogLevel().Should().Be(ApiLogLevel.Verbose);
                LogEventLevel.Debug.ToApiLogLevel().Should().Be(ApiLogLevel.Debug);
                LogEventLevel.Information.ToApiLogLevel().Should().Be(ApiLogLevel.Information);
                LogEventLevel.Warning.ToApiLogLevel().Should().Be(ApiLogLevel.Warning);
                LogEventLevel.Error.ToApiLogLevel().Should().Be(ApiLogLevel.Error);
                LogEventLevel.Fatal.ToApiLogLevel().Should().Be(ApiLogLevel.Critical);
            }

            [Test]
            public void LogLevelHierarchy_ShouldBeConsistent()
            {
                // Verify that the numeric values create a proper hierarchy
                ((int)ApiLogLevel.Verbose).Should().BeLessThan((int)ApiLogLevel.Debug);
                ((int)ApiLogLevel.Debug).Should().BeLessThan((int)ApiLogLevel.Information);
                ((int)ApiLogLevel.Information).Should().BeLessThan((int)ApiLogLevel.Warning);
                ((int)ApiLogLevel.Warning).Should().BeLessThan((int)ApiLogLevel.Error);
                ((int)ApiLogLevel.Error).Should().BeLessThan((int)ApiLogLevel.Critical);
            }
        }
    }
}